// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

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
    protected readonly SQLiteConnection Db;

    protected SimplePersistentManager(SQLiteConnection db)
    {
        Db = db;
        _ = Db.CreateTable<T>();
    }

    /// <inheritdoc />
    public virtual TableQuery<T> Queryable => Db.Table<T>();

    /// <inheritdoc />
    public virtual int Count => Queryable.Count();

    /// <inheritdoc />
    public virtual void Insert(T t) => Db.Insert(t, typeof(T));

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Query(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
    {
        return [.. Queryable.Where(predicate).Skip(skip).Take(limit)];
    }

    /// <inheritdoc />
    public virtual void AddOrUpdate(T entry) => Db.InsertOrReplace(entry, typeof(T));

    public virtual T Upsert(T entry)
    {
        AddOrUpdate(entry);
        return Queryable.FirstOrDefault(t => t.HistoryEntryId == entry.HistoryEntryId) ?? entry;
    }

    /// <inheritdoc />
    public virtual void Update(T entry) => Db.Update(entry, typeof(T));

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Take(int count) => [.. Queryable.Take(count)];

    /// <inheritdoc />
    public virtual IReadOnlyList<T> TakeLast(int count)
    {
        var c = Count;
        if (count > c)
            count = c;
        return [.. Queryable.Skip(c - count).Take(count)];
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Select(Expression<Func<T, bool>> predicate) => [.. Queryable.Where(predicate)];

    /// <inheritdoc />
    public virtual bool TryDelete(T item) => Db.Delete<T>(item.HistoryEntryId) is not 0;

    /// <inheritdoc />
    public virtual T? TryDelete(Expression<Func<T, bool>> predicate)
    {
        if (Queryable.FirstOrDefault(predicate) is { } e)
        {
            Db.Delete<T>(e.HistoryEntryId);
            return e;
        }

        return null;
    }

    /// <inheritdoc />
    public virtual int Delete(Expression<Func<T, bool>> predicate) => Queryable.Delete(predicate);

    /// <inheritdoc />
    public virtual IReadOnlyList<T> ToArray() => [.. Queryable];

    /// <inheritdoc />
    public virtual IReadOnlyList<T> Reverse() => [.. Queryable.OrderByDescending(t => t.HistoryEntryId)];

    /// <inheritdoc />
    public virtual void Purge(int limit)
    {
        var deleteCount = Count - limit;
        if (deleteCount > 0)
        {
            var toDelete = Queryable.Take(deleteCount).ToArray();
            foreach (var item in toDelete)
                Db.Delete<T>(item.HistoryEntryId);
        }
    }

    /// <inheritdoc />
    public virtual void Clear() => Db.DeleteAll<T>();

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => Queryable.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
