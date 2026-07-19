// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using SQLite;

namespace Pixeval.Models.Database.Managers;

internal static class DownloadHistoryMigration
{
    private const string LegacyTableName = nameof(DownloadHistoryEntry);
    private const string PayloadIdColumnName = nameof(ArtworkHistoryEntry.ArtworkPayloadEntryId);
    private const string WorkSubscriptionIdColumnName = nameof(LegacyDownloadHistoryEntry.WorkSubscriptionId);

    public static void Migrate(SQLiteConnection db)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (!db.GetTableInfo(LegacyTableName)
                .Any(static column => column.Name is WorkSubscriptionIdColumnName))
            return;

        db.RunInTransaction(() =>
        {
            _ = db.CreateTable<ArtworkPayloadEntry>();
            _ = db.CreateTable<SubscriptionDownloadHistoryEntry>();

            // The published schema had no real foreign key, so remove dangling rows before moving them.
            _ = db.Execute(
                $"""
                 DELETE FROM "{LegacyTableName}"
                 WHERE NOT EXISTS (
                     SELECT 1
                     FROM "{nameof(ArtworkPayloadEntry)}"
                     WHERE "{nameof(ArtworkPayloadEntry)}"."{nameof(ArtworkPayloadEntry.ArtworkPayloadEntryId)}"
                         = "{LegacyTableName}"."{PayloadIdColumnName}"
                 )
                 """);

            var entries = db.Table<LegacyDownloadHistoryEntry>()
                .Where(static entry => entry.WorkSubscriptionId > 0)
                .OrderBy(static entry => entry.HistoryEntryId)
                .ToArray();
            foreach (var entry in entries)
                MigrateEntry(db, entry);

            _ = db.Execute($"DROP INDEX IF EXISTS \"{LegacyTableName}_{WorkSubscriptionIdColumnName}\"");
            _ = db.Execute($"ALTER TABLE \"{LegacyTableName}\" DROP COLUMN \"{WorkSubscriptionIdColumnName}\"");
        });
    }

    private static void MigrateEntry(SQLiteConnection db, LegacyDownloadHistoryEntry entry)
    {
        var payload = db.Find<ArtworkPayloadEntry>(entry.ArtworkPayloadEntryId);
        if (payload is null
            || !TryAttachPayload(entry, payload)
            || string.IsNullOrWhiteSpace(entry.Destination)
            || string.IsNullOrWhiteSpace(entry.Entry.Id))
        {
            DeleteBrokenEntry(db, entry);
            return;
        }

        _ = db.Insert(
            new SubscriptionDownloadHistoryEntry
            {
                ArtworkId = entry.Entry.Id,
                ArtworkPayloadEntryId = entry.ArtworkPayloadEntryId,
                Destination = entry.Destination,
                ErrorMessage = entry.ErrorMessage,
                FormatToken = entry.FormatToken,
                SerializeKey = entry.SerializeKey,
                State = entry.State,
                WorkSubscriptionId = entry.WorkSubscriptionId
            },
            typeof(SubscriptionDownloadHistoryEntry));
        _ = db.Delete<LegacyDownloadHistoryEntry>(entry.HistoryEntryId);
    }

    private static bool TryAttachPayload(
        LegacyDownloadHistoryEntry entry,
        ArtworkPayloadEntry payload)
    {
        try
        {
            entry.AttachPayload(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void DeleteBrokenEntry(SQLiteConnection db, LegacyDownloadHistoryEntry entry)
    {
        _ = db.Delete<LegacyDownloadHistoryEntry>(entry.HistoryEntryId);
        if (entry.ArtworkPayloadEntryId > 0)
            _ = db.Delete<ArtworkPayloadEntry>(entry.ArtworkPayloadEntryId);
    }

    [Table(nameof(DownloadHistoryEntry))]
    private sealed class LegacyDownloadHistoryEntry : DownloadHistoryEntryBase
    {
        public LegacyDownloadHistoryEntry()
        {
        }

        [Indexed(Unique = true)]
        public override string Destination { get; init; } = null!;

        [Indexed]
        public int WorkSubscriptionId { get; init; }
    }
}
