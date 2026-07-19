using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Imouto.BooruParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.Download;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Download.Tasks;
using Pixeval.Models.Options;
using Pixeval.ViewModels;
using SQLite;

namespace Pixeval.Tests;

[TestClass]
public sealed class DownloadManagerTest
{
    [TestMethod]
    public void QueueTask_ReplacesViewModelAndStaleRemovalRemovesReplacement()
    {
        using var httpClient = new HttpClient();
        using var manager = new DownloadManager(httpClient, 1);
        using var db = new SQLiteConnection(":memory:");
        using var viewModel = new DownloadPageViewModel(
            manager.QueuedTasks,
            new WorkSubscriptionPersistentManager(db));
        var original = new TestDownloadTaskGroup("same", "1");
        var other = new TestDownloadTaskGroup("other", "2");
        var replacement = new TestDownloadTaskGroup("same", "3");

        manager.QueueTask(original);
        manager.QueueTask(other);
        manager.QueueTask(replacement);

        Assert.HasCount(2, manager.QueuedTasks);
        Assert.IsTrue(original.IsCancelled);
        Assert.AreSame(replacement, manager.QueuedTasks[0]);
        Assert.HasCount(2, viewModel.OrdinaryItems);
        Assert.AreSame(replacement, viewModel.OrdinaryItems[0].DownloadTask);

        Assert.IsTrue(manager.TryRemoveTask(original));
        Assert.IsTrue(replacement.IsCancelled);
        Assert.HasCount(1, manager.QueuedTasks);
        Assert.AreSame(other, manager.QueuedTasks[0]);
        Assert.HasCount(1, viewModel.OrdinaryItems);
        Assert.AreSame(other, viewModel.OrdinaryItems[0].DownloadTask);
        Assert.IsFalse(manager.TryRemoveTask(original));
    }

    [TestMethod]
    public void QueueTask_SubscriptionIdentityAllowsSameDestinationToCoexist()
    {
        using var httpClient = new HttpClient();
        using var manager = new DownloadManager(httpClient, 1);
        var first = new TestDownloadTaskGroup("same", "artwork", 1);
        var otherSubscription = new TestDownloadTaskGroup("same", "artwork", 2);
        var otherArtwork = new TestDownloadTaskGroup("same", "other", 1);
        var replacement = new TestDownloadTaskGroup("same", "artwork", 1);

        manager.QueueTask(first);
        manager.QueueTask(otherSubscription);
        manager.QueueTask(otherArtwork);
        manager.QueueTask(replacement);

        Assert.HasCount(3, manager.QueuedTasks);
        Assert.AreSame(replacement, manager.QueuedTasks[0]);
        Assert.Contains(otherSubscription, manager.QueuedTasks);
        Assert.Contains(otherArtwork, manager.QueuedTasks);
    }

    [TestMethod]
    public void ViewModel_ProjectsOrdinaryAndSubscriptionSources()
    {
        using var httpClient = new HttpClient();
        using var manager = new DownloadManager(httpClient, 1);
        using var db = new SQLiteConnection(":memory:");
        var subscriptionManager = new WorkSubscriptionPersistentManager(db);
        var subscription = subscriptionManager.Upsert(new()
        {
            Id = 1,
            SubscriptionType = WorkSubscriptionType.Posts,
            WorkKind = WorkSubscriptionWorkKind.Illustration,
            Name = "Subscription"
        });
        using var viewModel = new DownloadPageViewModel(manager.QueuedTasks, subscriptionManager);
        var ordinary = new TestDownloadTaskGroup("ordinary", "ordinary");
        var subscriptionTask = new TestDownloadTaskGroup(
            "subscription",
            "subscription",
            subscription.HistoryEntryId);

        manager.QueueTask(ordinary);
        manager.QueueTask(subscriptionTask);

        Assert.HasCount(1, viewModel.OrdinaryItems);
        Assert.AreSame(ordinary, viewModel.OrdinaryItems[0].DownloadTask);
        Assert.HasCount(1, viewModel.SubscriptionFolders);
        var folder = viewModel.SubscriptionFolders[0];
        Assert.HasCount(1, folder.Items);
        Assert.AreSame(subscriptionTask, folder.Items[0].DownloadTask);

        Assert.HasCount(1, viewModel.OrdinaryItems);
        Assert.AreSame(ordinary, viewModel.OrdinaryItems[0].DownloadTask);

        Assert.IsTrue(manager.TryRemoveTask(subscriptionTask));
        Assert.HasCount(1, viewModel.SubscriptionFolders);
        Assert.IsEmpty(viewModel.SubscriptionFolders[0].Items);
    }

