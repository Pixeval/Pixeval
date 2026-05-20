// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class DownloadFolderPersistentManager(SQLiteConnection db)
    : SimplePersistentManager<DownloadFolderEntry>(db, int.MaxValue)
{
    public DownloadFolderEntry? GetBySubscriptionEntryId(int subscriptionEntryId) =>
        subscriptionEntryId <= 0
            ? null
            : Queryable.FirstOrDefault(t => t.SubscriptionEntryId == subscriptionEntryId);

    public DownloadFolderEntry GetOrCreate(WorkSubscriptionEntry subscription)
    {
        if (GetBySubscriptionEntryId(subscription.HistoryEntryId) is { } existing)
        {
            if (existing.UserId != subscription.UserId || existing.Name != subscription.DisplayName)
            {
                existing.UserId = subscription.UserId;
                existing.Name = subscription.DisplayName;
                Update(existing);
            }

            return existing;
        }

        var entry = new DownloadFolderEntry
        {
            SubscriptionEntryId = subscription.HistoryEntryId,
            UserId = subscription.UserId,
            Name = subscription.DisplayName
        };
        Db.Insert(entry, typeof(DownloadFolderEntry));
        return GetBySubscriptionEntryId(subscription.HistoryEntryId) ?? entry;
    }

    public DownloadFolderEntry? GetByKey(int key) =>
        key <= 0 ? null : Db.Find<DownloadFolderEntry>(key);

    public override void AddOrUpdate(DownloadFolderEntry entry)
    {
        var existing = GetBySubscriptionEntryId(entry.SubscriptionEntryId);
        if (existing is not null)
        {
            existing.UpdateFrom(entry);
            Update(existing);
            return;
        }

        Db.Insert(entry, typeof(DownloadFolderEntry));
    }
}
