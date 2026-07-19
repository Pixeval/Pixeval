// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public sealed class DownloadHistoryPersistentManager(SQLiteConnection db, FileLogger logger)
    : DownloadHistoryPersistentManagerBase<DownloadHistoryEntry>(db, logger)
{
    public void AddOrReplace(DownloadHistoryEntry entry) =>
        InsertReplacing(entry, query => query.FirstOrDefault(item => item.Destination == entry.Destination));

    public bool TryDeleteByDestination(string destination) =>
        !string.IsNullOrWhiteSpace(destination)
        && TryDelete(query => query.FirstOrDefault(entry => entry.Destination == destination));
}
