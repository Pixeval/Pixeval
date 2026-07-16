// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public sealed class WatchLaterPersistentManager : WorkHistoryPersistentManager<WatchLaterEntry>
{
    private readonly HashSet<string> _workKeys;

    public WatchLaterPersistentManager(SQLiteConnection db, FileLogger logger) : base(db, logger)
    {
        _workKeys = AccessDatabase(connection => connection.Table<WatchLaterEntry>()
            .ToArray()
            .Select(static entry => entry.WorkKey)
            .Where(static workKey => !string.IsNullOrWhiteSpace(workKey))
            .ToHashSet(StringComparer.Ordinal));
    }

    public bool ContainsWorkKey(string workKey) =>
        !string.IsNullOrWhiteSpace(workKey)
        && AccessDatabase(_ => _workKeys.Contains(workKey));

    protected override void OnEntryInserted(WatchLaterEntry entry) => _workKeys.Add(entry.WorkKey);

    protected override void OnEntriesDeleted(IReadOnlyCollection<WatchLaterEntry> entries)
    {
        foreach (var entry in entries)
            _ = _workKeys.Remove(entry.WorkKey);
    }

    protected override void OnEntriesCleared() => _workKeys.Clear();
}
