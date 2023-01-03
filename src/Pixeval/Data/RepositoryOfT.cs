using LiteDB;
using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Data;

public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> : IReadRepository<T>
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}

public class Repository<T> : IRepository<T>
{
    private readonly ILiteCollectionAsync<T> _collection;
    public Repository(ILiteDatabaseAsync database)
    {
        _collection = database.GetCollection<T>();
    }

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        if (id is ObjectId objectId)
        {
            return await _collection.FindByIdAsync(objectId);
        }
        return default;
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _collection.Query().Where(predicate).FirstOrDefaultAsync()!;
    }

    public Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _collection.Query().Where(predicate).SingleOrDefaultAsync()!;
    }

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return _collection.Query().ToListAsync();
    }

    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _collection.Query().Where(predicate).ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _collection.CountAsync(predicate);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _collection.CountAsync();
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _collection.Query().Where(predicate).ExistsAsync();
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return _collection.Query().ExistsAsync();
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        return _collection.InsertAsync(entity);
    }

    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        return _collection.InsertBulkAsync(entities);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        return _collection.UpdateAsync(entity);
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        return _collection.InsertBulkAsync(entities);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteManyAsync(predicate);
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}