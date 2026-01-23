// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public class BrowseHistoryPersistentManager(SQLiteConnection db, int maximumRecords)
    : SimplePersistentManager<BrowseHistoryEntry>(db, maximumRecords)
{
    public static void AddHistory(IArtworkInfo entry)
    {
        if (entry.Id is "")
            return;
        if (entry is not ISerializable serializable)
            throw new InvalidCastException($"{nameof(entry)} should be {nameof(ISerializable)}");
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        _ = manager.TryDelete(x =>
            x.SerializeKey == serializable.SerializeKey
            && x.Id == entry.Id);
        var browseHistoryEntry = new BrowseHistoryEntry(entry);
        manager.Insert(browseHistoryEntry);
    }
}
