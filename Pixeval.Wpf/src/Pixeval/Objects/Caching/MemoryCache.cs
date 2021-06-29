#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Objects.Caching
{
    public class MemoryCache<T, THash> : IWeakCacheProvider<T, THash>, IEnumerable<KeyValuePair<int, WeakEntry<T>>> where T : class
    {
        public static readonly MemoryCache<T, THash> Shared = new MemoryCache<T, THash>();

        private readonly ConcurrentDictionary<int, WeakEntry<T>> cache = new ConcurrentDictionary<int, WeakEntry<T>>();

        public IEnumerator<KeyValuePair<int, WeakEntry<T>>> GetEnumerator()
        {
            return cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Attach(ref T key, THash associateWith)
        {
            if (associateWith == null || key == null)
            {
                return;
            }
            var weakRef = new WeakEntry<T>(key);
            key = null;
            cache.TryAdd(IWeakCacheProvider<T, THash>.HashKey(associateWith), weakRef);
        }

        public void Detach(THash associateWith)
        {
            cache.TryRemove(IWeakCacheProvider<T, THash>.HashKey(associateWith), out _);
        }

        public Task<(bool, T)> TryGet(THash key)
        {
            return cache.TryGetValue(IWeakCacheProvider<T, THash>.HashKey(key), out var weakRef) && weakRef.Target is { } target ? Task.FromResult((true, target)) : Task.FromResult((false, (T) null));
        }

        public void Clear()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }
    }
}