// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.Caching;

/// <summary>
/// All these three functions in the INativeAllocator must be fail-safe, that is, if the result is an error,
/// the function must recover the internal state of the allocator back to the state before the function was called.
/// </summary>
public interface INativeAllocator
{
    AllocatorState TryAllocate(nint size, out Span<byte> span);

    AllocatorState TryAllocateZeroed(nint size, out Span<byte> span);
}
