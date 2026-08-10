// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public sealed class BlockedUserPersistentManager : SimplePersistentManager<BlockedUserEntry>
{
    private readonly HashSet<long> _blockedUserIds;

    public BlockedUserPersistentManager(SQLiteConnection db) : base(db)
    {
        _blockedUserIds = AccessDatabase(connection => connection.Table<BlockedUserEntry>()
            .Select(static entry => entry.Id)
            .ToHashSet());
    }

    public FrozenSet<long> GetBlockedUserIds() => AccessDatabase(_ => _blockedUserIds.ToFrozenSet());

    public BlockedUserEntry? GetByUserId(long userId) =>
        userId <= 0
            ? null
            : AccessDatabase(connection => connection.Table<BlockedUserEntry>()
                .FirstOrDefault(entry => entry.Id == userId));

    public override void AddOrUpdate(BlockedUserEntry entry)
    {
        AccessDatabase(connection =>
        {
            var actual = AddOrUpdateCore(connection, entry);
            _ = _blockedUserIds.Add(actual.Id);
        });
    }

    public override BlockedUserEntry Upsert(BlockedUserEntry entry) =>
        AccessDatabase(connection =>
        {
            var actual = AddOrUpdateCore(connection, entry);
            _ = _blockedUserIds.Add(actual.Id);
            return actual;
        });

    public bool TryDeleteByUserId(long userId)
    {
        if (userId <= 0)
            return false;

        return AccessDatabase(connection =>
        {
            if (connection.Table<BlockedUserEntry>().FirstOrDefault(e => e.Id == userId) is not { } entry
                || connection.Delete<BlockedUserEntry>(entry.HistoryEntryId) is 0)
                return false;

            _ = _blockedUserIds.Remove(userId);
            return true;
        });
    }

    private static BlockedUserEntry AddOrUpdateCore(SQLiteConnection connection, BlockedUserEntry entry)
    {
        if (connection.Table<BlockedUserEntry>().FirstOrDefault(e => e.Id == entry.Id) is not { } existing)
        {
            _ = connection.Insert(entry, typeof(BlockedUserEntry));
            return entry;
        }

        existing.UpdateFrom(entry);
        _ = connection.Update(existing, typeof(BlockedUserEntry));
        return existing;
    }
}
