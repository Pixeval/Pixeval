#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadHistoryPersistentManager.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using Pixeval.Download;
using Pixeval.Download.Models;

namespace Pixeval.Database.Managers;

public class DownloadHistoryPersistentManager(ILiteDatabase collection, int maximumRecords) : IPersistentManager<DownloadHistoryEntry, DownloadTaskBase>
{
    public ILiteCollection<DownloadHistoryEntry> Collection { get; init; } = collection.GetCollection<DownloadHistoryEntry>(nameof(DownloadHistoryEntry));

    public int MaximumRecords { get; set; } = maximumRecords;

    public int Count => Collection.Count();

    public void Insert(DownloadHistoryEntry t)
    {
        if (Collection.Count() > MaximumRecords)
        {
            Purge(MaximumRecords);
        }

        _ = Collection.Insert(t);
        t.PropertyChanged += (_, _) =>
        {
            if (Collection.Find(entry => entry.Destination == t.Destination).Any())
            {
                _ = Collection.Update(t);
            }
        };
    }

    public IEnumerable<DownloadTaskBase> Query(Expression<Func<DownloadHistoryEntry, bool>> predicate)
    {
        return Collection.Find(predicate).Select(ToObservableDownloadTask);
    }

    public IEnumerable<DownloadTaskBase> Select(Expression<Func<DownloadHistoryEntry, bool>>? predicate = null, int? count = null)
    {
        var query = Collection.FindAll();
        if (count.HasValue)
        {
            query = query.TakeLast(count.Value);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate.Compile());
        }

        return query.Select(ToObservableDownloadTask);
    }

    public int Delete(Expression<Func<DownloadHistoryEntry, bool>> predicate)
    {
        return Collection.DeleteMany(predicate);
    }

    public IEnumerable<DownloadTaskBase> Enumerate()
    {
        return Collection.FindAll().Select(ToObservableDownloadTask);
    }

    public IEnumerable<DownloadTaskBase> Reverse()
    {
        return Collection.Find(LiteDB.Query.All(LiteDB.Query.Descending)).Select(ToObservableDownloadTask);
    }

    public void Purge(int limit)
    {
        if (Collection.Count() > limit)
        {
            var last = Collection.FindAll().Take(^limit..).Select(e => e.Destination).ToHashSet();
            _ = Delete(e => !last.Contains(e.Destination));
        }
    }

    public void Clear()
    {
        _ = Collection.DeleteAll();
        App.AppViewModel.DownloadManager.ClearTasks();
    }

    private static DownloadTaskBase ToObservableDownloadTask(DownloadHistoryEntry entry)
    {
        return entry.Type switch
        {
            DownloadItemType.Novel => new LazyInitializedNovelDownloadTask(entry) { CurrentState = DownloadState.Completed },
            DownloadItemType.Ugoira => new LazyInitializedUgoiraDownloadTask(entry) { CurrentState = DownloadState.Completed },
            DownloadItemType.Manga => new LazyInitializedMangaDownloadTask(entry) { CurrentState = DownloadState.Completed },
            _ => new LazyInitializedIllustrationDownloadTask(entry) { CurrentState = DownloadState.Completed }
        };
    }
}
