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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Objects.Caching
{
    public class FileCache<T, THash> : IWeakCacheProvider<T, THash>, IEnumerable<KeyValuePair<THash, string>> where T : class
    {
        private readonly Func<T, Stream> cachingPolicy;
        private readonly ConcurrentDictionary<THash, string> fileMapping = new ConcurrentDictionary<THash, string>();
        private readonly string initDirectory;
        private readonly Func<Stream, T> restorePolicy;

        public FileCache(string initDirectory, Func<T, Stream> cachingPolicy, Func<Stream, T> restorePolicy)
        {
            this.cachingPolicy = cachingPolicy;
            this.restorePolicy = restorePolicy;
            this.initDirectory = initDirectory;

            Directory.CreateDirectory(initDirectory);
        }

        public IEnumerator<KeyValuePair<THash, string>> GetEnumerator()
        {
            return fileMapping.GetEnumerator();
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
            var path = Path.Combine(initDirectory, IWeakCacheProvider<T, THash>.HashKey(associateWith) + ".tmp");
            if (!File.Exists(path))
            {
                var s = cachingPolicy(key);
                key = null;
                Task.Run(() =>
                {
                    fileMapping.TryAdd(associateWith, path);
                    WriteFile(path, s);
                });
            }
        }

        public void Detach(THash associateWith)
        {
            using var sem = new SemaphoreSlim(1);
            var path = Path.Combine(initDirectory, IWeakCacheProvider<T, THash>.HashKey(associateWith) + ".");
            if (File.Exists(path))
            {
                fileMapping.TryRemove(associateWith, out _);
                File.Delete(path);
            }
        }

        public async Task<(bool, T)> TryGet([NotNull] THash key)
        {
            if (fileMapping.TryGetValue(key, out var file) && File.Exists(file))
            {
                await using var fileStream = File.OpenRead(file);
                fileStream.Position = 0L;
                await using Stream memoStream = new MemoryStream();
                await fileStream.CopyToAsync(memoStream);
                return (true, restorePolicy(memoStream));
            }

            return (false, null);
        }

        public void Clear()
        {
            using var sem = new SemaphoreSlim(1);
            foreach (var file in Directory.GetFiles(initDirectory))
            {
                File.Delete(file);
            }
        }

        private static async void WriteFile(string path, Stream src)
        {
            await using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            src.Position = 0L;
            await src.CopyToAsync(fileStream);
        }
    }
}