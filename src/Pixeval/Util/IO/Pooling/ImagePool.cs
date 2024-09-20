#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/BinaryPool.cs
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
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Util.IO.Pooling;

public class ImagePool<TKey>(int maxSize) : IDisposable
{
    private record ImagePoolEntry(TKey Key, int ReferenceTimes);

    private readonly Dictionary<ImagePoolEntry, ImageSource> _pool = new();
    private readonly LinkedList<TKey> _cachedItems = new();

    public void Cache(TKey key, ImageSource source)
    {
        if (_cachedItems.Contains(key))
            return;
        EnsureCapacity();
        _pool.Add(new ImagePoolEntry(key, 0), source);
        _cachedItems.AddLast(key);
    }

    public bool TryGet(TKey key, out ImageSource result)
    {
        if (!_cachedItems.Contains(key))
        {
            result = null!;
            return false;
        }
         
        var entry = _pool.FirstOrDefault(x => x.Key.Key?.Equals(key) ?? false);
        _cachedItems.Remove(entry.Key.Key);
        _cachedItems.AddFirst(entry.Key.Key);
        result = entry.Value;
        return true;
    }

    public void Remove(TKey key)
    {
        var entry = _pool.FirstOrDefault(x => x.Key.Key?.Equals(key) ?? false);
        _pool.Remove(entry.Key);
        _cachedItems.Remove(key);
    }

    private void EnsureCapacity()
    {
        if (_pool.Count >= maxSize)
            while (_pool.Count >= maxSize)
                _pool.Remove(_pool.Keys.Last());
    }

    public void Dispose()
    {
        foreach (var (_, value) in _pool)
            if (value is IDisposable disposable)
                disposable.Dispose();
    }
}
