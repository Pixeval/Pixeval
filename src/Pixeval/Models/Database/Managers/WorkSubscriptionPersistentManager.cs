// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Models.Options;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public class WorkSubscriptionPersistentManager(SQLiteConnection db)
    : SimplePersistentManager<WorkSubscriptionEntry>(db, int.MaxValue)
{
    public WorkSubscriptionEntry? GetBySubscriptionKey(
        long userId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        Queryable.FirstOrDefault(t =>
            t.UserId == userId
            && t.SubscriptionType == subscriptionType
            && t.WorkKind == workKind);

    public override void AddOrUpdate(WorkSubscriptionEntry entry)
    {
        var existing = GetBySubscriptionKey(entry.UserId, entry.SubscriptionType, entry.WorkKind);
        if (existing is not null)
        {
            existing.UpdateFrom(entry);
            Update(existing);
            return;
        }

        Db.Insert(entry, typeof(WorkSubscriptionEntry));
    }

    public override WorkSubscriptionEntry Upsert(WorkSubscriptionEntry entry)
    {
        AddOrUpdate(entry);
        return GetBySubscriptionKey(entry.UserId, entry.SubscriptionType, entry.WorkKind)
               ?? entry;
    }

    public WorkSubscriptionEntry? GetByKey(int key) =>
        key <= 0 ? null : Db.Find<WorkSubscriptionEntry>(key);
}
