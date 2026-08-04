// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public sealed class SubscriptionDownloadHistoryPersistentManager(
    SQLiteConnection db,
    FileLogger logger) : DownloadHistoryPersistentManagerBase<SubscriptionDownloadHistoryEntry>(db, logger)
{
    public void AddOrReplace(SubscriptionDownloadHistoryEntry entry) =>
        InsertReplacing(
            entry,
            query => query.FirstOrDefault(item =>
                item.WorkSubscriptionId == entry.WorkSubscriptionId
                && item.ArtworkId == entry.ArtworkId
                && item.Destination == entry.Destination));

    public void AddOrReplaceRange(IReadOnlyCollection<SubscriptionDownloadHistoryEntry> entries) =>
        InsertReplacingRange(
            entries,
            static (query, entry) => query.FirstOrDefault(item =>
                item.WorkSubscriptionId == entry.WorkSubscriptionId
                && item.ArtworkId == entry.ArtworkId
                && item.Destination == entry.Destination));

    public bool ContainsIdentity(
        int workSubscriptionId,
        string artworkId,
        string destination) =>
        workSubscriptionId > 0
        && !string.IsNullOrWhiteSpace(artworkId)
        && !string.IsNullOrWhiteSpace(destination)
        && AccessDatabase(connection => connection.Table<SubscriptionDownloadHistoryEntry>()
            .FirstOrDefault(entry =>
                entry.WorkSubscriptionId == workSubscriptionId
                && entry.ArtworkId == artworkId
                && entry.Destination == destination) is not null);

    public bool TryDeleteByIdentity(
        int workSubscriptionId,
        string artworkId,
        string destination) =>
        workSubscriptionId > 0
        && !string.IsNullOrWhiteSpace(artworkId)
        && !string.IsNullOrWhiteSpace(destination)
        && TryDelete(query => query.FirstOrDefault(entry =>
            entry.WorkSubscriptionId == workSubscriptionId
            && entry.ArtworkId == artworkId
            && entry.Destination == destination));

    internal int DeleteByWorkSubscriptionId(int workSubscriptionId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workSubscriptionId);
        return DeleteMatching(connection => connection.Table<SubscriptionDownloadHistoryEntry>()
            .Where(entry => entry.WorkSubscriptionId == workSubscriptionId)
            .ToArray());
    }

    internal int DeleteOrphans(IReadOnlySet<int> workSubscriptionIds)
    {
        ArgumentNullException.ThrowIfNull(workSubscriptionIds);
        return DeleteMatching(connection => connection.Table<SubscriptionDownloadHistoryEntry>()
            .ToArray()
            .Where(entry => !workSubscriptionIds.Contains(entry.WorkSubscriptionId))
            .ToArray());
    }
}
