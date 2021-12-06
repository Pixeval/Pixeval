#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DownloadHistoryPersistentManager.cs
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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pixeval.Download;
using SQLite;

namespace Pixeval.Database.Managers
{
    public class DownloadHistoryPersistentManager : IPersistentManager<DownloadHistoryEntry, ObservableDownloadTask>
    {
        public SQLiteAsyncConnection Connection { get; init; } = null!;

        public int MaximumRecords { get; set; }

        public async Task InsertAsync(DownloadHistoryEntry t)
        {
            if (await Connection.Table<DownloadHistoryEntry>().CountAsync() > MaximumRecords)
            {
                await Purge(MaximumRecords);
            }
            await Connection.InsertAsync(t);
            t.PropertyChanged += async (_, _) =>
            {
                if ((await RawDataAsync()).Any(entry => entry.Destination == t.Destination))
                {
                    await Connection.UpdateAsync(t);
                }
            };
        }

        public async Task<IEnumerable<ObservableDownloadTask>> QueryAsync(Func<AsyncTableQuery<DownloadHistoryEntry>, AsyncTableQuery<DownloadHistoryEntry>> action)
        {
            return (await action(Connection.Table<DownloadHistoryEntry>()).ToListAsync()).Select(ToObservableDownloadTask);
        }

        public async Task<IEnumerable<ObservableDownloadTask>> SelectAsync(Expression<Func<DownloadHistoryEntry, bool>>? predicate = null, int? count = null)
        {
            var query = Connection.Table<DownloadHistoryEntry>();
            if (count != null)
                query = query.Take((int) count);
            if (predicate != null)
                query = query.Where(predicate);
            return (await query.ToListAsync()).Select(ToObservableDownloadTask);
        }

        public Task DeleteAsync(Expression<Func<DownloadHistoryEntry, bool>> predicate)
        {
            return Connection.Table<DownloadHistoryEntry>().DeleteAsync(predicate);
        }

        public async Task<IEnumerable<ObservableDownloadTask>> EnumerateAsync()
        {
            return (await RawDataAsync()).Select(ToObservableDownloadTask);
        }

        public async Task<IEnumerable<DownloadHistoryEntry>> RawDataAsync()
        {
            return await Connection.Table<DownloadHistoryEntry>().ToListAsync();
        }

        public async Task Purge(int limit)
        {
            var list = await Connection.Table<DownloadHistoryEntry>().ToListAsync();
            if (list.Count > limit)
            {
                var last = list.Take(^limit..).Select(e => e.Destination).ToHashSet();
                await DeleteAsync(e => !last.Contains(e.Destination!));
            }
        }

        private static ObservableDownloadTask ToObservableDownloadTask(DownloadHistoryEntry entry)
        {
            return entry.IsUgoira
                ? new LazyInitializedAnimatedIllustrationDownloadTask(entry) { ProgressPercentage = 100 }
                : new LazyInitializedIllustrationDownloadTask(entry) { ProgressPercentage = 100 };
        }
    }
}