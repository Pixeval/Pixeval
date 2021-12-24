using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB;

namespace Pixeval.Database.Managers;

/// <summary>
/// A simple persistent manager without mapping
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SimplePersistentManager<T> : IPersistentManager<T, T>
    where T : new()
{
#nullable disable
    public ILiteCollection<T> Collection { get; init; }
#nullable restore
    public int MaximumRecords { get; set; }
    public void Insert(T t)
    {
        if (Collection.Count() > MaximumRecords)
        {
            Purge(MaximumRecords);
        }

        Collection.Insert(t);
    }

    public IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return Collection.Find(predicate);
    }

    public IEnumerable<T> Select(Expression<Func<T, bool>>? predicate = null, int? count = null)
    {
        var query = Collection.FindAll();
        if (count.HasValue)
            query = query.Take(count.Value);
        if (predicate != null)
            query = query.Where(predicate.Compile());
        return query.ToList();
    }

    public int Delete(Expression<Func<T, bool>> predicate)
    {
        return Collection.DeleteMany(predicate);
    }

    public IEnumerable<T> Enumerate()
    {
        return Collection.FindAll();
    }

    public void Purge(int limit)
    {
        if (Collection.Count() > limit)
        {
            var last = Collection.FindAll().Take(^limit..).ToHashSet();
            Delete(e => !last.Contains(e!));
        }
    }
}