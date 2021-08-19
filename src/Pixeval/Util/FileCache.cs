using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Toolkit.Diagnostics;

namespace Pixeval.Util
{

    public class FileCache
    {
        private readonly Type[] _supportedKeyTypes;

        private const string IndexFileName = "index.json";
        private const string CacheFolderName = "cache";
        private const string ExpireIndexFileName = "eindex.json";

        // The expiration time
        private Dictionary<Guid, DateTimeOffset> _expireIndex;
        private Dictionary<Guid, string> _index;

        private StorageFolder _baseDirectory = null!;
        private StorageFile? _indexFile;
        private StorageFile? _expireIndexFile;

        private readonly SemaphoreSlim _indexLocker;
        private readonly SemaphoreSlim _expireIndexLocker;

        public int HitCount { get; private set; }

        public bool AutoExpire { get; set; }

        public static async Task<FileCache> CreateDefaultAsync()
        {
            var fileCache = new FileCache
            {
                AutoExpire = true, 
                _baseDirectory = await ApplicationData.Current.TemporaryFolder.GetOrCreateFolderAsync(CacheFolderName)
            };
            await fileCache.LoadIndexAsync();
            await fileCache.LoadExpireIndexAsync();
            return fileCache;
        }

        private FileCache()
        {
            _supportedKeyTypes = new[] {typeof(int), typeof(uint), typeof(ulong), typeof(long)};

            _index = new Dictionary<Guid, string>();
            _expireIndex = new Dictionary<Guid, DateTimeOffset>();

            _indexLocker = new SemaphoreSlim(1, 1);
            _expireIndexLocker = new SemaphoreSlim(1, 1);
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

                var file = await _baseDirectory.GetOrCreateFileAsync(key.ToString("N"));
                switch (data)
                {
                    case byte[] bytes:
                        await file.WriteBytesAsync(bytes);
                        break;
                    case IRandomAccessStream stream:
                        await stream.SaveToFile(file);
                        break;
                    default:
                        await file.WriteBytesAsync(JsonSerializer.SerializeToUtf8Bytes(data));
                        break;
                }

                _index[key] = eTag ?? string.Empty;
                _expireIndex[key] = GetExpiration(expireIn);
                Trace.WriteLine(_expireIndex[key]);

                await WriteIndexAsync();
                await WriteExpireIndexAsync();
            }
            finally
            {
                _indexLocker.Release();
            }
        }

        /// <summary>
        /// Adds an entry to the cache
        /// </summary>
        /// <param name="key">Unique identifier for the entry</param>
        /// <param name="data">Data object to store</param>
        /// <param name="expireIn">Time from UtcNow to expire entry in</param>
        /// <param name="eTag">Optional eTag information</param>
        public async Task AddAsync(object key, object data, TimeSpan expireIn, string? eTag = null)
        {
            Guard.IsNotNull(key, nameof(key));
            Guard.IsNotNullOrEmpty(key as string, nameof(key));
            Guard.IsNotNull(data, nameof(data));
            
            await AddAsync(key switch
            {
                Guid g => g,
                string s => Guid.Parse(s),
                _ when _supportedKeyTypes.Contains(key.GetType()) => HashToGuid(key),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            }, data, expireIn, eTag);
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
            Guard.IsNotNull(key, nameof(key));
            Guard.IsNotNull(data, nameof(data));

            return AddAsync(HashToGuid(key), data, expireIn, eTag);
        }

        /// <summary>
        /// Empties all specified entries regardless of whether they're expired or not.
        /// Throws an exception if any deletion fails and rollback changes.
        /// </summary>
        /// <param name="keys">keys to empty</param>
        public Task EmptyAsync(params object[] keys)
        {
            Guard.IsNotNull(keys, nameof(keys));

            var arrElementType = keys.GetType().GetElementType();
            return keys switch
            {
                var k when arrElementType == typeof(Guid) => EmptyAsync(k.Cast<Guid>()),
                string[] arr => EmptyAsync(arr),
                var k when _supportedKeyTypes.Contains(arrElementType) => EmptyAsync(k.Select(HashToGuid)),
                _ => throw new ArgumentException(@$"The element type of keys '{keys.GetType().GetElementType()} is not supported.'")
            };
        }