    [TestMethod]
    public void ViewModel_PreservesSubscriptionSourceOrderDuringInitialProjection()
    {
        using var db = new SQLiteConnection(":memory:");
        var subscriptionManager = new WorkSubscriptionPersistentManager(db);
        var subscription = subscriptionManager.Upsert(new()
        {
            Id = 1,
            SubscriptionType = WorkSubscriptionType.Posts,
            WorkKind = WorkSubscriptionWorkKind.Illustration,
            Name = "Subscription"
        });
        var newest = new TestDownloadTaskGroup("newest", "newest", subscription.HistoryEntryId);
        var older = new TestDownloadTaskGroup("older", "older", subscription.HistoryEntryId);
        ObservableCollection<IDownloadTaskGroupBase> source = [newest, older];
        using var viewModel = new DownloadPageViewModel(source, subscriptionManager);

        var folder = viewModel.SubscriptionFolders[0];
        Assert.AreSame(newest, folder.Items[0].DownloadTask);
        Assert.AreSame(older, folder.Items[1].DownloadTask);
    }

    [TestMethod]
    public async Task ViewModel_DisplaysSubscriptionsWithoutTasks()
    {
        using var db = new SQLiteConnection(":memory:");
        var subscriptionManager = new WorkSubscriptionPersistentManager(db);
        _ = subscriptionManager.Upsert(new()
        {
            Id = 1,
            SubscriptionType = WorkSubscriptionType.Posts,
            WorkKind = WorkSubscriptionWorkKind.Illustration,
            Name = "Subscription"
        });
        using var httpClient = new HttpClient();
        using var manager = new DownloadManager(httpClient, 1);
        using var viewModel = new DownloadPageViewModel(manager.QueuedTasks, subscriptionManager);

        await viewModel.SubscriptionFoldersLoadTask;

        Assert.HasCount(1, viewModel.SubscriptionFolders);
        Assert.IsEmpty(viewModel.SubscriptionFolders[0].Items);
    }

    private sealed class TestDownloadTaskGroup(
        string destination,
        string id,
        int? workSubscriptionId = null) : IDownloadTaskGroup
    {
        public DownloadHistoryEntryBase DatabaseEntry { get; } =
            DownloadHistoryEntryBase.Create(destination, CreatePost(id), workSubscriptionId);

        public string Id => DatabaseEntry.Entry.Id;

        public double ProgressPercentage => 100;

        public DownloadState CurrentState => DownloadState.Completed;

        public string Destination => DatabaseEntry.Destination;

        public string? ErrorMessage => null;

        public string OpenLocalDestination => Destination;

        public bool IsProcessing => false;

        public int ActiveCount => 0;

        public int CompletedCount => 1;

        public int ErrorCount => 0;

        public int Count => 0;

        public bool IsCancelled { get; private set; }

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add { }
            remove { }
        }

        event PropertyChangingEventHandler? INotifyPropertyChanging.PropertyChanging
        {
            add { }
            remove { }
        }

        public ValueTask InitializeTaskGroupAsync() => ValueTask.CompletedTask;

        public void SubscribeProgress(ChannelWriter<DownloadToken> writer)
        {
        }

        public DownloadToken GetToken() => new(this, CancellationToken.None);

        public void Reset()
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void Cancel() => IsCancelled = true;

        public void Delete()
        {
        }

        public void Dispose()
        {
        }

        public IEnumerator<ISingleDownloadTaskBase> GetEnumerator() =>
            ((IEnumerable<ISingleDownloadTaskBase>) []).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
}
