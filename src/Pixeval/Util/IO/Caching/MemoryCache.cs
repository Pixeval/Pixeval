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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;

namespace Pixeval.Util.IO.Caching;

public partial class MemoryCache(
    int maxSize,
    ImageSource imageNotAvailable,
    ImageSource pixivNoProfile) : IDisposable
{
    public static async Task<MemoryCache> CreateDefaultAsync(int maxSize)
    {
        var imageNotAvailable = await AppInfo.GetImageNotAvailableStream().DecodeBitmapImageAsync(true);
        var pixivNoProfile = await AppInfo.GetPixivNoProfileStream().DecodeBitmapImageAsync(true);
        return new MemoryCache(maxSize, imageNotAvailable, pixivNoProfile);
    }

    public ImageSource ImageNotAvailable { get; } = imageNotAvailable;

    public ImageSource PixivNoProfile { get; } = pixivNoProfile;

    private readonly Dictionary<string, int> _referenceTimes = [];
    private readonly Dictionary<string, ImageSource> _pool = [];
    private readonly LinkedList<string> _cachedItems = [];

    public void Cache(string key, ImageSource source)
    {
        if (_pool.ContainsKey(key))
            return;
        EnsureCapacity();
        _pool[key] = source;
        _referenceTimes[key] = 0;
        _ = _cachedItems.AddFirst(key);
    }

    public void CacheOrIncrease(string key, ImageSource source)
    {
        if (_pool.ContainsKey(key))
        {
            ++_referenceTimes[key];
            _ = _cachedItems.Remove(key);
        }
        else
        {
            EnsureCapacity();
            _pool[key] = source;
            _referenceTimes[key] = 0;
        }

        _ = _cachedItems.AddFirst(key);
    }

    public bool TryGet(string key, [NotNullWhen(true)] out ImageSource? result)
    {
        if (_pool.TryGetValue(key, out result))
        {
            ++_referenceTimes[key];
            _ = _cachedItems.Remove(key);
            _ = _cachedItems.AddFirst(key);
            return true;
        }

        return false;
    }

    public void ForceRemove(string key)
    {
        _ = _pool.Remove(key);
        _ = _referenceTimes.Remove(key);
        _ = _cachedItems.Remove(key);
    }

    /// <summary>
    /// TODO: 通过内存大小限制来清理缓存
    /// </summary>
    private void EnsureCapacity()
    {
        while (_cachedItems.Count >= maxSize)
        {
            var key = _cachedItems.Last();
            _ = _cachedItems.Remove(key);
            _ = _pool.Remove(key);
            _ = _referenceTimes.Remove(key);
        }

        //if (_referenceTimes.Count >= maxSize)
        //{
        //    var outDatedItems = _referenceTimes.Where(t => t.Value is 0)
        //        .Take(_imagePool.Count - maxSize);
        //    foreach (var (key, _) in outDatedItems)
        //    {
        //        _ = _imagePool.Remove(key);
        //        _ = _streamPool.Remove(key);
        //        _ = _referenceTimes.Remove(key);
        //    }
        //}
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var (_, value) in _pool)
            if (value is IDisposable disposable)
                disposable.Dispose();
        _pool.Clear();
        _referenceTimes.Clear();
        _cachedItems.Clear();
    }
}
