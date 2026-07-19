using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Imouto.BooruParser;
using Mako.Global.Enum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using SQLite;

namespace Pixeval.Tests;

[TestClass]
public sealed class ArtworkHistoryPersistentManagerTest
{
    [TestMethod]
    public async Task StreamEntriesAsync_RemovesEntriesWithoutPayload()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        var manager = new BrowseHistoryPersistentManager(db, logger);
        _ = db.Insert(new BrowseHistoryEntry
        {
            ArtworkPayloadEntryId = 42,
            Id = "1",
            SerializeKey = "missing",
            WorkKey = "missing:1"
        });
        Assert.AreEqual(1, manager.Count);

        await using var enumerator = manager.StreamEntriesAsync().GetAsyncEnumerator();

        Assert.IsFalse(await enumerator.MoveNextAsync());
        Assert.AreEqual(0, manager.Count);
    }

    [TestMethod]
    public async Task StreamAsync_RemovesEntryWithNullSerializeKey()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new BrowseHistoryPersistentManager(db, CreateLogger());
        _ = db.Insert(new BrowseHistoryEntry
        {
            ArtworkPayloadEntryId = 42,
            Id = "1",
            SerializeKey = null!,
            WorkKey = "missing:1"
        });

        await using var enumerator = manager.StreamAsync(SimpleWorkType.Illustration).GetAsyncEnumerator();

        Assert.IsFalse(await enumerator.MoveNextAsync());
        Assert.AreEqual(0, manager.Count);
    }

    [TestMethod]
    public void GetByWorkKey_ReturnsNullAndRemovesBrokenEntry()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new BrowseHistoryPersistentManager(db, CreateLogger());
        _ = db.Insert(new BrowseHistoryEntry
        {
            ArtworkPayloadEntryId = 42,
            Id = "1",
            SerializeKey = "missing",
            WorkKey = "missing:1"
        });

        Assert.IsNull(manager.GetByWorkKey("missing:1"));
        Assert.AreEqual(0, manager.Count);
    }

    [TestMethod]
    public async Task StreamEntriesAsync_ContinuesAfterPageOfMissingPayloads()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        var manager = new DownloadHistoryPersistentManager(db, logger);
        manager.Insert(new("valid", CreatePost("valid")));
        for (var i = 0; i < 100; i++)
        {
            _ = db.Insert(new DownloadHistoryEntry
            {
                ArtworkPayloadEntryId = 1000 + i,
                Destination = $"broken-{i}",
                SerializeKey = "missing"
            });
        }

        await using var enumerator = manager.StreamEntriesAsync().GetAsyncEnumerator();

        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.AreEqual("valid", enumerator.Current.Destination);
        Assert.IsFalse(await enumerator.MoveNextAsync());
        Assert.AreEqual(1, manager.Count);
    }

    [TestMethod]
    public void Clear_RemovesOneToOnePayload()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        var manager = new BrowseHistoryPersistentManager(db, logger);
        for (var i = 0; i < 45; i++)
            manager.AddOrReplace(new(CreatePost(i.ToString())));

        manager.Clear();

        Assert.AreEqual(0, manager.Count);
        Assert.AreEqual(0, db.Table<ArtworkPayloadEntry>().Count());
    }

    [TestMethod]
    public async Task SimplePersistentManager_StreamEntriesAsyncPagesNewestFirst()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new SearchHistoryPersistentManager(db);
        for (var i = 0; i < 45; i++)
            manager.Insert(new() { Value = i.ToString() });

        var values = new List<string>();
        await foreach (var entry in manager.StreamEntriesAsync(7))
            values.Add(entry.Value);

        Assert.HasCount(38, values);
        for (var i = 0; i < values.Count; i++)
            Assert.AreEqual((37 - i).ToString(), values[i]);
    }

    [TestMethod]
    public void SearchHistory_UpsertReplacesValueAndReturnsPersistedEntry()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new SearchHistoryPersistentManager(db);
        var first = manager.Upsert(new()
        {
            Value = "query",
            TranslatedName = "old"
        });
        var entry = new SearchHistoryEntry
        {
            Value = "query",
            TranslatedName = "new"
        };

        var result = manager.Upsert(entry);

        Assert.AreSame(entry, result);
        Assert.AreNotEqual(first.HistoryEntryId, result.HistoryEntryId);
        Assert.AreNotEqual(0, result.HistoryEntryId);
        Assert.AreEqual(1, manager.Count);
        Assert.AreEqual("new", manager.GetByValue("query")?.TranslatedName);
    }

    [TestMethod]
    public async Task StreamAsync_LoadsPayloadAndKeepsOneToOneRelationship()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        var manager = new BrowseHistoryPersistentManager(db, logger);
        var post = CreatePost("1");
        var changedCount = 0;
        manager.Changed += (_, _) => changedCount++;

        manager.AddOrReplace(new(post));
        manager.AddOrReplace(new(post));

        Assert.AreEqual(2, changedCount);
        Assert.AreEqual(1, manager.Count);
        Assert.AreEqual(1, db.Table<ArtworkPayloadEntry>().Count());
        await using var enumerator = manager.StreamAsync(SimpleWorkType.Illustration).GetAsyncEnumerator();
        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.AreEqual(post.Id.Id, enumerator.Current.Id);
    }

    [TestMethod]
    public async Task SearchHistory_AddOrUpdateKeepsOnlyNewestEntry()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new SearchHistoryPersistentManager(db);
        manager.AddOrUpdate(new()
        {
            Value = "same",
            TranslatedName = "old"
        });
        manager.AddOrUpdate(new()
        {
            Value = "same",
            TranslatedName = "new"
        });

        await using var enumerator = manager.StreamEntriesAsync().GetAsyncEnumerator();

        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.AreEqual("new", enumerator.Current.TranslatedName);
        Assert.IsFalse(await enumerator.MoveNextAsync());
        Assert.AreEqual(1, manager.Count);
    }

    [TestMethod]
    public async Task StreamEntriesAsync_AppliesInitialSkipAndContinuesAcrossPages()
    {
        using var db = new SQLiteConnection(":memory:");
        var commands = new List<string>();
        db.Trace = true;
        db.Tracer = commands.Add;
        var logger = CreateLogger();
        var manager = new DownloadHistoryPersistentManager(db, logger);
        for (var i = 0; i < 45; i++)
            manager.Insert(new(i.ToString(), CreatePost(i.ToString())));

        commands.Clear();
        var destinations = new List<string>();
        await foreach (var entry in manager.StreamEntriesAsync(7))
            destinations.Add(entry.Destination);

        Assert.HasCount(38, destinations);
        for (var i = 0; i < destinations.Count; i++)
            Assert.AreEqual((37 - i).ToString(), destinations[i]);
        Assert.HasCount(3, commands);
    }

    [TestMethod]
    public void SubscriptionHistory_ContainsIdentityDoesNotLoadPayload()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        var manager = new SubscriptionDownloadHistoryPersistentManager(db, logger);
        var commands = new List<string>();
        for (var i = 0; i < 45; i++)
            manager.Insert(new(i.ToString(), CreatePost(i.ToString()), i % 2 + 1));
        db.Trace = true;
        db.Tracer = commands.Add;

        Assert.IsTrue(manager.ContainsIdentity(2, "43", "43"));
        Assert.IsFalse(manager.ContainsIdentity(1, "43", "43"));
        Assert.DoesNotContain(command => command.Contains(nameof(ArtworkPayloadEntry), StringComparison.Ordinal), commands);
    }

    [TestMethod]
    public async Task AddOrReplace_ReplacesDestinationAndPayload()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new DownloadHistoryPersistentManager(db, CreateLogger());
        manager.Insert(new("same", CreatePost("old")));
        manager.AddOrReplace(new("same", CreatePost("new")));

        Assert.AreEqual(1, manager.Count);
        Assert.AreEqual(1, db.Table<ArtworkPayloadEntry>().Count());
        await using var enumerator = manager.StreamEntriesAsync().GetAsyncEnumerator();
        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.AreEqual("new", enumerator.Current.Entry.Id);
        Assert.IsFalse(await enumerator.MoveNextAsync());
    }

    [TestMethod]
    public async Task SubscriptionHistory_AddOrReplaceUsesCompositeIdentity()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new SubscriptionDownloadHistoryPersistentManager(db, CreateLogger());
        manager.AddOrReplace(new("same", CreatePost("artwork"), 1));
        var firstHistoryEntryId = db.Table<SubscriptionDownloadHistoryEntry>().Single().HistoryEntryId;

        manager.AddOrReplace(new("same", CreatePost("artwork"), 1));
        manager.AddOrReplace(new("same", CreatePost("artwork"), 2));
        manager.AddOrReplace(new("same", CreatePost("other"), 1));

        Assert.AreEqual(3, manager.Count);
        Assert.AreEqual(3, db.Table<ArtworkPayloadEntry>().Count());
        Assert.AreNotEqual(
            firstHistoryEntryId,
            db.Table<SubscriptionDownloadHistoryEntry>()
                .Single(entry => entry.WorkSubscriptionId == 1 && entry.ArtworkId == "artwork")
                .HistoryEntryId);
        var identities = new List<(int SubscriptionId, string ArtworkId, string Destination)>();
        await foreach (var entry in manager.StreamEntriesAsync())
            identities.Add((entry.WorkSubscriptionId, entry.ArtworkId, entry.Destination));
        CollectionAssert.AreEquivalent(
            new[] { (1, "artwork", "same"), (2, "artwork", "same"), (1, "other", "same") },
            identities);
    }

    [TestMethod]
    public void DownloadHistory_ClearOnlyRemovesOwnedPayloads()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        var ordinaryManager = new DownloadHistoryPersistentManager(db, logger);
        var subscriptionManager = new SubscriptionDownloadHistoryPersistentManager(db, logger);
        ordinaryManager.AddOrReplace(new("ordinary", CreatePost("ordinary")));
        subscriptionManager.AddOrReplace(new("subscription", CreatePost("subscription"), 1));
        Assert.AreEqual(2, db.Table<ArtworkPayloadEntry>().Count());

        ordinaryManager.Clear();

        Assert.AreEqual(0, ordinaryManager.Count);
        Assert.AreEqual(1, subscriptionManager.Count);
        Assert.AreEqual(1, db.Table<ArtworkPayloadEntry>().Count());

        subscriptionManager.Clear();

        Assert.AreEqual(0, subscriptionManager.Count);
        Assert.AreEqual(0, db.Table<ArtworkPayloadEntry>().Count());
    }

    [TestMethod]
    public void DownloadHistoryTables_HaveSeparateSchemas()
    {
        using var db = new SQLiteConnection(":memory:");
        var logger = CreateLogger();
        _ = new DownloadHistoryPersistentManager(db, logger);
        _ = new SubscriptionDownloadHistoryPersistentManager(db, logger);
        var ordinaryColumns = db.GetTableInfo(nameof(DownloadHistoryEntry))
            .Select(static column => column.Name)
            .ToArray();
        var subscriptionColumns = db.GetTableInfo(nameof(SubscriptionDownloadHistoryEntry))
            .Select(static column => column.Name)
            .ToArray();

        CollectionAssert.DoesNotContain(ordinaryColumns, nameof(SubscriptionDownloadHistoryEntry.WorkSubscriptionId));
        CollectionAssert.DoesNotContain(ordinaryColumns, nameof(SubscriptionDownloadHistoryEntry.ArtworkId));
        Assert.Contains(nameof(SubscriptionDownloadHistoryEntry.WorkSubscriptionId), subscriptionColumns);
        Assert.Contains(nameof(SubscriptionDownloadHistoryEntry.ArtworkId), subscriptionColumns);
    }

    [TestMethod]
    public async Task LoginUsers_LoadRestoresCurrentSelection()
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new LoginUserPersistentManager(db);
        var user = new LoginUserEntry
        {
            RefreshToken = "refresh-token",
            UserId = 1,
            Name = "user"
        };
        manager.Insert(user);
        var viewModel = new LoginPageViewModel(manager, user.HistoryEntryId);

        await viewModel.LoadUsersAsync();

        Assert.AreEqual(user.HistoryEntryId, viewModel.SelectedUser?.HistoryEntryId);
        Assert.AreEqual(user.RefreshToken, viewModel.RefreshToken);
    }

    [TestMethod]
    public void LoginUsers_UpsertReturnsUpdatedEntryWithoutDuplicateReadback()
    {
        using var db = new SQLiteConnection(":memory:");
        var commands = new List<string>();
        var manager = new LoginUserPersistentManager(db);
        var existing = manager.Upsert(new()
        {
            RefreshToken = "old-token",
            UserId = 1,
            Name = "old-name"
        });
        db.Trace = true;
        db.Tracer = commands.Add;

        var result = manager.Upsert(new()
        {
            RefreshToken = "new-token",
            UserId = 1,
            Name = "new-name"
        });

        Assert.AreEqual(existing.HistoryEntryId, result.HistoryEntryId);
        Assert.AreEqual("new-token", result.RefreshToken);
        Assert.AreEqual("new-name", result.Name);
        Assert.HasCount(2, commands);
    }

    private static FileLogger CreateLogger() => new(Path.Combine(
        Path.GetTempPath(),
        nameof(ArtworkHistoryPersistentManagerTest),
        Guid.NewGuid().ToString("N")));

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
}
