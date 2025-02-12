#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/MemoryMappedFileAllocatorManager.cs
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
using System.IO;
using System.Linq;
using Microsoft.Win32.SafeHandles;

namespace Pixeval.Caching;

public record MemoryMappedFileCacheHandle(Guid Filename, nint Pointer, SafeMemoryMappedViewHandle ViewHandle);

public class MemoryMappedFileMemoryManager : IDisposable
{
    public CacheToken Token { get; }

    public List<MemoryMappedFileCacheHandle> Handles = [];

    public MemoryMappedFileMemoryManager(CacheToken token)
    {
        Token = token;
        _ = Directory.CreateDirectory(token.CacheDirectory);
        DelegatedCombinedBumpPointerAllocator = new DelegatedMultipleAllocator(this, BumpPointerAllocators.Values);
    }

    public INativeAllocator DominantAllocator => new MemoryMappedFileAllocator(this);

    public INativeAllocator DelegatedCombinedBumpPointerAllocator { get; }

    /// <summary>
    /// Each BumpPointerAllocator is responsible for a single mmaped file
    /// </summary>
    public Dictionary<Guid, HeapAllocator> BumpPointerAllocators { get; } = [];

    public void Dispose()
    {
        foreach (var se in Handles.Select(f => Path.Combine(Token.CacheDirectory, f.Filename.ToString())))
        {
            File.Delete(se);
        }

        foreach (var (filename, ptr, handle) in Handles)
        {
            File.Delete(filename.ToString());
            if (ptr != 0)
            {
                handle.ReleasePointer();
                handle.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }
}
