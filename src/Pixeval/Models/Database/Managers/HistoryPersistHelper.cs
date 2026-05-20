// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Download;
using Pixeval.Models.Download.Tasks;

namespace Pixeval.Models.Database.Managers;

public class HistoryPersistHelper : IDisposable
{
    public HistoryPersistHelper(
        [FromKeyedServices(IPlatformInfo.Pixiv)] IDownloadHttpClientService service,
        DownloadHistoryPersistentManager downloadHistoryPersistentManager,
        SearchHistoryPersistentManager searchHistoryPersistentManager,
        BrowseHistoryPersistentManager browseHistoryPersistentManager)
    {
        DownloadManager = new DownloadManager(service.GetImageDownloadClient(), App.AppViewModel.AppSettings.MaxDownloadTaskConcurrencyLevel);

        foreach (var downloadTaskGroup in downloadHistoryPersistentManager)
            DownloadManager.QueueTask(downloadTaskGroup);
        SearchHistoryEntries = new(searchHistoryPersistentManager.Reverse());
        BrowseHistoryEntries = new(browseHistoryPersistentManager.Queryable.Select(t => t.Entry).Reverse());

        SearchHistoryEntries.CollectionChanged += (s, e) =>
            OnCollectionChangedEventHandler<SearchHistoryPersistentManager, SearchHistoryEntry, SearchHistoryEntry>(s,
                e, (m, t) => m.TryDelete(x => x.Value == t.Value), t => t);

        BrowseHistoryEntries.CollectionChanged += (s, e) =>
            OnCollectionChangedEventHandler<BrowseHistoryPersistentManager, BrowseHistoryEntry, IArtworkInfo>(s, e,
                (m, t) =>
                {
                    if (BrowseHistoryEntry.TryCreateWorkKey(t, out var workKey))
                        m.TryDelete(x => x.WorkKey == workKey);
                }, t => new(t));

        DownloadManager.QueuedTasks.CollectionChanged += (s, e) =>
            OnCollectionChangedEventHandler<DownloadHistoryPersistentManager, DownloadHistoryEntry,
                IDownloadTaskGroupBase>(s, e, (m, t) =>
            {
                if (t is IDownloadTaskGroup { DatabaseEntry.Destination: { } dest })
                    m.TryDelete(x => x.Destination == dest);
            }, t => ((IDownloadTaskGroup) t).DatabaseEntry);
    }

    private static ServiceProvider AppServiceProvider => App.AppViewModel.AppServiceProvider;

    public DownloadManager DownloadManager { get; }

    public ObservableCollection<SearchHistoryEntry> SearchHistoryEntries { get; }

    public ObservableCollection<IArtworkInfo> BrowseHistoryEntries { get; }

    public void AddSearchHistory(string text, string? translatedName = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        var searchHistoryEntry = new SearchHistoryEntry
        {
            Value = text,
            TranslatedName = translatedName,
            Time = DateTime.UtcNow
        };
        if (SearchHistoryEntries.FirstOrDefault(t => t.Value == text) is { } existing)
            SearchHistoryEntries.Remove(existing);
        SearchHistoryEntries.Insert(0, searchHistoryEntry);
    }

    public void AddBrowseHistory(IArtworkInfo entry)
    {
        if (!BrowseHistoryEntry.TryCreateWorkKey(entry, out var workKey))
            return;
        if (BrowseHistoryEntries.FirstOrDefault(t =>
                BrowseHistoryEntry.TryCreateWorkKey(t, out var itemWorkKey)
                && itemWorkKey == workKey) is { } e)
            BrowseHistoryEntries.Remove(e);
        BrowseHistoryEntries.Insert(0, entry);
    }

    private static void OnCollectionChangedEventHandler<TManager, TEntry, TItem>(
        object? sender,
        NotifyCollectionChangedEventArgs args,
        Action<TManager, TItem> tryDelete,
        Func<TItem, TEntry> convert)
        where TManager : IWriteOnlyPersistentManager<TEntry>
        where TEntry : HistoryEntry
    {
        var manager = AppServiceProvider.GetRequiredService<TManager>();
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (args.NewItems is not null)
                    foreach (var newItem in args.NewItems.OfType<TItem>())
                        manager.Insert(convert(newItem));
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is not null)
                    foreach (var oldItem in args.OldItems.OfType<TItem>())
                        tryDelete(manager, oldItem);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.NewItems is not null)
                    foreach (var oldItem in args.NewItems.OfType<TItem>())
                        tryDelete(manager, oldItem);
                if (args.NewItems is not null)
                    foreach (var newItem in args.NewItems.OfType<TItem>())
                        manager.Insert(convert(newItem));
                break;
            case NotifyCollectionChangedAction.Reset when args.NewItems is not { Count: > 0 }:
                manager.Clear();
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DownloadManager.Dispose();
    }
}
