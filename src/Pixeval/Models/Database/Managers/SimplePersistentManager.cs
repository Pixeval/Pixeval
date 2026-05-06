// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using SQLite;

namespace Pixeval.Models.Database.Managers;

/// <summary>
/// A simple persistent manager without mapping
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SimplePersistentManager<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IPersistentManager<T, T>
    where T : HistoryEntry, new()
{
    private readonly SQLiteConnection _db;

    protected SimplePersistentManager(SQLiteConnection db, int maximumRecords)
    {
        _db = db;
        MaximumRecords = maximumRecords;
        _ = _db.CreateTable<T>();
    }

    /// <inheritdoc />
    public virtual TableQuery<T> Queryable => _db.Table<T>();

    /// <inheritdoc />
    public int MaximumRecords { get; set; }

    /// <inheritdoc />
    public virtual int Count => Queryable.Count();

    /// <inheritdoc />
    public virtual void Insert(T t)
    {
        if (Queryable.Count() > MaximumRecords)
            Purge(MaximumRecords);

        _db.Insert(t, typeof(T));
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Query(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
    {
        return Queryable.Where(predicate).Skip(skip).Take(limit).ToArray();
    }

    /// <inheritdoc />
    public virtual void AddOrUpdate(T entry) => _db.InsertOrReplace(entry, typeof(T));

    /// <inheritdoc />
    public virtual void Update(T entry) => _db.Update(entry, typeof(T));

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Take(int count) => Queryable.Take(count).ToArray();

    /// <inheritdoc />
    public virtual IReadOnlyList<T> TakeLast(int count)
    {
        var c = Count;
        if (count > c)
            count = c;
        return Queryable.Skip(c - count).Take(count).ToArray();
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Select(Expression<Func<T, bool>> predicate) => Queryable.Where(predicate).ToArray();

    /// <inheritdoc />
    public virtual bool TryDelete(T item) => _db.Delete<T>(item.HistoryEntryId) is not 0;

    /// <inheritdoc />
    public virtual T? TryDelete(Expression<Func<T, bool>> predicate)
    {
        if (Queryable.FirstOrDefault(predicate) is { } e)
        {
            _db.Delete<T>(e.HistoryEntryId);
            return e;
        }

        return null;
    }

    /// <inheritdoc />
    public virtual int Delete(Expression<Func<T, bool>> predicate) => Queryable.Delete(predicate);

    /// <inheritdoc />
    public virtual IReadOnlyList<T> ToArray() => Queryable.ToArray();

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Reverse() => Queryable.OrderByDescending(t => t.HistoryEntryId).ToArray();

    /// <inheritdoc />
    public virtual void Purge(int limit)
    {
        var deleteCount = Count - limit;
        if (deleteCount > 0)
        {
            var toDelete = Queryable.Take(deleteCount).ToArray();
            foreach (var item in toDelete)
                _db.Delete<T>(item.HistoryEntryId);
        }
    }

    /// <inheritdoc />
    public virtual void Clear() => _db.DeleteAll<T>();

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => Queryable.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
