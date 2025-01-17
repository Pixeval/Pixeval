#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/CacheTable.cs
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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pixeval.Utilities.Memory;

namespace Pixeval.Caching;

public class CacheTable<TKey, THeader, TProtocol>(
    TProtocol protocol,
    CacheToken token)
    where THeader : unmanaged
    where TKey : IEquatable<TKey>
    where TProtocol : ICacheProtocol<TKey, THeader>
{
    public MemoryMappedFileMemoryManager MemoryManager { get; } = new(token);

    private readonly ConcurrentDictionary<TKey, Lock> _locks = new();

    private readonly Lock _purgeLock = new();

    private Dictionary<TKey, (LinkedListNode<TKey> node, nint ptr, int allocatedLength)> _cacheTable = [];

    private readonly LinkedList<TKey> _lruCacheIndex = [];

    // ReSharper disable once InconsistentNaming
    public int CacheLRUFactor { get; set; } = 2;

    private TProtocol _protocol = protocol;

    /// <summary>
    /// While calling this method, all cache operations should be halted.
    /// </summary>
    private unsafe void PurgeCompact()
    {
        // when purging, all operations must be halt
        lock (_purgeLock)
        {
            var retain = _lruCacheIndex.Count / CacheLRUFactor;

            var garbage = new Dictionary<nint, (TKey, int)>();

            foreach (var key in _lruCacheIndex.Skip(retain))
            {
                if (TryReadCache0(key, out var span, true))
                {
                    var pointer = (byte*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)) - sizeof(THeader);
                    garbage[(nint) pointer] = (key, span.Length);
                }
            }

            var grouped = _cacheTable.Values.GroupBy(
                tuple => MemoryManager.BumpPointerAllocators.First(pair => pair.Value.GetBlock((byte*) tuple.ptr) != null).Value,
                tuple => tuple);
            foreach (var group in grouped)
            {
                var replacement = group.Key.Compact(group.ToDictionary(tuple => tuple.ptr, tuple => tuple.allocatedLength), garbage.Keys.ToHashSet());
                // forward reference
                _cacheTable = _cacheTable.SelectMany(pair =>
                {
                    return replacement.TryGetValue(pair.Value.ptr, out var newPointer)
                        ? new[] { KeyValuePair.Create(pair.Key, (pair.Value.node, newPointer, pair.Value.allocatedLength)) }
                        : new[] { pair };
                }).ToDictionary();
            }

            foreach (var (key, _) in garbage.Values)
            {
                _locks.Remove(key, out _);
                _cacheTable.Remove(key);
            }

            _lruCacheIndex.Skip(retain).ToList().ForEach(g => _lruCacheIndex.Remove(g));
        }
    }

    public AllocatorState TryCache(TKey key, Stream stream)
    {
        return TryCache(key, stream.ReadEnd());
    }

    public AllocatorState TryCache(TKey key, Span<byte> span)
    {
        lock (_purgeLock)
        {
            if (_locks.TryGetValue(key, out var lk))
            {
                lock (lk) return TryCache0(key, span, false);
            }

            return TryCache0(key, span, false);
        }
    }

    private unsafe AllocatorState TryCache0(TKey key, Span<byte> span, bool collected)
    {
        if (_cacheTable.ContainsKey(key))
        {
            return AllocatorState.AllocationSuccess;
        }

        var header = _protocol.SerializeHeader(_protocol.GetHeader(key));

        var result = MemoryManager.DominantAllocator.TryAllocate(header.Length + span.Length, out var cacheArea);
        switch (result)
        {
            case AllocatorState.AllocationSuccess:
                header.CopyTo(cacheArea);
                span.CopyTo(cacheArea[header.Length..]);

                _lruCacheIndex.AddFirst(key);
                _cacheTable[key] = (_lruCacheIndex.First!, (nint) Unsafe.AsPointer(ref cacheArea.GetPinnableReference()), cacheArea.Length);

                _locks[key] = new Lock();
                return AllocatorState.AllocationSuccess;
            case AllocatorState.OutOfMemory when collected:
                return result;
            case AllocatorState.OutOfMemory:
                PurgeCompact();
                return TryCache0(key, span, true);
            default:
                return result;
        }
    }

    public bool TryReadCache(TKey key, out Stream readonlyStream)
    {
        if (TryReadCache(key, out Span<byte> span))
        {
            unsafe
            {
                var pointer =(byte*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
                var newStream = new UnmanagedMemoryStream(pointer, span.Length);
                newStream.Seek(0, SeekOrigin.Begin);
                readonlyStream = newStream;
                return true;
            }
        }

        readonlyStream = null!;
        return false;
    }

    public bool TryReadCache(TKey key, out Span<byte> span)
    {
        lock (_purgeLock)
        {
            if (_locks.TryGetValue(key, out var lk))
            {
                lock (lk) return TryReadCache0(key, out span, false);
            }

            return TryReadCache0(key, out span, false);
        }
    }

    private unsafe bool TryReadCache0(TKey key, out Span<byte> span, bool transparent)
    {
        if (_cacheTable.TryGetValue(key, out var tuple))
        {
            var headerLength = sizeof(THeader);
            var header = new Span<byte>((byte*) tuple.ptr, headerLength);
            var headerStruct = _protocol.DeserializeHeader(header);
            var dataLength = _protocol.GetDataLength(headerStruct);
            var totalSpan = new Span<byte>((void*) tuple.ptr, tuple.allocatedLength);
            span = totalSpan[headerLength..(headerLength + dataLength)];

            if (!transparent)
            {
                _lruCacheIndex.Remove(tuple.node);
                _lruCacheIndex.AddFirst(tuple.node);
            }

            return true;
        }

        span = Span<byte>.Empty;
        return false;
    }
}
