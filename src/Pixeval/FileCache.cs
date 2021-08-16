using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Pixeval
{
    

    public class FileCache
    {
        private readonly Type[] _supportedKeyTypes;
        private readonly string _baseDirectory;
        private const string IndexFileName = "idx.json";
        private Dictionary<Guid, string> _index;
        private string? _indexFile;
        private readonly ReaderWriterLockSlim _indexLocker;
        private const string ExpireIndexFileName = "eidx.json";
        private Dictionary<Guid, DateTimeOffset> _expireIndex;
        private string? _expireIndexFile;
        private readonly ReaderWriterLockSlim _expireIndexLocker;
        private readonly MD5 _md5;

        
        public static FileCache Default { get; private set; }
        public int HitCount { get; private set; }

        static FileCache()
        {
            Default = new FileCache();
        }

        private FileCache(string? cacheDirectory = null)
        {
            _md5 = MD5.Create();
            _supportedKeyTypes = new[] { typeof(int), typeof(uint), typeof(ulong), typeof(long) };
            _baseDirectory = string.IsNullOrEmpty(cacheDirectory) ? Path.Combine(Path.GetTempPath(), Assembly.GetAssembly(typeof(App))!.GetName().Name + "Cache") : cacheDirectory;

            _index = new Dictionary<Guid, string>();
            _indexLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _expireIndex = new Dictionary<Guid, DateTimeOffset>();
            _expireIndexLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            LoadIndex();
            WriteIndex();
        }


        public bool AutoExpire { get; set; }

        public static FileCache Create(string cacheDirectory) =>
            new(cacheDirectory);

        /// <summary>
        /// Adds an entry to the cache
        /// </summary>
        /// <param name="key">Unique identifier for the entry</param>
        /// <param name="data">Data object to store</param>
        /// <param name="expireIn">Time from UtcNow to expire entry in</param>
        /// <param name="eTag">Optional eTag information</param>
        public void Add(Guid key, object data, TimeSpan expireIn, string? eTag = null)
        {
            _indexLocker.EnterWriteLock();
            _expireIndexLocker.EnterWriteLock();
            try
            {
                string? path = Path.Combine(_baseDirectory, key.ToString("N"));
                if (!Directory.Exists(_baseDirectory))
                    Directory.CreateDirectory(_baseDirectory);

                switch (data)
                {
                    case byte[] bytes:
                        File.WriteAllBytes(path, bytes);
                        break;
                    case Stream stream:
                        var fs = File.OpenWrite(path);
                        stream.CopyTo(fs);
                        fs.Dispose();
                        break;
                    default:
                        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(data));
                        break;
                }

                _index[key] = eTag ?? string.Empty;
                _expireIndex[key] = GetExpiration(expireIn);

                WriteIndex();
                WriteExpireIndex();
            }
            finally
            {
                _indexLocker.ExitWriteLock();
                _expireIndexLocker.ExitWriteLock();
            }
        }
        /// <summary>
        /// Adds an entry to the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique identifier for the entry</param>
        /// <param name="data">Data object to store</param>
        /// <param name="expireIn">Time from UtcNow to expire entry in</param>
        /// <param name="eTag">Optional eTag information</param>
        public void Add(object key, object data, TimeSpan expireIn, string? eTag = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(key as string))
                throw new ArgumentException(@"Key can not be null or empty.", nameof(key));
            _indexLocker.EnterWriteLock();
            if (data == null)
                throw new ArgumentNullException(nameof(data), @"Data can not be null.");
            if (key is Guid g)
            {
                Add(g, data, expireIn, eTag);
            }
            if (key is string s)
            {
                Add(Guid.Parse(s), data, expireIn, eTag);
            }

            if (_supportedKeyTypes.Contains(key.GetType()))
            {
                Add(HashToGuid(key), data, expireIn, eTag);
            }

        }
        /// <summary>
        /// Adds an entry to the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique identifier for the entry</param>
        /// <param name="data">Data object to store</param>
        /// <param name="expireIn">Time from UtcNow to expire entry in</param>
        /// <param name="eTag">Optional eTag information</param>
        public void Add(string key, object data, TimeSpan expireIn, string? eTag = null)
        {

            if (string.IsNullOrWhiteSpace(key as string))
                throw new ArgumentException(@"Key can not be null or empty.", nameof(key));
            _indexLocker.EnterWriteLock();
            if (data == null)
                throw new ArgumentNullException(nameof(data), @"Data can not be null.");
            Add(HashToGuid(key), data, expireIn, eTag);

        }

        /// <summary>
        /// Empties all specified entries regardless if they are expired.
        /// Throws an exception if any deletions fail and rolls back changes.
        /// </summary>
        /// <param name="keys">keys to empty</param>
        public void Empty(params object[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (keys.GetType().GetElementType() == typeof(Guid))
            {
                Empty(keys.Cast<Guid>());
            }
            if (keys is string[] strings)
            {
                Empty(strings);
            }
            if (_supportedKeyTypes.Contains(keys.GetType().GetElementType()))
            {
                Empty(keys.Select(HashToGuid).ToArray());
            }
            throw new ArgumentException($"The element type of keys '{keys.GetType().GetElementType()} is not supported.'");
        }

        /// <summary>
        /// Empties all specified entries regardless if they are expired.
        /// Throws an exception if any deletions fail and rolls back changes.
        /// </summary>
        /// <param name="keys">keys to empty</param>
        public void Empty(params string[] keys)
        {
            Empty(keys.Select(HashToGuid).ToArray());
        }
        /// <summary>
        /// Empties all specified entries regardless if they are expired.
        /// Throws an exception if any deletions fail and rolls back changes.
        /// </summary>
        /// <param name="keys">keys to empty</param>
        public void Empty(params Guid[] keys)
        {
            _indexLocker.EnterWriteLock();

            try
            {
                foreach (var k in keys)
                {
                    string? file = Path.Combine(_baseDirectory, HashToGuid(k).ToString("N"));
                    if (File.Exists(file))
                        File.Delete(file);

                    _index.Remove(k);
                }

                WriteIndex();
            }
            finally
            {
                _indexLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Empties all expired entries that are in the cache.
        /// Throws an exception if any deletions fail and rolls back changes.
        /// </summary>
        public void EmptyAll()
        {
            _indexLocker.EnterWriteLock();

            try
            {
                foreach (var item in _index)
                {
                    var guid = HashToGuid(item.Key);
                    string? file = Path.Combine(_baseDirectory, guid.ToString("N"));
                    if (File.Exists(file))
                        File.Delete(file);
                }

                _index.Clear();

                WriteIndex();
            }
            finally
            {
                _indexLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Empties all expired entries that are in the cache.
        /// Throws an exception if any deletions fail and rolls back changes.
        /// </summary>
        public void EmptyExpired()
        {
            _expireIndexLocker.EnterWriteLock();
            _indexLocker.EnterWriteLock();

            try
            {
                var expired = _expireIndex.Where(k => k.Value < DateTimeOffset.Now);

                var toRemove = new List<Guid>();

                foreach (var item in expired)
                {
                    string? file = Path.Combine(_baseDirectory, item.Key.ToString("N"));
                    if (File.Exists(file))
                        File.Delete(file);
                    toRemove.Add(item.Key);
                }

                foreach (var key in toRemove)
                    _index.Remove(key);

                WriteIndex();
                WriteExpireIndex();
            }
            finally
            {
                _indexLocker.ExitWriteLock();
                _expireIndexLocker.ExitWriteLock();
            }
        }
        /// <summary>
        /// Checks to see if the key exists in the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the entry to check</param>
        /// <returns>If the key exists</returns>
        public bool Exists(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key is Guid g)
            {
                return Exists(g);
            }
            if (key is string s)
            {
                return Exists(s);
            }
            if (_supportedKeyTypes.Contains(key.GetType()))
            {
                return Exists(HashToGuid(key));
            }
            throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'");
        }

        /// <summary>
        /// Checks to see if the key exists in the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the entry to check</param>
        /// <returns>If the key exists</returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(@"Key can not be null or empty.", nameof(key));
            return Exists(HashToGuid(key));
        }

        /// <summary>
        /// Checks to see if the key exists in the cache.
        /// </summary>
        /// <param name="key">Unique identifier for the entry to check</param>
        /// <returns>If the key exists</returns>
        public bool Exists(Guid key)
        {
            bool exists;

            _indexLocker.EnterReadLock();

            try
            {
                exists = _index.ContainsKey(key);
            }
            finally
            {
                _indexLocker.ExitReadLock();
            }

            return exists;
        }

        /// <summary>
        /// Gets all the keys that are saved in the cache
        /// </summary>
        /// <returns>The IEnumerable of keys</returns>
        public List<Guid> GetKeys(CacheState state = CacheState.Active)
        {
            _indexLocker.EnterReadLock();

            try
            {
                var bananas = new List<KeyValuePair<Guid, DateTimeOffset>>();

                if (state.HasFlag(CacheState.Active))
                {
                    bananas = _expireIndex
                        .Where(x => x.Value >= DateTimeOffset.Now)
                        .ToList();
                }

                if (state.HasFlag(CacheState.Expired))
                {
                    bananas.AddRange(_expireIndex.Where(x => x.Value < DateTimeOffset.Now));
                }

                return bananas.Select(x => x.Key).ToList();
            }
            catch (Exception)
            {
                return new List<Guid>();
            }
            finally
            {
                _indexLocker.ExitReadLock();
            }
        }

        public T? Get<T>(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key is Guid g)
            {
                return Get<T>(g);
            }

            if (key is string s)
            {
                return Get<T>(s);
            }

            if (_supportedKeyTypes.Contains(key.GetType()))
            {
                return Get<T>(HashToGuid(key));
            }
            throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'");
        }

        public T? Get<T>(string key)
        {
            return Get<T>(HashToGuid(key));
        }

        /// <summary>
        /// Gets the data entry for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for the entry to get</param>
        /// <returns>The data object that was stored if found, else default(T)</returns>
        public T? Get<T>(Guid key)
        {
            var result = default(T);

            _indexLocker.EnterReadLock();

            try
            {
                string? path = Path.Combine(_baseDirectory, key.ToString("N"));

                if (_index.ContainsKey(key) && File.Exists(path) && (!AutoExpire || (AutoExpire && !IsExpired(key))))
                {
                    var bytes = File.ReadAllBytes(path);
                    if (typeof(T)==typeof(Stream)||typeof(T).IsSubclassOf(typeof(Stream)))
                    {
                        result= (T)(object)new MemoryStream(bytes);
                    }
                    else if (typeof(T)==typeof(byte[]))
                    {
                        result = (T)(object)bytes;
                    }
                    else
                    {
                        result = JsonSerializer.Deserialize<T>(bytes);
                    }
                }
            }
            finally
            {
                _indexLocker.ExitReadLock();
            }
            HitCount++;
            Debug.WriteLine(HitCount);
            return result;
        }
        public DateTimeOffset? GetExpiration(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key is Guid g)
            {
                return GetExpiration(g);
            }

            if (key is string s)
            {
                return GetExpiration(s);
            }

            if (_supportedKeyTypes.Contains(key.GetType()))
            {
                return GetExpiration(HashToGuid(key));
            }
            throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'");
        }

        /// <summary>
        /// Gets the DateTime that the item will expire for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The expiration date if the key is found, else null</returns>
        public DateTimeOffset? GetExpiration(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(@"Key can not be null or empty.", nameof(key));
            return GetExpiration(Guid.Parse(key));
        }
        /// <summary>
        /// Gets the DateTime that the item will expire for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The expiration date if the key is found, else null</returns>
        public DateTimeOffset? GetExpiration(Guid key)
        {
            DateTimeOffset? date = null;

            _expireIndexLocker.EnterReadLock();

            try
            {
                if (_expireIndex.ContainsKey(key))
                    date = _expireIndex[key];
            }
            finally
            {
                _indexLocker.ExitReadLock();
            }

            return date;
        }

        /// <summary>
        /// Gets the ETag for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The ETag if the key is found, else null</returns>
        public string? GetETag(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key is Guid g)
            {
                return GetETag(g);
            }

            if (key is string s)
            {
                return GetETag(s);
            }

            if (_supportedKeyTypes.Contains(key.GetType()))
            {
                return GetETag(HashToGuid(key));
            }
            throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'");
        }
        /// <summary>
        /// Gets the ETag for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The ETag if the key is found, else null</returns>
        public string? GetETag(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(@"Key can not be null or empty.", nameof(key));

            return GetETag(HashToGuid(key));
        }
        /// <summary>
        /// Gets the ETag for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for entry to get</param>
        /// <returns>The ETag if the key is found, else null</returns>
        public string? GetETag(Guid key)
        {
            string? etag = null;

            _indexLocker.EnterReadLock();

            try
            {
                if (_index.ContainsKey(key))
                    etag = _index[key];
            }
            finally
            {
                _indexLocker.ExitReadLock();
            }

            return etag;
        }

        /// <summary>
        /// Checks to see if the entry for the key is expired.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>If the expiration data has been met</returns>
        public bool IsExpired(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key is Guid g)
            {
                return IsExpired(g);
            }

            if (key is string s)
            {
                return IsExpired(s);
            }

            if (_supportedKeyTypes.Contains(key.GetType()))
            {
                return IsExpired(HashToGuid(key));
            }
            throw new ArgumentException($"The type of key '{key.GetType()} is not supported.'");
        }
        /// <summary>
        /// Checks to see if the entry for the key is expired.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>If the expiration data has been met</returns>
        public bool IsExpired(string key)
        {
            return IsExpired(HashToGuid(key));
        }

        /// <summary>
        /// Checks to see if the entry for the key is expired.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>If the expiration data has been met</returns>
        public bool IsExpired(Guid key)
        {
            var expired = true;

            _expireIndexLocker.EnterReadLock();

            try
            {
                if (_expireIndex.ContainsKey(key))
                    expired = _expireIndex[key] < DateTimeOffset.Now;
            }
            finally
            {
                _expireIndexLocker.ExitReadLock();
            }

            return expired;
        }

        private void WriteIndex()
        {
            if (string.IsNullOrEmpty(_indexFile))
                _indexFile = Path.Combine(_baseDirectory, IndexFileName);
            if (!Directory.Exists(_baseDirectory))
                Directory.CreateDirectory(_baseDirectory);

            File.WriteAllBytes(_indexFile, JsonSerializer.SerializeToUtf8Bytes(_index));
        }

        private void LoadIndex()
        {
            if (string.IsNullOrEmpty(_indexFile))
                _indexFile = Path.Combine(_baseDirectory, IndexFileName);

            if (!File.Exists(_indexFile))
                return;

            _index = null!;

            byte[] bytes = File.ReadAllBytes(_indexFile);
            _index = JsonSerializer.Deserialize<Dictionary<Guid, string>>(bytes)!;
        }

        private void WriteExpireIndex()
        {
            if (string.IsNullOrEmpty(_expireIndexFile))
                _expireIndexFile = Path.Combine(_baseDirectory, ExpireIndexFileName);
            if (!Directory.Exists(_baseDirectory))
                Directory.CreateDirectory(_baseDirectory);

            File.WriteAllBytes(_expireIndexFile, JsonSerializer.SerializeToUtf8Bytes(_expireIndex));
        }

        private void LoadExpireIndex()
        {
            if (string.IsNullOrEmpty(_expireIndexFile))
                _expireIndexFile = Path.Combine(_baseDirectory, ExpireIndexFileName);

            if (!File.Exists(_expireIndexFile))
                return;

            _expireIndex = null!;

            byte[] bytes = File.ReadAllBytes(_expireIndexFile);
            _expireIndex = JsonSerializer.Deserialize<Dictionary<Guid, DateTimeOffset>>(bytes)!;
        }



        private Guid HashToGuid(object input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input is Guid)
            {
                throw new ArgumentException(@"The input type can't be Guid.", nameof(input));
            }
            switch (input)
            {
                case string str:
                    return new Guid(_md5.ComputeHash(Encoding.UTF8.GetBytes(str)));
                case int i32:
                    byte[] buf1 = new byte[4];
                    MemoryMarshal.Write(buf1, ref i32);
                    return new Guid(_md5.ComputeHash(buf1));
                case uint ui32:
                    byte[] buf2 = new byte[4];
                    MemoryMarshal.Write(buf2, ref ui32);
                    return new Guid(_md5.ComputeHash(buf2));
                case long i64:
                    byte[] buf3 = new byte[4];
                    MemoryMarshal.Write(buf3, ref i64);
                    return new Guid(_md5.ComputeHash(buf3));
                case ulong ui64:
                    byte[] buf4 = new byte[4];
                    MemoryMarshal.Write(buf4, ref ui64);
                    return new Guid(_md5.ComputeHash(buf4));
                case byte[] bytes:
                    return new Guid(_md5.ComputeHash(bytes));
                default:
                    throw new ArgumentException($"The input type '{input.GetType()} is not supported.'");
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
                return DateTimeOffset.Now.Add(timeSpan);
            }
            catch
            {
                if (timeSpan.Milliseconds < 0)
                    return DateTimeOffset.MinValue;

                return DateTimeOffset.MaxValue;
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