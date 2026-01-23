// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32.SafeHandles;

namespace Pixeval.Caching;

public record MemoryMappedFileCacheHandle(Guid Filename, nint Pointer, SafeMemoryMappedViewHandle ViewHandle);

public class MemoryMappedFileMemoryManager : IDisposable
{
    public CacheToken Token { get; }

    public List<MemoryMappedFileCacheHandle> Handles = [];

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
        foreach (var se in Handles.Select(f => Path.Combine(Token.CacheDirectory, f.Filename.ToString())))
        {
            File.Delete(se);
        }

        foreach (var (filename, ptr, handle) in Handles)
        {
            File.Delete(filename.ToString());
            if (ptr != 0)
            {
                handle.ReleasePointer();
                handle.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }
}
