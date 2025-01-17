#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/HeapAllocator.cs
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pixeval.Utilities;
using Pixeval.Utilities.Memory;
using Void = Pixeval.Utilities.Void;

namespace Pixeval.Caching;

public unsafe record HeapBlock(Memory<byte> Memory, nint UnallocatedStart)
{
    public int Size => Memory.Length;

    public ref byte StartPtr => ref Unsafe.AsRef<byte>((byte*) Memory.Pin().Pointer);

    public ref byte UnallocatedStartPtr => ref Unsafe.AsRef<byte>((byte*) UnallocatedStart);

    public bool RangeContains(ref byte ptr)
    {
        var ptrRawPointer = (byte*) Unsafe.AsPointer(ref ptr);
        var startRawPointer = (byte*) Unsafe.AsPointer(ref StartPtr);
        var end = startRawPointer + Memory.Length;
        return ptrRawPointer >= startRawPointer && ptrRawPointer < end;
    }

    public long RelativeOffset<T>(ref byte ptr) where T : unmanaged
    {
        return Unsafe.ByteOffset(ref ptr, ref StartPtr);
    }

    public ref T AbsoluteOffset<T>(nint offset) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((byte*) Unsafe.AsPointer(ref StartPtr) + offset);
    }

    public ref byte BlockEnd => ref Unsafe.AsRef<byte>((byte*) Unsafe.AsPointer(ref StartPtr) + Memory.Length);

    public long AllocatedSize => Unsafe.ByteOffset(ref StartPtr, ref UnallocatedStartPtr);

    public nint UnallocatedStart { get; set; } = UnallocatedStart;
}

public class HeapAllocator : IDisposable
{
    // 1mb, this will be doubled the first time the allocator allocates.
    private nint _lastGrowthSize = 1 * 1024 * 1024;
    private const double ExpandFactor = 2;

    private readonly LinkedList<HeapBlock> _commitedRegions;
    private readonly Action<HeapBlock>? _callbackOnExpansion;
    private bool _available;
    // This is supposed to be a BumpPointerAllocator, the HeapAllocator runs as if it's allocating system memory.
    private readonly INativeAllocator _allocator;

    public nint Size { get; private set; }

    private HeapAllocator(
        nint size,
        Action<HeapBlock>? callbackOnExpansion,
        bool available,
        INativeAllocator allocator)
    {
        Size = size;
        _callbackOnExpansion = callbackOnExpansion;
        _available = available;
        _allocator = allocator;
        _commitedRegions = [];
    }

    public static HeapAllocator Create(INativeAllocator allocator, Action<HeapBlock>? callback = null)
    {
        return new HeapAllocator(0, callback, true, allocator);
    }

    public unsafe Result<Void> Expand(nint desiredSize, nint align)
    {
        if (!_available)
        {
            return Result<Void>.AsFailure(new AllocatorException(AllocatorState.AllocatorClosed));
        }

        _lastGrowthSize = MemoryHelper.RoundToNearestMultipleOf((nint) (_lastGrowthSize * ExpandFactor), align);

        if (_lastGrowthSize <= desiredSize)
        {
            _lastGrowthSize = MemoryHelper.RoundToNearestMultipleOf((nint) (desiredSize * ExpandFactor), align);
        }

        var result = _allocator.TryAllocate(_lastGrowthSize, out var span);

        switch (result)
        {
            case AllocatorState.AllocationSuccess:
                fixed (byte* elem = &MemoryMarshal.GetReference(span))
                {
                    var region = new HeapBlock(span.AsMemory(), new nint(elem));
                    _commitedRegions.AddLast(region);
                    Size += _lastGrowthSize;
                    _callbackOnExpansion?.Invoke(region);
                }

                break;
            case var _:
                return Result<Void>.AsFailure(new AllocatorException(result));
        }

        return Result<Void>.AsSuccess(Void.Value);
    }

    public Result<(nint ptr, nint actualSize)> Allocate(nint size, nint align)
    {
        if (!_available)
        {
            return Result<(nint ptr, nint actualSize)>.AsFailure(new AllocatorException(AllocatorState.AllocatorClosed));
        }

        var sizeAligned = MemoryHelper.RoundToNearestMultipleOf(size, align);
        var tracker = _commitedRegions.FirstOrDefault(entry => entry.AllocatedSize + sizeAligned <= entry.Size);
        if (tracker != null)
        {
            var padding = MemoryHelper.RoundToNearestMultipleOf(tracker.UnallocatedStart, align);
            tracker.UnallocatedStart = padding;
            var ptr = tracker.UnallocatedStart;
            tracker.UnallocatedStart += sizeAligned;
            return Result<(nint, nint)>.AsSuccess((ptr, sizeAligned));
        }

        if (Expand(sizeAligned, align) is Result<Void>.Failure err)
        {
            return err.ViewAs<Void, (nint ptr, nint actualSize)>();
        }

        return Allocate(size, align);
    }

    public int BlockIndex(HeapBlock block)
    {
        var result = _commitedRegions.Index().FirstOrDefault(b => Unsafe.AreSame(ref block.StartPtr, ref b.Item.StartPtr));
        return result == default ? -1 : result.Index;
    }

    public unsafe HeapBlock? GetBlock(byte* ptr)
    {
        var result = _commitedRegions.FirstOrDefault(block => block.RangeContains(ref Unsafe.AsRef<byte>(ptr)));
        return result;
    }

    public long Allocated()
    {
        return !_available ? 0 : _commitedRegions.Select(block => block.AllocatedSize).Sum();
    }

    public unsafe Dictionary<nint, nint> Compact(Dictionary<nint, int> pointersWithin, ISet<nint> garbage)
    {
        var grouped = pointersWithin.GroupBy(
            tuple => GetBlock((byte*) tuple.Key),
            tuple => tuple);
        var resultDictionary = new Dictionary<nint, nint>();
        foreach (var group in grouped.Where(grp => grp.Key != null))
        {
            var heapBlock = group.Key;
            var span = heapBlock!.Memory.Span;
            var currentUnallocatedStartOffset = 0;
            foreach (var (pointer, allocatedSize) in group.Where(tuple => !garbage.Contains(tuple.Key)))
            {
                var oldContentSpan = new Span<byte>((void*) pointer, allocatedSize);
                // shift the memory block
                oldContentSpan.CopyTo(span[currentUnallocatedStartOffset..(currentUnallocatedStartOffset + allocatedSize)]);
                resultDictionary[pointer] = (nint) (((byte*) Unsafe.AsPointer(ref heapBlock.StartPtr)) + currentUnallocatedStartOffset);
                currentUnallocatedStartOffset += allocatedSize;
            }

            heapBlock.UnallocatedStart = (nint) Unsafe.AsPointer(ref heapBlock.StartPtr) + currentUnallocatedStartOffset;
        }

        return resultDictionary;
    }

    public void Dispose()
    {
        _available = true;
    }
}
