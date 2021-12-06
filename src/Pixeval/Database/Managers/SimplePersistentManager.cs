using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;

namespace Pixeval.Database.Managers
{
    /// <summary>
    /// A simple persistent manager without mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SimplePersistentManager<T> : IPersistentManager<T, T>
        where T : new()
    {
        public SQLiteAsyncConnection Connection { get; init; } = null!;
        public int MaximumRecords { get; set; }

        public async Task InsertAsync(T t)
        {
            if (await Connection.Table<DownloadHistoryEntry>().CountAsync() > MaximumRecords)
            {
                await Purge(MaximumRecords);
            }
            await Connection.InsertAsync(t);
        }

        public async Task<IEnumerable<T>> QueryAsync(Func<AsyncTableQuery<T>, AsyncTableQuery<T>> action)
        {
            return await action(Connection.Table<T>()).ToListAsync();
        }

        public async Task<IEnumerable<T>> SelectAsync(Expression<Func<T, bool>>? predicate = null, int? count = null)
        {
            var query = Connection.Table<T>();
            if (count != null)
                query = query.Take((int) count);
            if (predicate != null)
                query = query.Where(predicate);
            return await query.ToListAsync();
        }

        public Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            return Connection.Table<T>().DeleteAsync(predicate);
        }

        public async Task<IEnumerable<T>> EnumerateAsync()
        {
            return await Connection.Table<T>().ToListAsync();
        }

        public Task<IEnumerable<T>> RawDataAsync()
        {
            return EnumerateAsync();
        }

        public async Task Purge(int limit)
        {
            var list = (await EnumerateAsync()).ToList();
            if (list.Count > limit)
            {
                var last = list.Take(^limit..).ToHashSet();
                await DeleteAsync(e => !last.Contains(e!));
            }
        }
    }
}
