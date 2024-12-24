#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/FileCache.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using ThrowHelper = WinUI3Utilities.ThrowHelper;

namespace Pixeval.Util.IO.Caching;

public class FileCache
{
    private const string IndexFileName = "index.json";
    private const string CacheFolderName = "cache";
    private const string ExpireIndexFileName = "eindex.json";
    private readonly SemaphoreSlim _expireIndexLocker;

    private readonly SemaphoreSlim _indexLocker;
    private readonly Type[] _supportedKeyTypes;

    private readonly DirectoryInfo _baseDirectory = Directory.CreateDirectory(Path.Combine(AppKnownFolders.Cache.Self.Path, CacheFolderName));

    // The expiration time
    private Dictionary<Guid, DateTimeOffset> _expireIndex;
    private readonly FileInfo _expireIndexFile;
    private Dictionary<Guid, string> _index;
    private readonly FileInfo _indexFile;

    private FileCache()
    {
        _supportedKeyTypes = [typeof(int), typeof(uint), typeof(ulong), typeof(long)];

        _index = [];
        _expireIndex = [];
        _indexFile = new(Path.Combine(_baseDirectory.FullName, IndexFileName));
        _expireIndexFile = new(Path.Combine(_baseDirectory.FullName, ExpireIndexFileName));

        _indexLocker = new SemaphoreSlim(1, 1);
        _expireIndexLocker = new SemaphoreSlim(1, 1);
    }

    public int HitCount { get; private set; }

    public bool AutoExpire { get; set; }

    public static async Task<FileCache> CreateDefaultAsync()
    {
        var fileCache = new FileCache
        {
            AutoExpire = true,
        };
        await fileCache.LoadIndexAsync();
        await fileCache.LoadExpireIndexAsync();
        return fileCache;
    }

