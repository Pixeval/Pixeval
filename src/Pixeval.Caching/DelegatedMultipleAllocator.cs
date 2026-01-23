// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Pixeval.Utilities;

namespace Pixeval.Caching;

public unsafe class DelegatedMultipleAllocator(MemoryMappedFileMemoryManager manager, IEnumerable<HeapAllocator> allocators) : INativeAllocator
{
    public AllocatorState TryAllocate(nint size, out Span<byte> span)
    {
        foreach (var nativeAllocator in allocators)
        {
            if (nativeAllocator.Allocate(size, manager.Token.Align) is Result<(nint ptr, nint actualSize)>.Success(var (ptr, actualSize)))
            {
                span = new Span<byte>((byte*) ptr, (int) actualSize);
                return AllocatorState.AllocationSuccess;
            }
        }

        span = [];
        return AllocatorState.AggregateError;
    }

    public AllocatorState TryAllocateZeroed(nint size, out Span<byte> span)
    {
        var result = TryAllocate(size, out var s);
        s.Clear();
        span = s;
        return result;
    }
}
