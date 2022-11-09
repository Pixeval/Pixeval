using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using LiteDB.Async;

namespace Pixeval.Data
{
    public interface IBaseRepository<T>
    {
        ILiteCollectionAsync<T> Collection { get; }
        Task<int> CountAsync();
        Task<T> CreateAsync(T obj);
        Task<IEnumerable<T>> FindAllAsync();
        IAsyncEnumerable<T> FindAllEnumerableAsync();
        Task<T> FindByIdAsync(ObjectId id);
        Task UpdateAsync(T obj);
        Task<bool> DeleteAsync(ObjectId id);
        Task DeleteAllAsync();
    }

    public class BaseRepository<T> : IBaseRepository<T>
    {
        public ILiteDatabaseAsync Database { get; }
        public ILiteCollectionAsync<T> Collection { get; }

        public BaseRepository(ILiteDatabaseAsync database)
        {
            Database = database;
            Collection = database.GetCollection<T>();
        }

        public Task<int> CountAsync()
        {
            return Collection.CountAsync();
        }

        public virtual async Task<T> CreateAsync(T obj)
        {
            var newId = await Collection.InsertAsync(obj);
            return await Collection.FindByIdAsync(newId.AsObjectId);
        }

        public virtual Task<IEnumerable<T>> FindAllAsync()
        {
            return Collection.FindAllAsync();
        }

        public virtual async IAsyncEnumerable<T> FindAllEnumerableAsync()
        {
            foreach (var obj in await Collection.FindAllAsync())
            {
                yield return obj;
            }
        }

        public virtual Task<T> FindByIdAsync(ObjectId id)
        {
            return Collection.FindByIdAsync(id);
        }

        public virtual Task UpdateAsync(T obj)
        {
            return Collection.UpdateAsync(obj);
        }

        public virtual Task<bool> DeleteAsync(ObjectId id)
        {
            return Collection.DeleteAsync(id);
        }

        public Task DeleteAllAsync()
        {
            return Collection.DeleteAllAsync();
        }
    }
}
