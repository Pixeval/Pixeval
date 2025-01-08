// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using Pixeval.Download.Models;

namespace Pixeval.Database.Managers;

public class DownloadHistoryPersistentManager(ILiteDatabase collection, int maximumRecords) : IPersistentManager<DownloadHistoryEntry, IDownloadTaskGroup>
{
    public ILiteCollection<DownloadHistoryEntry> Collection { get; init; } = collection.GetCollection<DownloadHistoryEntry>(nameof(DownloadHistoryEntry));

    public int MaximumRecords { get; set; } = maximumRecords;

    public int Count => Collection.Count();

    public void Insert(DownloadHistoryEntry t)
    {
        if (Collection.Count() > MaximumRecords)
            Purge(MaximumRecords);

        _ = Collection.Insert(t);
    }

    public IEnumerable<IDownloadTaskGroup> Query(Expression<Func<DownloadHistoryEntry, bool>> predicate)
    {
        return Collection.Find(predicate).Select(ToDownloadTaskGroup);
    }

    public void Update(DownloadHistoryEntry entry)
    {
        Collection.Update(entry);
    }

    public IEnumerable<IDownloadTaskGroup> Select(int count)
    {
        return Collection.Find(_ => true, 0, count).Select(ToDownloadTaskGroup);
    }

    public IEnumerable<IDownloadTaskGroup> SelectLast(int count)
    {
        return Collection.Find(_ => true, Collection.Count() - count, count).Select(ToDownloadTaskGroup);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public int Delete(Expression<Func<DownloadHistoryEntry, bool>> predicate)
    {
        return Collection.DeleteMany(predicate);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public DownloadHistoryEntry? TryDelete(Expression<Func<DownloadHistoryEntry, bool>> predicate)
    {
        if (Collection.FindOne(predicate) is { } e)
        {
            Collection.Delete(e.HistoryEntryId);
            return e;
        }

        return null;
    }

    /// <summary>
    /// 遍历
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IDownloadTaskGroup> Enumerate()
    {
        return Collection.FindAll().Select(ToDownloadTaskGroup);
    }

    /// <summary>
    /// 反转
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IDownloadTaskGroup> Reverse()
    {
        return Collection.Find(LiteDB.Query.All(LiteDB.Query.Descending)).Select(ToDownloadTaskGroup);
    }

    /// <summary>
    /// 清除多于<paramref name="limit"/>的记录
    /// </summary>
    /// <param name="limit"></param>
    public void Purge(int limit)
    {
        if (Collection.Count() > limit)
        {
            var last = Collection.FindAll().Take(^limit..).Select(e => e.Destination).ToHashSet();
            _ = Delete(e => !last.Contains(e.Destination));
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

    private static IDownloadTaskGroup ToDownloadTaskGroup(DownloadHistoryEntry entry)
    {
        return entry.Type switch
        {
            DownloadItemType.Novel => new NovelDownloadTaskGroup(entry),
            DownloadItemType.Ugoira => new UgoiraDownloadTaskGroup(entry),
            DownloadItemType.Manga => new MangaDownloadTaskGroup(entry),
            _ => new SingleImageDownloadTaskGroup(entry)
        };
    }
}
