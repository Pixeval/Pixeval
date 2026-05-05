// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
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
    public HistoryPersistHelper()
    {
        DownloadManager = new DownloadManager(App.AppViewModel.MakoClient.GetImageDownloadClient(), App.AppViewModel.AppSettings.MaxDownloadTaskConcurrencyLevel);

        var downloadHistoryPersistentManager = AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var searchHistoryPersistentManager = AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        var browseHistoryPersistentManager = AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();

        foreach (var downloadTaskGroup in downloadHistoryPersistentManager)
            DownloadManager.QueueTask(downloadTaskGroup);
        SearchHistoryEntries = new(searchHistoryPersistentManager);
        BrowseHistoryEntries = new(browseHistoryPersistentManager.Queryable.Select(t => t.Entry));

        SearchHistoryEntries.CollectionChanged += (s, e) =>
            OnCollectionChangedEventHandler<SearchHistoryPersistentManager, SearchHistoryEntry, SearchHistoryEntry>(s,
                e, (m, t) => m.TryDelete(x => x.Value == t.Value), t => t);

        BrowseHistoryEntries.CollectionChanged += (s, e) =>
            OnCollectionChangedEventHandler<BrowseHistoryPersistentManager, BrowseHistoryEntry, IArtworkInfo>(s, e,
                (m, t) =>
                {
                    if (t is ISerializable { SerializeKey: { } key })
                        m.TryDelete(x =>
                            x.SerializeKey == key
                            && x.Id == t.Id);
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
        SearchHistoryEntries.Add(searchHistoryEntry);
    }

    public void AddBrowseHistory(IArtworkInfo entry)
    {
        if (entry.Id is "")
            return;
        BrowseHistoryEntries.Add(entry);
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
                    {
                        tryDelete(manager, newItem);
                        manager.Insert(convert(newItem));
                    }

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
                    {
                        tryDelete(manager, newItem);
                        manager.Insert(convert(newItem));
                    }
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
