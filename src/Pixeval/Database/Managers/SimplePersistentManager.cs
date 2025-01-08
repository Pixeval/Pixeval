// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;

namespace Pixeval.Database.Managers;

/// <summary>
/// A simple persistent manager without mapping
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SimplePersistentManager<T>(ILiteDatabase db, int maximumRecords) : IPersistentManager<T, T>
    where T : class, IHistoryEntry, new()
{
    public ILiteCollection<T> Collection { get; init; } = db.GetCollection<T>(typeof(T).Name);

    public int MaximumRecords { get; set; } = maximumRecords;

    public int Count => Collection.Count();

    public void Insert(T t)
    {
        if (Collection.Count() > MaximumRecords)
            Purge(MaximumRecords);

        _ = Collection.Insert(t);
    }

    public IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return Collection.Find(predicate);
    }

    public void Update(T entry)
    {
        Collection.Update(entry);
    }

    public IEnumerable<T> Take(int count)
    {
        return Collection.Find(_ => true, 0, count);
    }

    public IEnumerable<T> TakeLast(int count)
    {
        return Collection.Find(_ => true, Collection.Count() - count, count);
    }

    public IEnumerable<T> Select(Expression<Func<T, bool>> predicate)
    {
        return Collection.Find(predicate);
    }

    public T? TryDelete(Expression<Func<T, bool>> predicate)
    {
        if (Collection.FindOne(predicate) is { } e)
        {
            Collection.Delete(e.HistoryEntryId);
            return e;
        }

        return null;
    }

    public int Delete(Expression<Func<T, bool>> predicate)
    {
        return Collection.DeleteMany(predicate);
    }

    public IEnumerable<T> Enumerate()
    {
        return Collection.FindAll();
    }

    public IEnumerable<T> Reverse()
    {
        return Collection.Find(LiteDB.Query.All(LiteDB.Query.Descending));
    }

    public void Purge(int limit)
    {
        if (Collection.Count() > limit)
        {
            var last = Collection.FindAll().Take(^limit..).ToHashSet();
            _ = Delete(e => !last.Contains(e));
        }
    }

    public void Clear()
    {
        _ = Collection.DeleteAll();
    }
}
