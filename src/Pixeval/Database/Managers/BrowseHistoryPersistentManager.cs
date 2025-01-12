// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;

namespace Pixeval.Database.Managers;

public class BrowseHistoryPersistentManager(ILiteDatabase db, int maximumRecords)
    : SimplePersistentManager<BrowseHistoryEntry>(db, maximumRecords)
{
    public static void AddHistory(IWorkEntry entry)
    {
        if (entry.Id is 0)
            return;
        var type = entry is Illustration ? SimpleWorkType.IllustAndManga : SimpleWorkType.Novel;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        _ = manager.TryDelete(x => x.Id == entry.Id && x.Type == type);
        var browseHistoryEntry = new BrowseHistoryEntry(entry);
        manager.Insert(browseHistoryEntry);
    }
}
