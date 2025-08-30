using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;

namespace Pixeval.Database.Managers;

public abstract class PersistentManagerBase<TEntry, TModel>(ILiteDatabase collection, int maximumRecords) : IPersistentManager<TEntry, TModel> where TEntry : class, IHistoryEntry
{
    public ILiteCollection<TEntry> Collection { get; init; } = collection.GetCollection<TEntry>(nameof(TEntry));

    public int MaximumRecords { get; set; } = maximumRecords;

    public int Count => Collection.Count();

    public void Insert(TEntry t)
    {
        if (Collection.Count() > MaximumRecords)
            Purge(MaximumRecords);

        _ = Collection.Insert(t);
    }

    public IEnumerable<TModel> Query(Expression<Func<TEntry, bool>> predicate, int skip = 0, int limit = int.MaxValue)
    {
        return Collection.Find(predicate, skip, limit).Select(ToModel);
    }

    public void Update(TEntry entry)
    {
        _ = Collection.Update(entry);
    }

    public IEnumerable<TModel> Take(int count)
    {
        return Collection.Find(LiteDB.Query.All(), 0, count).Select(ToModel);
    }

    public IEnumerable<TModel> TakeLast(int count)
    {
        return Collection.Find(LiteDB.Query.All(LiteDB.Query.Descending), Collection.Count() - count, count).Select(ToModel);
    }

    public IEnumerable<TModel> Select(Expression<Func<TEntry, bool>> predicate)
    {
        return Collection.Find(predicate).Select(ToModel);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public int Delete(Expression<Func<TEntry, bool>> predicate)
    {
        return Collection.DeleteMany(predicate);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public TEntry? TryDelete(Expression<Func<TEntry, bool>> predicate)
    {
        if (Collection.FindOne(predicate) is { } e)
        {
            _ = Collection.Delete(e.HistoryEntryId);
            return e;
        }

        return null;
    }

    /// <summary>
    /// 遍历
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TModel> Enumerate()
    {
        return Collection.FindAll().Select(ToModel);
    }

    /// <summary>
    /// 反转
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TModel> Reverse()
    {
        return Collection.Find(LiteDB.Query.All(LiteDB.Query.Descending)).Select(ToModel);
    }

    /// <summary>
    /// 清除多于<paramref name="limit"/>的记录
    /// </summary>
    /// <param name="limit"></param>
    public void Purge(int limit)
    {
        if (Collection.Count() > limit)
        {
            var last = Collection.FindAll().Take(^limit..);
            foreach (var id in last)
                Collection.Delete(id.HistoryEntryId);
        }
    }

    /// <summary>
    /// 清除所有记录
    /// </summary>
    public void Clear()
    {
        _ = Collection.DeleteAll();
        App.AppViewModel.DownloadManager.ClearTasks();
    }

    protected abstract TModel ToModel(TEntry entry);
}
