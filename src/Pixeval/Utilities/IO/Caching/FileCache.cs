// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Utilities.IO.Caching;

internal sealed class FileCache(string cacheDirectory)
{
    private const string CacheFileExtension = ".cache";
    private const string TempFileExtension = ".tmp";
    private const int CacheCopyBufferSize = 81920;
    private const int CacheFileLockCount = 64;

    private long _totalCacheSize;
    private bool _sizeInitialized;
    private bool _cacheMaintenanceInProgress;
    private readonly Lock _cacheStateLock = new();
    private readonly SemaphoreSlim _cacheMaintenanceSemaphore = new(1, 1);

    private readonly Lock[] _cacheFileLocks =
        [.. Enumerable.Repeat(0, CacheFileLockCount).Select(static _ => new Lock())];

    private int _cacheDirectoryCleaned;

    public string CacheDirectory { get; } = cacheDirectory;

    public Stream? TryOpen(string key)
    {
        try
        {
            if (!TryEnsureCacheDirectory())
                return null;

            var path = GetCacheFilePath(key);
            if (!File.Exists(path))
                return null;

            _ = FileHelper.TryTouchFileAccessTime(path);
            return FileHelper.OpenRead(path, FileShare.ReadWrite | FileShare.Delete, CacheCopyBufferSize,
                FileOptions.SequentialScan);
        }
        catch
        {
            return null;
        }
    }

    public FileCacheWriteResult TryCache(string key, Stream stream, long? sizeLimitBytes)
    {
        if (!TryEnsureCacheDirectory())
            return FileCacheWriteResult.Failed;

        lock (_cacheStateLock)
        {
            if (_cacheMaintenanceInProgress)
                return FileCacheWriteResult.Failed;
        }

        try
        {
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
                    var fileInfo = new FileInfo(tempPath);
                    var fileSize = fileInfo.Length;

                    lock (_cacheStateLock)
                    {
                        if (_cacheMaintenanceInProgress)
                        {
                            _ = FileHelper.TryDeleteFile(tempPath);
                            return FileCacheWriteResult.Failed;
                        }

                        if (File.Exists(path))
                        {
                            _ = FileHelper.TryDeleteFile(tempPath);
                            _ = FileHelper.TryTouchFile(path);
                            return FileCacheWriteResult.Success;
                        }

                        FileHelper.Move(tempPath, path);
                        if (_sizeInitialized)
                            _totalCacheSize += fileSize;
                    }

                    if (sizeLimitBytes is { } maxBytes)
                        _ = EnforceSizeLimitAsync(maxBytes);

                    return FileCacheWriteResult.Success;
                }
                catch (IOException) when (File.Exists(path))
                {
                    _ = FileHelper.TryDeleteFile(tempPath);
                    _ = FileHelper.TryTouchFile(path);
                    lock (_cacheStateLock)
                        _sizeInitialized = false;
                    return FileCacheWriteResult.Success;
                }
                catch
                {
                    _ = FileHelper.TryDeleteFile(tempPath);
                    return FileCacheWriteResult.Failed;
                }
            }
        }
        catch
        {
            return FileCacheWriteResult.Failed;
        }
    }

    public Task PurgeAsync(CancellationToken token = default) =>
        RunCacheMaintenanceAsync(Purge, token);

    private void Purge()
    {
        try
        {
            if (!Directory.Exists(CacheDirectory))
            {
                _ = FileHelper.TryCreateDirectory(CacheDirectory);
                return;
            }

            var files = FileHelper.EnumerateFiles(CacheDirectory).ToArray();
            var directories = FileHelper.EnumerateDirectories(CacheDirectory).ToArray();

            foreach (var file in files)
                _ = FileHelper.TryDeleteFile(file);

            foreach (var directory in directories)
                _ = FileHelper.TryDeleteDirectory(directory);

            _ = FileHelper.TryCreateDirectory(CacheDirectory);
        }
        catch
        {
            // Cache cleanup is best effort.
        }
        finally
        {
            lock (_cacheStateLock)
            {
                _sizeInitialized = false;
                _totalCacheSize = 0;
            }
        }
    }

    public Task EnforceSizeLimitAsync(long maxBytes, CancellationToken token = default) =>
        RunCacheMaintenanceAsync(() => EnforceSizeLimit(maxBytes), token);

    private void EnforceSizeLimit(long maxBytes)
    {
        if (!TryEnsureCacheDirectory())
            return;

        try
        {
            var fileInfos = new List<(string Path, long Length, DateTime LastAccess, DateTime LastWrite)>();
            if (!TryEnumerateCacheFiles(out var filePaths))
                return;

            foreach (var filePath in filePaths)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                        fileInfos.Add((fileInfo.FullName, fileInfo.Length, fileInfo.LastAccessTimeUtc,
                            fileInfo.LastWriteTimeUtc));
                }
                catch
                {
                    // The cache is best effort; ignore files that disappear during inspection.
                }
            }

            var currentTotal = fileInfos.Sum(static file => file.Length);
            if (currentTotal > maxBytes)
            {
                foreach (var file in fileInfos.OrderBy(static file => file.LastAccess)
                             .ThenBy(static file => file.LastWrite))
                {
                    if (currentTotal <= maxBytes)
                        break;

                    if (FileHelper.TryDeleteFile(file.Path))
                        currentTotal -= file.Length;
                }
            }

            lock (_cacheStateLock)
            {
                _totalCacheSize = currentTotal;
                _sizeInitialized = true;
            }
        }
        catch
        {
            // Cache eviction must not affect image loading or app startup.
        }
    }

    private bool TryEnumerateCacheFiles(out IReadOnlyList<string> filePaths)
    {
        try
        {
            filePaths = Directory.GetFiles(CacheDirectory, $"*{CacheFileExtension}", SearchOption.TopDirectoryOnly);
            return true;
        }
        catch
        {
            filePaths = [];
            return false;
        }
    }

    private bool TryEnsureCacheDirectory()
    {
        if (!FileHelper.TryCreateDirectory(CacheDirectory))
            return false;

        if (Interlocked.Exchange(ref _cacheDirectoryCleaned, 1) is not 0)
            return true;

        foreach (var file in FileHelper.EnumerateFiles(CacheDirectory))
        {
            var extension = Path.GetExtension(file);
            if (extension is TempFileExtension
                || !string.Equals(extension, CacheFileExtension, StringComparison.OrdinalIgnoreCase))
                _ = FileHelper.TryDeleteFile(file);
        }

        return true;
    }

    private async Task RunCacheMaintenanceAsync(Action action, CancellationToken token)
    {
        await _cacheMaintenanceSemaphore.WaitAsync(token).ConfigureAwait(false);
        try
        {
            lock (_cacheStateLock)
            {
                _cacheMaintenanceInProgress = true;
                _sizeInitialized = false;
            }

            await Task.Run(action, token).ConfigureAwait(false);
        }
        finally
        {
            lock (_cacheStateLock)
                _cacheMaintenanceInProgress = false;
            _cacheMaintenanceSemaphore.Release();
        }
    }

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
