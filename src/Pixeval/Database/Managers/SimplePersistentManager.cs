using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        public SimplePersistentManager(SQLiteAsyncConnection connection)
        {
            Connection = connection;
            Connection.CreateTableAsync<T>().Wait();
        }

        public SQLiteAsyncConnection Connection { get; }

        public void Insert(T t)
        {
            Connection.InsertAsync(t);
        }

        public async Task<IEnumerable<T>> QueryAsync(Func<AsyncTableQuery<T>, AsyncTableQuery<T>> action)
        {
            var query = Connection.Table<T>();
            query = action(query);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> SelectAsync(Expression<Func<T, bool>>? predicate = null, int? count = null)
        {
            var query = Connection.Table<T>();
            if (count != null)
                query = query.Take((int)count);
            if (predicate != null)
                query = query.Where(predicate);
            return await query.ToListAsync();
        }

        public Task Delete(Expression<Func<T, bool>> predicate)
        {
            return Connection.Table<T>().DeleteAsync(predicate);
        }

        public async Task<IEnumerable<T>> EnumerateAsync()
        {
            return (await Connection.Table<T>().ToListAsync());
        }
    }
}
