#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/AllocatorState.cs
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