    /// <summary>
    /// Adds an entry to the cache
    /// </summary>
    /// <param name="key">Unique identifier for the entry</param>
    /// <param name="data">Data object to store</param>
    /// <param name="expireIn">Time from UtcNow to expire entry in</param>
    /// <param name="eTag">Optional eTag information</param>
    public async Task AddAsync(Guid key, object data, TimeSpan expireIn, string? eTag = null)
    {
        try
        {
            await _indexLocker.WaitAsync();

            var filePath = Path.Combine(_baseDirectory.FullName, key.ToString("N"));
            switch (data)
            {
                case byte[] bytes:
                    await File.WriteAllBytesAsync(filePath, bytes);
                    break;
                case Stream stream:
                {
                    await using var s = IoHelper.OpenAsyncWrite(filePath);
                    await stream.CopyToAsync(s);
                    break;
                }
                default:
                    ThrowHelper.NotSupported("AOT");
                    break;
            }

            _index[key] = eTag ?? "";
            _expireIndex[key] = GetExpiration(expireIn);

            await WriteIndexAsync();
            await WriteExpireIndexAsync();
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Adds an entry to the cache
    /// </summary>
    /// <param name="key">Unique identifier for the entry</param>
    /// <param name="data">Data object to store</param>
    /// <param name="expireIn">Time from UtcNow to expire entry in</param>
    /// <param name="eTag">Optional eTag information</param>
    public Task AddAsync(object key, object data, TimeSpan expireIn, string? eTag = null)
    {
        Guard.IsNotNull(key);
        Guard.IsNotNullOrEmpty(key as string);
        Guard.IsNotNull(data);

        return key switch
        {
            Guid g => AddAsync(g, data, expireIn, eTag),
            string s => AddAsync(Guid.Parse(s), data, expireIn, eTag),
            _ when _supportedKeyTypes.Contains(key.GetType()) => AddAsync(HashToGuid(key), data, expireIn, eTag),
            _ => ThrowHelper.ArgumentOutOfRange<object, Task>(key)
        };
    }

    public async Task<bool> TryAddAsync(string key, object data, TimeSpan expireIn, string? eTag = null)
    {
        if (!await ExistsAsync(key))
        {
            await AddAsync(key, data, expireIn, eTag);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds an entry to the cache
    /// </summary>
    /// <param name="key">Unique identifier for the entry</param>
    /// <param name="data">Data object to store</param>
    /// <param name="expireIn">Time from UtcNow to expire entry in</param>
    /// <param name="eTag">Optional eTag information</param>
    public Task AddAsync(string key, object data, TimeSpan expireIn, string? eTag = null)
    {
        Guard.IsNotNull(key);
        Guard.IsNotNull(data);

        return AddAsync(HashToGuid(key), data, expireIn, eTag);
    }

    /// <summary>
    /// Empties all specified entries regardless of whether they're expired or not.
    /// Throws an exception if any deletion fails and rollback changes.
    /// </summary>
    /// <param name="keys">keys to empty</param>
    public Task EmptyAsync(params object[] keys)
    {
        Guard.IsNotNull(keys);

        var arrElementType = keys.GetType().GetElementType();
        return keys switch
        {
            var k when arrElementType == typeof(Guid) => EmptyAsync(k.Cast<Guid>()),
            string[] arr => EmptyAsync(arr),
            var k when _supportedKeyTypes.Contains(arrElementType) => EmptyAsync(k.Select(HashToGuid)),
            _ => ThrowHelper.Argument<object, Task>($"The element type of keys '{keys.GetType().GetElementType()} is not supported.'")
        };
    }

    /// <summary>
    /// Empties all specified entries regardless if they are expired.
    /// Throws an exception if any deletions fail and rolls back changes.
    /// </summary>
    /// <param name="keys">keys to empty</param>
    public Task EmptyAsync(params string[] keys)
    {
        return EmptyAsync(keys.Select(HashToGuid));
    }

    /// <summary>
    /// Empties all specified entries regardless if they are expired.
    /// Throws an exception if any deletions fail and rolls back changes.
    /// </summary>
    /// <param name="keys">keys to empty</param>
    public async Task EmptyAsync(IEnumerable<Guid> keys)
    {
        await _indexLocker.WaitAsync();

        try
        {
            foreach (var k in keys)
            {
                File.Delete(Path.Combine(_baseDirectory.FullName, HashToGuid(k).ToString("N")));
                _ = _index.Remove(k);
            }

            await WriteIndexAsync();
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Empties all expired entries that are in the cache.
    /// Throws an exception if any deletions fail and rolls back changes.
    /// </summary>
    public async Task EmptyAllAsync()
    {
        await _indexLocker.WaitAsync();

        try
        {
            foreach (var k in _index)
                File.Delete(Path.Combine(_baseDirectory.FullName, HashToGuid(k).ToString("N")));
            _index.Clear();
            await WriteIndexAsync();
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Empties all expired entries that are in the cache.
    /// Throws an exception if any deletions fail and rolls back changes.
    /// </summary>
    public async Task EmptyExpiredAsync()
    {
        await _indexLocker.WaitAsync();

        try
        {
            var expired = _expireIndex.Where(k => k.Value < DateTimeOffset.Now);

            foreach (var (key, _) in expired)
            {
                File.Delete(Path.Combine(_baseDirectory.FullName, key.ToString("N")));
                _ = _index.Remove(key);
            }

            await WriteIndexAsync();
            await WriteExpireIndexAsync();
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Checks to see if the key exists in the cache.
    /// </summary>
    /// <param name="key">Unique identifier for the entry to check</param>
    /// <returns>If the key exists</returns>
    public Task<bool> ExistsAsync(object key)
    {
        Guard.IsNotNull(key, nameof(key));

        return key switch
        {
            Guid g => ExistsAsync(g),
            string s => ExistsAsync(s),
            var k when _supportedKeyTypes.Contains(k.GetType()) => ExistsAsync(HashToGuid(k)),
            _ => ThrowHelper.Argument<object, Task<bool>>($"The type of key '{key.GetType()} is not supported.'")
        };
    }

    /// <summary>
    /// Checks to see if the key exists in the cache.
    /// </summary>
    /// <param name="key">Unique identifier for the entry to check</param>
    /// <returns>If the key exists</returns>
    public Task<bool> ExistsAsync(string key)
    {
        Guard.IsNotNull(key);
        return ExistsAsync(HashToGuid(key));
    }

    /// <summary>
    /// Checks to see if the key exists in the cache.
    /// </summary>
    /// <param name="key">Unique identifier for the entry to check</param>
    /// <returns>If the key exists</returns>
    public async Task<bool> ExistsAsync(Guid key)
    {
        await _indexLocker.WaitAsync();

        try
        {
            return _index.ContainsKey(key);
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Gets all the keys that are saved in the cache
    /// </summary>
    /// <returns>The IEnumerable of keys</returns>
    public async Task<IEnumerable<Guid>> GetKeysAsync(CacheState state = CacheState.Active)
    {
        await _indexLocker.WaitAsync();

        try
        {
            var bananas = state.HasFlag(CacheState.Active)
                ? _expireIndex
                    .Where(x => x.Value >= DateTimeOffset.Now)
                    .ToList()
                : [];

            if (state.HasFlag(CacheState.Expired))
            {
                bananas.AddRange(_expireIndex.Where(x => x.Value < DateTimeOffset.Now));
            }

            return bananas.Select(x => x.Key);
        }
        catch
        {
            return [];
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    public Task<T?> GetAsync<T>(object key) where T : class
    {
        Guard.IsNotNull(key);

        return key switch
        {
            Guid g => GetAsync<T>(g),
            string s => GetAsync<T>(s),
            _ when _supportedKeyTypes.Contains(key.GetType()) => GetAsync<T>(HashToGuid(key)),
            _ => ThrowHelper.Argument<object, Task<T?>>($"The type of key '{key.GetType()} is not supported.'")
        };
    }

    public async Task<T?> TryGetAsync<T>(string key) where T : class
    {
        return await ExistsAsync(key) && !await IsExpiredAsync(key) ? await GetAsync<T>(key) : default;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        return GetAsync<T>(HashToGuid(key));
    }

    /// <summary>
    /// Gets the data entry for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for the entry to get</param>
    /// <returns>The data object that was stored if found, else default(T)</returns>
    public async Task<T?> GetAsync<T>(Guid key) where T : class
    {
        await _indexLocker.WaitAsync();

        try
        {
            var items = _baseDirectory.GetFiles(key.ToString("N"));

            if (_index.ContainsKey(key) && items is [var file] && (!AutoExpire || AutoExpire && !await IsExpiredAsync(key)))
            {
                ++HitCount;
                return typeof(T) switch
                {
                    var type when type == typeof(FileInfo) => (T)(object)file,
                    var type when type == typeof(Stream) || type.IsAssignableTo(typeof(Stream)) => (T)(object)await file.OpenAsyncRead().CopyToMemoryStreamAsync(true),
                    var type when type == typeof(byte[]) => (T)(object)File.ReadAllBytesAsync(file.FullName),
                    _ => ThrowHelper.NotSupported<T>("AOT")
                };
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            _ = _indexLocker.Release();
        }

        return ThrowUtils.KeyNotFound<T>(key.ToString());
    }

    public Task<DateTimeOffset?> GetExpirationAsync(object key)
    {
        Guard.IsNotNull(key);

        return key switch
        {
            Guid g => GetExpirationAsync(g),
            string s => GetExpirationAsync(s),
            _ when _supportedKeyTypes.Contains(key.GetType()) => GetExpirationAsync(HashToGuid(key)),
            _ => ThrowHelper.Argument<object, Task<DateTimeOffset?>>($"The type of key '{key.GetType()} is not supported.'")
        };
    }

    /// <summary>
    /// Gets the DateTime that the item will expire for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for entry to get</param>
    /// <returns>The expiration date if the key is found, else null</returns>
    public Task<DateTimeOffset?> GetExpirationAsync(string key)
    {
        Guard.IsNotNullOrWhiteSpace(key);
        return GetExpirationAsync(Guid.Parse(key));
    }

    /// <summary>
    /// Gets the DateTime that the item will expire for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for entry to get</param>
    /// <returns>The expiration date if the key is found, else null</returns>
    public async Task<DateTimeOffset?> GetExpirationAsync(Guid key)
    {
        await _expireIndexLocker.WaitAsync();

        try
        {
            return _expireIndex.TryGetValue(key, out var date) ? date : null;
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Gets the ETag for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for entry to get</param>
    /// <returns>The ETag if the key is found, else null</returns>
    public Task<string?> GetETag(object key)
    {
        Guard.IsNotNull(key);

        return key switch
        {
            Guid g => GetETagAsync(g),
            string s => GetETagAsync(s),
            var k when _supportedKeyTypes.Contains(k.GetType()) => GetETagAsync(HashToGuid(k)),
            _ => ThrowHelper.Argument<object, Task<string?>>($"The type of key '{key.GetType()} is not supported.'")
        };
    }

    /// <summary>
    /// Gets the ETag for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for entry to get</param>
    /// <returns>The ETag if the key is found, else null</returns>
    public Task<string?> GetETagAsync(string key)
    {
        Guard.IsNotNullOrWhiteSpace(key);
        return GetETagAsync(HashToGuid(key));
    }

    /// <summary>
    /// Gets the ETag for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for entry to get</param>
    /// <returns>The ETag if the key is found, else null</returns>
    public async Task<string?> GetETagAsync(Guid key)
    {
        await _indexLocker.WaitAsync();

        try
        {
            return _index.GetValueOrDefault(key);
        }
        finally
        {
            _ = _indexLocker.Release();
        }
    }

    /// <summary>
    /// Checks to see if the entry for the key is expired.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>If the expiration data has been met</returns>
    public Task<bool> IsExpiredAsync(object key)
    {
        Guard.IsNotNull(key, nameof(key));

        return key switch
        {
            Guid g => IsExpiredAsync(g),
            string s => IsExpiredAsync(s),
            var k when _supportedKeyTypes.Contains(k.GetType()) => IsExpiredAsync(HashToGuid(key)),
            _ => ThrowHelper.Argument<object, Task<bool>>($"The type of key '{key.GetType()} is not supported.'")
        };
    }

    /// <summary>
    /// Checks to see if the entry for the key is expired.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>If the expiration data has been met</returns>
    public Task<bool> IsExpiredAsync(string key)
    {
        return IsExpiredAsync(HashToGuid(key));
    }

    /// <summary>
    /// Checks to see if the entry for the key is expired.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>If the expiration data has been met</returns>
    public async Task<bool> IsExpiredAsync(Guid key)
    {
        await _expireIndexLocker.WaitAsync();

        try
        {
            return !_expireIndex.TryGetValue(key, out var date) || date < DateTimeOffset.Now;
        }
        finally
        {
            _ = _expireIndexLocker.Release();
        }
    }

    private async Task WriteIndexAsync()
    {
        await File.WriteAllBytesAsync(_indexFile.FullName, JsonSerializer.SerializeToUtf8Bytes(_expireIndex, typeof(Dictionary<Guid, string>), IndexContext.Default));
    }

    private async Task LoadIndexAsync()
    {
        if (_indexFile.Exists)
        {
            await using var openRead = _indexFile.OpenAsyncRead();
            _index = (Dictionary<Guid, string>)(await JsonSerializer.DeserializeAsync(openRead, typeof(Dictionary<Guid, string>), IndexContext.Default))!;
        }
    }

    private async Task WriteExpireIndexAsync()
    {
        await File.WriteAllBytesAsync(_expireIndexFile.FullName, JsonSerializer.SerializeToUtf8Bytes(_expireIndex, typeof(Dictionary<Guid, DateTimeOffset>), ExpireIndexContext.Default));
    }

    private async Task LoadExpireIndexAsync()
    {
        if (_expireIndexFile.Exists)
        {
            await using var openRead = _expireIndexFile.OpenAsyncRead();
            _expireIndex = (Dictionary<Guid, DateTimeOffset>)(await JsonSerializer.DeserializeAsync(openRead, typeof(Dictionary<Guid, DateTimeOffset>), ExpireIndexContext.Default))!;
        }
    }

    private static Guid HashToGuid(object input)
    {
        Guard.IsNotNull(input);
        Guard.IsNotOfType<Guid>(input);

        return input switch
        {
            string str => new Guid(HashAndTruncateTo128Bit(Encoding.UTF8.GetBytes(str))),
            byte[] bytes => new Guid(HashAndTruncateTo128Bit(bytes)),
            var number and (int or uint) => Functions.Block(() =>
            {
                Span<byte> span = stackalloc byte[sizeof(int)];
                Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), number);
                return new Guid(HashAndTruncateTo128Bit(span));
            }),
            var number and (long or ulong) => Functions.Block(() =>
            {
                Span<byte> span = stackalloc byte[sizeof(long)];
                Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), number);
                return new Guid(HashAndTruncateTo128Bit(span));
            }),
            _ => ThrowHelper.Argument<object, Guid>($"The input type '{input.GetType()}' is not supported.")
        };

        static byte[] HashAndTruncateTo128Bit(ReadOnlySpan<byte> span) => SHA256.HashData(span)[..16];
    }

    /// <summary>
    /// Gets the expiration from a timespan
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    internal static DateTimeOffset GetExpiration(TimeSpan timeSpan)
    {
        try
        {
            return DateTimeOffset.Now + timeSpan;
        }
        catch
        {
            return timeSpan.Milliseconds < 0 ? DateTimeOffset.MinValue : DateTimeOffset.MaxValue;
        }
    }
}

/// <summary>
/// Current state of the item in the cache.
/// </summary>
[Flags]
public enum CacheState
{
    /// <summary>
    /// An unknown state for the cache item
    /// </summary>
    None = 0,

    /// <summary>
    /// Expired cache item
    /// </summary>
    Expired = 1,

    /// <summary>
    /// Active non-expired cache item
    /// </summary>
    Active = 2
}

[JsonSerializable(typeof(Dictionary<Guid, string>))]
internal partial class IndexContext : JsonSerializerContext;

[JsonSerializable(typeof(Dictionary<Guid, DateTimeOffset>))]
internal partial class ExpireIndexContext : JsonSerializerContext;
