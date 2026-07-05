// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace Pixeval.Caching;

public sealed record MemoryMappedFileCacheHandle(
    Guid Filename,
    nint Pointer,
    MemoryMappedFile File,
    MemoryMappedViewAccessor Accessor) : IDisposable
{
    private bool _isDisposed;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        Accessor.Dispose();
        File.Dispose();
    }
}

public sealed class MemoryMappedFileMemoryManager : IDisposable
{
    public CacheToken Token { get; }

    public List<MemoryMappedFileCacheHandle> Handles = [];

    public bool IsDisposed { get; private set; }

    public MemoryMappedFileMemoryManager(CacheToken token)
    {
        Token = token;
        _ = Directory.CreateDirectory(token.CacheDirectory);
        DelegatedCombinedBumpPointerAllocator = new DelegatedMultipleAllocator(this, BumpPointerAllocators.Values);
    }

    public INativeAllocator DominantAllocator => new MemoryMappedFileAllocator(this);

    public INativeAllocator DelegatedCombinedBumpPointerAllocator { get; }

    /// <summary>
    /// Each BumpPointerAllocator is responsible for a single mmaped file
    /// </summary>
    public Dictionary<Guid, HeapAllocator> BumpPointerAllocators { get; } = [];

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;

        foreach (var allocator in BumpPointerAllocators.Values)
            allocator.Dispose();

        BumpPointerAllocators.Clear();

        foreach (var handle in Handles.ToList())
            Free(handle);
    }

    internal string GetCacheFilePath(Guid filename) => Path.Combine(Token.CacheDirectory, filename.ToString());

    internal void Free(MemoryMappedFileCacheHandle handle)
    {
        _ = Handles.Remove(handle);
        _ = BumpPointerAllocators.Remove(handle.Filename);
        handle.Dispose();
        File.Delete(GetCacheFilePath(handle.Filename));
    }
}
