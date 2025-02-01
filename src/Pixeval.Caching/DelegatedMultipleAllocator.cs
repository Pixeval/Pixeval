#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/DelegatedMultipleAllocator.cs
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
