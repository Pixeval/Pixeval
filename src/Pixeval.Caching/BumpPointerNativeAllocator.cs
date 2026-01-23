// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;
using System.Runtime.CompilerServices;

namespace Pixeval.Caching;

public unsafe class BumpPointerNativeAllocator(ref byte ptrStart, nint heapSize) : INativeAllocator
{
    public nint BumpingPointer { get; private set; } = (nint) Unsafe.AsPointer(ref ptrStart);

    private readonly byte* _endPointer = (byte*) Unsafe.AsPointer(ref ptrStart) + heapSize;

    public AllocatorState TryAllocate(nint size, out Span<byte> span)
    {
        ref var ptr = ref Unsafe.AsRef<byte>((byte*) BumpingPointer);
        var newAddress = (byte*) BumpingPointer + size;
        if (newAddress >= _endPointer)
        {
            span = [];
            return AllocatorState.OutOfMemory;
        }

        BumpingPointer = (nint) newAddress;
        span = new Span<byte>(Unsafe.AsPointer(ref ptr), (int) size);
        return AllocatorState.AllocationSuccess;
    }

    public AllocatorState TryAllocateZeroed(nint size, out Span<byte> span)
    {
        var result = TryAllocate(size, out var s);
        s.Clear();
        span = s;
        return result;
    }
}
