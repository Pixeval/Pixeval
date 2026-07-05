// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using Pixeval.Utilities;
using Void = Pixeval.Utilities.Void;

namespace Pixeval.Caching;

// Manages all memory mapped files
public unsafe class MemoryMappedFileAllocator(MemoryMappedFileMemoryManager manager) : INativeAllocator
{
    public AllocatorState TryAllocate(nint size, out Span<byte> span)
    {
        if (manager.IsDisposed)
        {
            span = [];
            return AllocatorState.AllocatorClosed;
        }

        if (manager.DelegatedCombinedBumpPointerAllocator.TryAllocate(size, out var s) is AllocatorState.AllocationSuccess)
        {
            span = s;
            return AllocatorState.AllocationSuccess;
        }

        // If we must expand.

        if (manager.BumpPointerAllocators.Count >= manager.Token.MaxMemoryMappedFileCount)
        {
            span = [];
            return AllocatorState.OutOfMemory;
        }

        var fileName = Guid.NewGuid();
        var path = manager.GetCacheFilePath(fileName);
        MemoryMappedFile? mmf = null;
        MemoryMappedViewAccessor? accessor = null;
        byte* ptr = null;

        try
        {
            mmf = MemoryMappedFile.CreateFromFile(path, FileMode.OpenOrCreate, null, manager.Token.MemoryMappedFileInitialSize, MemoryMappedFileAccess.ReadWrite);
            accessor = mmf.CreateViewAccessor();
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        }
        catch
        {
            accessor?.Dispose();
            mmf?.Dispose();
            File.Delete(path);
            span = [];
            return AllocatorState.FailedToAllocateNewRegion;
        }

        if (ptr == null)
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            accessor.Dispose();
            mmf.Dispose();
            File.Delete(path);
            span = [];
            return AllocatorState.MMapFailedWithNullPointer;
        }

        manager.Handles.Add(new MemoryMappedFileCacheHandle(fileName, (nint) ptr, mmf, accessor));
        manager.BumpPointerAllocators[fileName] = HeapAllocator.Create(new BumpPointerNativeAllocator(ref Unsafe.AsRef<byte>(ptr), manager.Token.MemoryMappedFileInitialSize));

        var result = manager.DelegatedCombinedBumpPointerAllocator.TryAllocate(size, out var s2);
        switch (result)
        {
            case AllocatorState.AllocationSuccess:
                span = s2;
                return AllocatorState.AllocationSuccess;
            case var _:
                span = [];
                return result;
        }
    }

    // Avoid using this ! The Unsafe.InitBlockUnaligned causes the entire file 
    // to be copied into RAM.
    public AllocatorState TryAllocateZeroed(nint size, out Span<byte> span)
    {
        var result = TryAllocate(size, out var s);
        s.Clear();
        span = s;
        return AllocatorState.AllocationSuccess;
    }

    // this allows us to free one of the memory mapped files
    public Result<Void> Free(nint ptr)
    {
        var memoryMappedFileCacheHandle = manager.Handles.FirstOrDefault(cacheHandle => cacheHandle.Pointer == ptr);

        if (memoryMappedFileCacheHandle is null)
        {
            return Result<Void>.AsFailure(new AllocatorException(AllocatorState.ReadFailed));
        }

        manager.Free(memoryMappedFileCacheHandle);

        return Result<Void>.AsSuccess(Void.Value);
    }
}
