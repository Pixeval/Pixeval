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
    
    private long _totalCacheSize;
    private bool _sizeInitialized;
    private readonly object _sizeInitLock = new();

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

        // 确保计数器初始化完成，避免重复计数（问题3）
        EnsureSizeInitialized();

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
                    FileHelper.Move(tempPath, path);
                    Interlocked.Add(ref _totalCacheSize, fileSize);

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
        catch
        {
            return FileCacheWriteResult.Failed;
        }
    }

    public void Purge()
    {
        try
        {
            lock (_sizeInitLock)
            {
                // 重置计数器并标记未初始化，保证后续重新扫描
                _sizeInitialized = false;
                _totalCacheSize = 0;

                if (!Directory.Exists(CacheDirectory))
                {
                    _ = FileHelper.TryCreateDirectory(CacheDirectory);
                    return;
                }

                var files = FileHelper.EnumerateFiles(CacheDirectory);
                var directories = FileHelper.EnumerateDirectories(CacheDirectory);

                foreach (var file in files)
                    _ = FileHelper.TryDeleteFile(file);

                foreach (var directory in directories)
                    _ = FileHelper.TryDeleteDirectory(directory);

                _ = FileHelper.TryCreateDirectory(CacheDirectory);
            }
        }
        catch
        {
            // Cache cleanup is best effort.
        }
    }

    public void EnforceSizeLimit(long maxBytes)
    {
        if (!TryEnsureCacheDirectory())
            return;
        
        EnsureSizeInitialized();
        if (Interlocked.Read(ref _totalCacheSize) <= maxBytes)
            return;

        // 使用锁防止并发淘汰，保证计数器更新原子性
        lock (_sizeInitLock)
        {
            // 双重检查，避免其他线程已执行淘汰
            if (Interlocked.Read(ref _totalCacheSize) <= maxBytes)
                return;

            // 收集缓存文件信息快照，避免后续访问 FileInfo 时文件被删除导致异常
            var fileInfos = new List<(string path, long length, DateTime lastAccess, DateTime lastWrite)>();
            foreach (var filePath in FileHelper.EnumerateFiles(CacheDirectory, $"*{CacheFileExtension}"))
            {
                try
                {
                    var fi = new FileInfo(filePath);
                    if (fi.Exists)
                    {
                        fileInfos.Add((fi.FullName, fi.Length, fi.LastAccessTimeUtc, fi.LastWriteTimeUtc));
                    }
                }
                catch
                {
                    // 忽略已删除或无法访问的文件
                }
            }

            var currentTotal = Interlocked.Read(ref _totalCacheSize);
            if (currentTotal <= maxBytes)
                return;

            // 按最后访问时间升序淘汰（最早访问的最先被删除）
            foreach (var item in fileInfos.OrderBy(i => i.lastAccess).ThenBy(i => i.lastWrite))
            {
                if (currentTotal <= maxBytes)
                    break;

                if (FileHelper.TryDeleteFile(item.path))
                {
                    currentTotal -= item.length;
                    Interlocked.Add(ref _totalCacheSize, -item.length);  // 同步扣减全局计数器
                }
            }
        }
    }
    
    private void EnsureSizeInitialized()
    {
        if (_sizeInitialized) return;
        lock (_sizeInitLock)
        {
            if (_sizeInitialized) return;

            long total = 0;
            foreach (var filePath in FileHelper.EnumerateFiles(CacheDirectory, $"*{CacheFileExtension}"))
            {
                try
                {
                    var fi = new FileInfo(filePath);
                    if (fi.Exists)
                        total += fi.Length;
                }
                catch
                {
                    // 文件可能在枚举过程中被删除，忽略该文件
                }
            }

            _totalCacheSize = total;
            _sizeInitialized = true;
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
