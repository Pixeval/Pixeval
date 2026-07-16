// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Models.Options;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public class WorkSubscriptionPersistentManager(SQLiteConnection db)
    : SimplePersistentManager<WorkSubscriptionEntry>(db)
{
    public WorkSubscriptionEntry? GetBySubscriptionKey(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        AccessDatabase(connection => connection.Table<WorkSubscriptionEntry>()
            .FirstOrDefault(entry =>
                entry.Id == targetId
                && entry.SubscriptionType == subscriptionType
                && entry.WorkKind == workKind));

    public override void AddOrUpdate(WorkSubscriptionEntry entry)
    {
        AccessDatabase(connection => _ = AddOrUpdateCore(connection, entry));
    }

    public override WorkSubscriptionEntry Upsert(WorkSubscriptionEntry entry) =>
        AccessDatabase(connection => AddOrUpdateCore(connection, entry));

    public WorkSubscriptionEntry? GetByKey(int key) =>
        key <= 0 ? null : AccessDatabase(connection => connection.Find<WorkSubscriptionEntry>(key));

    private static WorkSubscriptionEntry AddOrUpdateCore(SQLiteConnection connection, WorkSubscriptionEntry entry)
    {
        if (FindBySubscriptionKey(connection, entry.Id, entry.SubscriptionType, entry.WorkKind) is not { } existing)
        {
            _ = connection.Insert(entry, typeof(WorkSubscriptionEntry));
            return entry;
        }

        existing.UpdateFrom(entry);
        _ = connection.Update(existing, typeof(WorkSubscriptionEntry));
        return existing;
    }

    private static WorkSubscriptionEntry? FindBySubscriptionKey(
        SQLiteConnection connection,
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        connection.Table<WorkSubscriptionEntry>()
            .FirstOrDefault(entry =>
                entry.Id == targetId
                && entry.SubscriptionType == subscriptionType
                && entry.WorkKind == workKind);
}
