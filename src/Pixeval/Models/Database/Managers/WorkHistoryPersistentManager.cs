// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Diagnostics.CodeAnalysis;
using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public abstract class WorkHistoryPersistentManager<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
TEntry>(SQLiteConnection db, FileLogger logger) : ArtworkHistoryPersistentManager<TEntry>(db, logger)
    where TEntry : BrowseHistoryEntry, new()
{
    public TEntry? GetByWorkKey(string workKey) =>
        string.IsNullOrWhiteSpace(workKey)
            ? null
            : FindEntry(entry => entry.WorkKey == workKey);

    public void AddOrReplace(TEntry entry) =>
        InsertReplacing(entry, query => query.FirstOrDefault(item => item.WorkKey == entry.WorkKey));

    public bool TryDeleteByWorkKey(string workKey) =>
        !string.IsNullOrWhiteSpace(workKey)
        && TryDelete(query => query.FirstOrDefault(entry => entry.WorkKey == workKey));
}
