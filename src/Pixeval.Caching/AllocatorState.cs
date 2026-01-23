// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.Caching;

public enum AllocatorState
{
    AllocationSuccess,
    OutOfMemory,
    AllocatorClosed,
    PersistedCacheNotFound,
    SizeMismatch,
    ReadFailed,
    FailedToAllocateNewRegion,
    UnalignedAllocation,
    AggregateError,
    MMapFailedWithNullPointer
}

public class AllocatorException(AllocatorState state) : Exception(state.ToString())
{
    public AllocatorState State { get; init; } = state;
}