        /// <summary>
        /// Empties all specified entries regardless if they are expired.
        /// Throws an exception if any deletions fail and rolls back changes.
        /// </summary>
        /// <param name="keys">keys to empty</param>
        public async Task EmptyAsync(params string[] keys)
        {
            await EmptyAsync(keys.Select(HashToGuid));
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
                    (await _baseDirectory.TryGetItemAsync(HashToGuid(k).ToString("N")))?.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    _index.Remove(k);
                }
                await WriteIndexAsync();
            }
            finally
            {
                _indexLocker.Release();
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
                await Task.WhenAll(_index.Select(item => HashToGuid(item.Key))
                    .Select(guid => _baseDirectory.TryGetItemAsync(guid.ToString("N")).AsTask())
                    .Select(item => item.ContinueWith(t => t.Result?.DeleteAsync())));
                _index.Clear();
                await WriteIndexAsync();
            }
            finally
            {
                _indexLocker.Release();
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
                    await (await _baseDirectory.TryGetItemAsync(key.ToString("N")))?.DeleteAsync();
                    _index.Remove(key);
                }

                await WriteIndexAsync();
                await WriteExpireIndexAsync();
            }
            finally
            {
                _indexLocker.Release();
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
                _ => throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'")
            };
        }

        /// <summary>
        /// Checks to see if the key exists in the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the entry to check</param>
        /// <returns>If the key exists</returns>
        public Task<bool> ExistsAsync(string key)
        {
            Guard.IsNotNull(key, nameof(key));
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
                _indexLocker.Release();
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
                    : new List<KeyValuePair<Guid, DateTimeOffset>>();

                if (state.HasFlag(CacheState.Expired))
                {
                    bananas.AddRange(_expireIndex.Where(x => x.Value < DateTimeOffset.Now));
                }

                return bananas.Select(x => x.Key);
            }
            catch
            {
                return Enumerable.Empty<Guid>();
            }
            finally
            {
                _indexLocker.Release();
            }
        }

        public Task<T?> GetAsync<T>(object key)
        {
            Guard.IsNotNull(key, nameof(key));

            return key switch
            {
                Guid g => GetAsync<T>(g),
                string s => GetAsync<T>(s),
                var k when _supportedKeyTypes.Contains(k.GetType()) => GetAsync<T>(HashToGuid(key)),
                _ => throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'")
            };
        }

        public async Task<T?> TryGetAsync<T>(string key)
        {
            return await ExistsAsync(key) ? await GetAsync<T>(key) : default;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            return GetAsync<T>(HashToGuid(key));
        }

        /// <summary>
        /// Gets the data entry for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for the entry to get</param>
        /// <returns>The data object that was stored if found, else default(T)</returns>
        public async Task<T?> GetAsync<T>(Guid key)
        {
            await _indexLocker.WaitAsync();

            try
            {
                var item = await _baseDirectory.TryGetItemAsync(key.ToString("N"));

                if (_index.ContainsKey(key) && item is StorageFile file && (!AutoExpire || AutoExpire && !await IsExpiredAsync(key)))
                {
                    HitCount++;
                    return typeof(T) switch
                    {
                        var type when type == typeof(IRandomAccessStream) || type.IsSubclassOf(typeof(IRandomAccessStream)) => (T) await file.OpenAsync(FileAccessMode.Read),
                        var type when type == typeof(byte[]) || type.IsSubclassOf(typeof(IEnumerable<byte>)) => (T) (object) await file.ReadBytesAsync(),
                        _ => await Functions.Block(async () =>
                        {
                            await using var stream = await file.OpenStreamForReadAsync();
                            return await JsonSerializer.DeserializeAsync<T>(stream);
                        })
                    };
                }
            }
            finally
            {
                _indexLocker.Release();
            }

            throw new KeyNotFoundException(key.ToString());
        }

        public Task<DateTimeOffset?> GetExpirationAsync(object key)
        {
            Guard.IsNotNull(key, nameof(key));

            return key switch
            {
                Guid g => GetExpirationAsync(g),
                string s => GetExpirationAsync(s),
                var k when _supportedKeyTypes.Contains(k.GetType()) => GetExpirationAsync(HashToGuid(key)),
                _ => throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'")
            };
        }

        /// <summary>
        /// Gets the DateTime that the item will expire for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The expiration date if the key is found, else null</returns>
        public Task<DateTimeOffset?> GetExpirationAsync(string key)
        {
            Guard.IsNotNullOrWhiteSpace(key, nameof(key));
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
                _indexLocker.Release();
            }
        }

        /// <summary>
        /// Gets the ETag for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The ETag if the key is found, else null</returns>
        public Task<string?> GetETag(object key)
        {
            Guard.IsNotNull(key, nameof(key));

            return key switch
            {
                Guid g => GetETagAsync(g),
                string s => GetETagAsync(s),
                var k when _supportedKeyTypes.Contains(k.GetType()) => GetETagAsync(HashToGuid(k)),
                _ => throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'")
            };
        }

        /// <summary>
        /// Gets the ETag for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The ETag if the key is found, else null</returns>
        public Task<string?> GetETagAsync(string key)
        {
            Guard.IsNotNullOrWhiteSpace(key, nameof(key));
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
                return _index.TryGetValue(key, out var tag) ? tag : null;
            }
            finally
            {
                _indexLocker.Release();
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
                _ => throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'")
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
                _expireIndexLocker.Release();
            }
        }

        private async Task WriteIndexAsync()
        {
            
            _indexFile ??= await _baseDirectory.CreateFileAsync(IndexFileName, CreationCollisionOption.ReplaceExisting);
            await _indexFile.WriteBytesAsync(JsonSerializer.SerializeToUtf8Bytes(_index));
        }

        private async Task LoadIndexAsync()
        {
            if (await _baseDirectory.TryGetItemAsync(IndexFileName) is StorageFile file)
            {
                _indexFile = file;
                await using var fileStream = await _indexFile.OpenStreamForReadAsync();
                _index = (await JsonSerializer.DeserializeAsync<Dictionary<Guid, string>>(fileStream))!;
            }
        }

        private async Task WriteExpireIndexAsync()
        {
            _expireIndexFile ??= await _baseDirectory.CreateFileAsync(ExpireIndexFileName, CreationCollisionOption.ReplaceExisting);
            await _expireIndexFile.WriteBytesAsync(JsonSerializer.SerializeToUtf8Bytes(_expireIndex));
        }

        private async Task LoadExpireIndexAsync()
        {
            if (await _baseDirectory.TryGetItemAsync(ExpireIndexFileName) is StorageFile file)
            {
                _expireIndexFile = file;
                await using var fileStream = await _indexFile.OpenStreamForReadAsync();
                _expireIndex = (await JsonSerializer.DeserializeAsync<Dictionary<Guid, DateTimeOffset>>(fileStream))!;
            }
        }

        private static Guid HashToGuid(object input)
        {
            Guard.IsNotNull(input, nameof(input));
            Guard.IsNotOfType<Guid>(input, nameof(input));

            return input switch
            {
                string str => new Guid(HashAndTruncateTo128Bit(Encoding.UTF8.GetBytes(str))),
                byte[] bytes => new Guid(HashAndTruncateTo128Bit(bytes)),
                var number and (int or uint or long or ulong) => Functions.Block(() =>
                {
                    Span<byte> span = stackalloc byte[Marshal.SizeOf(number)];
                    Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), number);
                    return new Guid(HashAndTruncateTo128Bit(span));
                }),
                _ => throw new ArgumentException($"The input type '{input.GetType()}' is not supported.")
            };

            static byte[] HashAndTruncateTo128Bit(ReadOnlySpan<byte> span)
            {
                return SHA256.HashData(span)[..16];
            }
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
}