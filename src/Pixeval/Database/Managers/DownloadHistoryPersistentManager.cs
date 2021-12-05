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
        public DownloadHistoryPersistentManager(SQLiteAsyncConnection connection)
        {
            Connection = connection;
            Connection.CreateTableAsync<DownloadHistoryEntry>().Wait();
        }

        public SQLiteAsyncConnection Connection { get; }

        public void Insert(DownloadHistoryEntry t)
        {
            Connection.InsertAsync(t);
        }


        public async Task<IEnumerable<ObservableDownloadTask>> QueryAsync(Func<AsyncTableQuery<DownloadHistoryEntry>, AsyncTableQuery<DownloadHistoryEntry>> action)
        {
            var query = Connection.Table<DownloadHistoryEntry>();
            query = action(query);
            return (await query.ToListAsync()).Select(ToObservableDownloadTask);
        }

        public async Task<IEnumerable<ObservableDownloadTask>> SelectAsync(Expression<Func<DownloadHistoryEntry, bool>>? predicate = null, int? count = null)
        {
            var query = Connection.Table<DownloadHistoryEntry>();
            if (count != null)
                query = query.Take((int)count);
            if (predicate != null)
                query = query.Where(predicate);
            return (await query.ToListAsync()).Select(ToObservableDownloadTask);
        }

        public Task Delete(Expression<Func<DownloadHistoryEntry, bool>> predicate)
        {
            return Connection.Table<DownloadHistoryEntry>().DeleteAsync(predicate);
        }

        public async Task<IEnumerable<ObservableDownloadTask>> EnumerateAsync()
        {
            return (await Connection.Table<DownloadHistoryEntry>().ToListAsync()).Select(ToObservableDownloadTask);
        }

        private static ObservableDownloadTask ToObservableDownloadTask(DownloadHistoryEntry entry)
        {
            return entry.Type switch
            {
                DownloadHistoryType.Illustration => new LazyInitializedIllustrationDownloadTask(entry.Id!, entry.Title, entry.Description, entry.Url!, entry.Destination!, entry.Thumbnail, entry.UniqueCacheId, entry.FinalState)
                {
                    ProgressPercentage = 100,
                },
                DownloadHistoryType.AnimatedIllustration => new LazyInitializedAnimatedIllustrationDownloadTask(entry.Id!, entry.Title, entry.Description, entry.Url!, entry.Destination!, entry.Thumbnail, entry.UniqueCacheId, entry.FinalState)
                {
                    ProgressPercentage = 100,
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}