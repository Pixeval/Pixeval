#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/INativeAllocator.cs
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
