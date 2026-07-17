using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Imouto.BooruParser;
using Mako.Global.Enum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.Download;
using Pixeval.Models.Database;
using Pixeval.Models.Download.Tasks;
using Pixeval.ViewModels;

namespace Pixeval.Tests;

[TestClass]
public sealed class DownloadManagerTest
{
    [TestMethod]
    public void QueueTask_ReplacesViewModelAndStaleRemovalRemovesReplacement()
    {
        using var httpClient = new HttpClient();
        using var manager = new DownloadManager(httpClient, 1);
        using var viewModel = new DownloadViewViewModel(manager.QueuedTasks);
        var original = new TestDownloadTaskGroup("same", "1");
        var other = new TestDownloadTaskGroup("other", "2");
        var replacement = new TestDownloadTaskGroup("same", "3");

        manager.QueueTask(original);
        manager.QueueTask(other);
        manager.QueueTask(replacement);

        Assert.AreEqual(2, manager.QueuedTasks.Count);
        Assert.AreSame(replacement, manager.QueuedTasks[0]);
        Assert.AreEqual(2, viewModel.View.Count);
        Assert.AreSame(replacement, ((DownloadItemViewModel) viewModel.View[0]).DownloadTask);

        Assert.IsTrue(manager.TryRemoveTask(original));
        Assert.IsTrue(replacement.IsCancelled);
        Assert.AreEqual(1, manager.QueuedTasks.Count);
        Assert.AreSame(other, manager.QueuedTasks[0]);
        Assert.AreEqual(1, viewModel.View.Count);
        Assert.AreSame(other, ((DownloadItemViewModel) viewModel.View[0]).DownloadTask);
        Assert.IsFalse(manager.TryRemoveTask(original));
    }

    private sealed class TestDownloadTaskGroup(string destination, string id) : IDownloadTaskGroup
    {
        public DownloadHistoryEntry DatabaseEntry { get; } = new(destination, CreatePost(id));

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
            ((IEnumerable<ISingleDownloadTaskBase>) Array.Empty<ISingleDownloadTaskBase>()).GetEnumerator();

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
