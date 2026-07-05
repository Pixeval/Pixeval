// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Pixeval.Utilities.IO.Caching;

internal sealed class FileCache(string cacheDirectory)
{
    private const string CacheFileExtension = ".cache";
    private const string TempFileExtension = ".tmp";
    private const int CacheCopyBufferSize = 81920;
    private const int CacheFileLockCount = 64;

    private readonly Lock[] _cacheFileLocks =
        [.. Enumerable.Repeat(0, CacheFileLockCount).Select(static _ => new Lock())];

    private int _cacheDirectoryCleaned;

    public string CacheDirectory { get; } = cacheDirectory;

    public Stream? TryOpen(string key)
    {
        EnsureCacheDirectory();

        var path = GetCacheFilePath(key);
        if (!File.Exists(path))
            return null;

        try
        {
            _ = FileHelper.TryTouchFileAccessTime(path);
            return FileHelper.OpenRead(path, FileShare.ReadWrite | FileShare.Delete, CacheCopyBufferSize,
                FileOptions.SequentialScan);
        }
        catch (FileNotFoundException)
        {
            // Cache was evicted between the existence check and opening the stream.
        }
        catch (DirectoryNotFoundException)
        {
            // Cache directory was cleared while opening the stream.
        }

        return null;
    }

    public FileCacheWriteResult TryCache(string key, Stream stream, long? sizeLimitBytes)
    {
        EnsureCacheDirectory();

        var path = GetCacheFilePath(key);
        lock (GetCacheFileLock(path))
        {
            if (File.Exists(path))
            {
                _ = FileHelper.TryTouchFileAccessTime(path);
                return FileCacheWriteResult.Success;
            }

            var tempPath = GetTemporaryCacheFilePath(path);
            try
            {
                using (var fileStream = FileHelper.CreateWriteCreateParent(tempPath, CacheCopyBufferSize,
                           FileOptions.SequentialScan))
                {
                    FileHelper.CopyWholeStream(stream, fileStream, CacheCopyBufferSize);
                    fileStream.Flush(true);
                }

                if (sizeLimitBytes is { } limit && new FileInfo(tempPath).Length > limit)
                {
                    _ = FileHelper.TryDeleteFile(tempPath);
                    return FileCacheWriteResult.TooLarge;
                }

                _ = FileHelper.TryTouchFile(tempPath, DateTime.UtcNow);
                FileHelper.Move(tempPath, path);

                if (sizeLimitBytes is { } maxBytes)
                    EnforceSizeLimit(maxBytes);

                return FileCacheWriteResult.Success;
            }
            catch (IOException) when (File.Exists(path))
            {
                _ = FileHelper.TryDeleteFile(tempPath);
                _ = FileHelper.TryTouchFile(path);
                return FileCacheWriteResult.Success;
            }
            catch
            {
                _ = FileHelper.TryDeleteFile(tempPath);
                return FileCacheWriteResult.Failed;
            }
        }
    }

    public void Purge()
    {
        if (!Directory.Exists(CacheDirectory))
        {
            _ = Directory.CreateDirectory(CacheDirectory);
            return;
        }

        var files = FileHelper.EnumerateFiles(CacheDirectory);
        var directories = FileHelper.EnumerateDirectories(CacheDirectory);

        foreach (var file in files)
            _ = FileHelper.TryDeleteFile(file);

        foreach (var directory in directories)
            _ = FileHelper.TryDeleteDirectory(directory);

        _ = Directory.CreateDirectory(CacheDirectory);
    }

    public void EnforceSizeLimit(long maxBytes)
    {
        EnsureCacheDirectory();

        var files = EnumerateCacheFiles().ToArray();
        var totalBytes = files.Sum(static file => file.Length);
        if (totalBytes <= maxBytes)
            return;

        foreach (var file in files.OrderBy(static file => file.LastAccessTimeUtc)
                     .ThenBy(static file => file.LastWriteTimeUtc))
        {
            if (totalBytes <= maxBytes)
                return;

            var length = file.Length;
            if (FileHelper.TryDeleteFile(file.FullName))
                totalBytes -= length;
        }
    }

    private void EnsureCacheDirectory()
    {
        _ = Directory.CreateDirectory(CacheDirectory);

        if (Interlocked.Exchange(ref _cacheDirectoryCleaned, 1) is not 0)
            return;

        foreach (var file in FileHelper.EnumerateFiles(CacheDirectory))
        {
            var extension = Path.GetExtension(file);
            if (extension is TempFileExtension
                || !string.Equals(extension, CacheFileExtension, StringComparison.OrdinalIgnoreCase))
                _ = FileHelper.TryDeleteFile(file);
        }
    }

    private IEnumerable<FileInfo> EnumerateCacheFiles() =>
        FileHelper.EnumerateFiles(CacheDirectory, $"*{CacheFileExtension}")
            .Select(static file => new FileInfo(file))
            .Where(static file => file.Exists);

    private string GetCacheFilePath(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(CacheDirectory, $"{Convert.ToHexString(hash)}{CacheFileExtension}");
    }

    private string GetTemporaryCacheFilePath(string cacheFilePath) =>
        Path.Combine(CacheDirectory, $"{Path.GetFileName(cacheFilePath)}.{Guid.NewGuid():N}{TempFileExtension}");

    private Lock GetCacheFileLock(string path) =>
        _cacheFileLocks[(path.GetHashCode() & int.MaxValue) % _cacheFileLocks.Length];
}
