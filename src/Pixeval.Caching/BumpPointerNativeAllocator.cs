#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/BumpPointerNativeAllocator.cs
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
