// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using WinUI3Utilities;

namespace Pixeval.Database.Managers;

public class BrowseHistoryPersistentManager(ILiteDatabase db, int maximumRecords)
    : SimplePersistentManager<BrowseHistoryEntry>(db, maximumRecords)
{
    public static void AddHistory(IArtworkInfo entry)
    {
        if (entry.Id is "")
            return;
        if (entry is not ISerializable serializable)
        {
            ThrowHelper.InvalidOperation($"{nameof(entry)} should be {nameof(ISerializable)}");
            return;
        }
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        _ = manager.TryDelete(x =>
            x.SerializeKey == serializable.SerializeKey
            && x.Id == entry.Id);
        var browseHistoryEntry = new BrowseHistoryEntry(entry);
        manager.Insert(browseHistoryEntry);
    }
}
