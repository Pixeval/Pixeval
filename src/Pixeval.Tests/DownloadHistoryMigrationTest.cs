using System;
using System.Linq;
using Imouto.BooruParser;
using Mako.Global.Enum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.Download;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using SQLite;

namespace Pixeval.Tests;

[TestClass]
public sealed class DownloadHistoryMigrationTest
{
    [TestMethod]
    public void Migrate_SplitsReleasedTableAndRemovesDanglingRows()
    {
        using var db = new SQLiteConnection(":memory:");
        _ = db.CreateTable<ArtworkPayloadEntry>();
        _ = db.CreateTable<LegacyDownloadHistoryEntry>();
        var ordinaryArtwork = CreatePost("ordinary");
        var subscriptionArtwork = CreatePost("subscription");
        var ordinaryPayload = InsertPayload(db, ordinaryArtwork);
        var subscriptionPayload = InsertPayload(db, subscriptionArtwork);
        _ = db.Insert(CreateLegacyEntry(
            "ordinary-path",
            ordinaryArtwork,
            ordinaryPayload.ArtworkPayloadEntryId,
            0));
        _ = db.Insert(CreateLegacyEntry(
            "subscription-path",
            subscriptionArtwork,
            subscriptionPayload.ArtworkPayloadEntryId,
            42));
        _ = db.Insert(CreateLegacyEntry("dangling-path", CreatePost("dangling"), 999, 0));

        DownloadHistoryMigration.Migrate(db);
        DownloadHistoryMigration.Migrate(db);

        var ordinaryEntry = db.Table<DownloadHistoryEntry>().Single();
        var subscriptionEntry = db.Table<SubscriptionDownloadHistoryEntry>().Single();
        Assert.AreEqual("ordinary-path", ordinaryEntry.Destination);
        Assert.AreEqual(ordinaryPayload.ArtworkPayloadEntryId, ordinaryEntry.ArtworkPayloadEntryId);
        Assert.AreEqual("subscription-path", subscriptionEntry.Destination);
        Assert.AreEqual("subscription", subscriptionEntry.ArtworkId);
        Assert.AreEqual(42, subscriptionEntry.WorkSubscriptionId);
        Assert.AreEqual(subscriptionPayload.ArtworkPayloadEntryId, subscriptionEntry.ArtworkPayloadEntryId);
        Assert.AreEqual(DownloadState.Completed, subscriptionEntry.State);
        Assert.AreEqual("format", subscriptionEntry.FormatToken);
        Assert.AreEqual("error", subscriptionEntry.ErrorMessage);
        Assert.AreEqual(2, db.Table<ArtworkPayloadEntry>().Count());
        Assert.DoesNotContain(static column => column.Name is nameof(LegacyDownloadHistoryEntry.WorkSubscriptionId), db.GetTableInfo(nameof(DownloadHistoryEntry)));
    }

    private static LegacyDownloadHistoryEntry CreateLegacyEntry(
        string destination,
        ISerializable artwork,
        int payloadId,
        int workSubscriptionId) =>
        new()
        {
            ArtworkPayloadEntryId = payloadId,
            Destination = destination,
            ErrorMessage = "error",
            FormatToken = "format",
            SerializeKey = artwork.SerializeKey,
            State = DownloadState.Completed,
            WorkSubscriptionId = workSubscriptionId
        };

    private static ArtworkPayloadEntry InsertPayload(SQLiteConnection db, ISerializable artwork)
    {
        var payload = new ArtworkPayloadEntry(artwork.Serialize());
        _ = db.Insert(payload);
        return payload;
    }

    private static Post CreatePost(string id) => new(
        new(id, $"hash-{id}", PlatformType.Danbooru),
        $"https://example.com/{id}.jpg",
        null,
        null,
        ExistState.Exist,
        DateTimeOffset.UtcNow,
        new("1", "uploader", PlatformType.Danbooru),
        null,
        new(100, 100),
        0,
        SafeRating.General,
        [],
        null);

    [Table(nameof(DownloadHistoryEntry))]
    private sealed class LegacyDownloadHistoryEntry : DownloadHistoryEntryBase
    {
        public LegacyDownloadHistoryEntry()
        {
        }

        [Indexed(Unique = true)]
        public override string Destination { get; init; } = null!;

        [Indexed]
        public int WorkSubscriptionId { get; set; }
    }
}
