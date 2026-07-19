// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities;

namespace Pixeval.Models.Database.Managers;

public sealed class HistoryPersistHelper : IDisposable
{
    private readonly BrowseHistoryPersistentManager _browseHistoryPersistentManager;
    private readonly DownloadHistoryPersistentManager _downloadHistoryPersistentManager;
    private readonly SubscriptionDownloadHistoryPersistentManager _subscriptionDownloadHistoryPersistentManager;
    private readonly SearchHistoryPersistentManager _searchHistoryPersistentManager;
    private readonly WatchLaterPersistentManager _watchLaterPersistentManager;
    private readonly CancellationTokenSource _downloadRestoreCancellationTokenSource = new();
    private readonly HashSet<DownloadTaskKey> _removedDownloadHistoryKeys = [];
    private readonly HashSet<string> _removedSearchHistoryValues = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _searchRestoreCancellationTokenSource = new();
    private readonly FileLogger _logger;
    private bool _isDownloadHistoryRestoreCompleted;
    private bool _isDisposed;
    private bool _isRestoringDownloadHistory;
    private bool _isRestoringSearchHistory;
    private bool _isSearchHistoryRestoreCompleted;
    private bool _isUpdatingSearchHistory;

    public HistoryPersistHelper(
        [FromKeyedServices(IPlatformInfo.Pixiv)]
        IDownloadHttpClientService service,
        DownloadHistoryPersistentManager downloadHistoryPersistentManager,
        SubscriptionDownloadHistoryPersistentManager subscriptionDownloadHistoryPersistentManager,
        SearchHistoryPersistentManager searchHistoryPersistentManager,
        BrowseHistoryPersistentManager browseHistoryPersistentManager,
        WatchLaterPersistentManager watchLaterPersistentManager,
        FileLogger logger)
    {
        _browseHistoryPersistentManager = browseHistoryPersistentManager;
        _downloadHistoryPersistentManager = downloadHistoryPersistentManager;
        _subscriptionDownloadHistoryPersistentManager = subscriptionDownloadHistoryPersistentManager;
        _logger = logger;
        _searchHistoryPersistentManager = searchHistoryPersistentManager;
        _watchLaterPersistentManager = watchLaterPersistentManager;
        BrowseHistorySource = browseHistoryPersistentManager;
        WatchLaterSource = watchLaterPersistentManager;

        DownloadManager = new(
            service.GetImageDownloadClient(),
            App.AppViewModel.AppSettings.DownloadSettings.MaxDownloadTaskConcurrencyLevel);
        SearchHistoryEntries = [];
        SearchHistoryEntries.CollectionChanged += OnSearchHistoryCollectionChanged;
        DownloadManager.QueuedTasks.CollectionChanged += OnDownloadHistoryCollectionChanged;
        RestoreTask = Task.WhenAll(
            RestoreSafelyAsync(
                RestoreSearchHistoryAsync,
                nameof(RestoreSearchHistoryAsync),
                _searchRestoreCancellationTokenSource.Token),
            RestoreSafelyAsync(
                RestoreDownloadHistoryAsync,
                nameof(RestoreDownloadHistoryAsync),
                _downloadRestoreCancellationTokenSource.Token));
    }

    public DownloadManager DownloadManager { get; }

    public ObservableCollection<SearchHistoryEntry> SearchHistoryEntries { get; }

    public Task RestoreTask { get; }

    public IArtworkHistorySource BrowseHistorySource { get; }

    public IArtworkHistorySource WatchLaterSource { get; }

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
        _ = _removedSearchHistoryValues.Remove(text);
        _searchHistoryPersistentManager.AddOrUpdate(searchHistoryEntry);
        _isUpdatingSearchHistory = true;
        try
        {
            if (SearchHistoryEntries.FirstOrDefault(entry => entry.Value == text) is { } existing)
                SearchHistoryEntries.Remove(existing);
            SearchHistoryEntries.Insert(0, searchHistoryEntry);
        }
        finally
        {
            _isUpdatingSearchHistory = false;
        }
    }

    public void AddBrowseHistory(IArtworkInfo entry)
    {
        if (!BrowseHistoryEntry.TryCreateWorkKey(entry, out _))
            return;
        _browseHistoryPersistentManager.AddOrReplace(new(entry));
    }

    public bool ContainsWatchLater(IArtworkInfo entry) =>
        WatchLaterEntry.TryCreateWorkKey(entry, out var workKey)
        && _watchLaterPersistentManager.ContainsWorkKey(workKey);

    public bool AddWatchLater(IArtworkInfo entry)
    {
        if (!WatchLaterEntry.TryCreateWorkKey(entry, out _))
            return false;
        _watchLaterPersistentManager.AddOrReplace(new(entry));
        return true;
    }

    public bool RemoveWatchLater(IArtworkInfo entry) =>
        WatchLaterEntry.TryCreateWorkKey(entry, out var workKey)
        && _watchLaterPersistentManager.TryDeleteByWorkKey(workKey);

    public void ClearBrowseHistory() => _browseHistoryPersistentManager.Clear();

    public void UpdateDownloadHistory(DownloadHistoryEntryBase entry)
    {
        switch (entry)
        {
            case DownloadHistoryEntry downloadEntry:
                _downloadHistoryPersistentManager.Update(downloadEntry);
                break;
            case SubscriptionDownloadHistoryEntry subscriptionEntry:
                _subscriptionDownloadHistoryPersistentManager.Update(subscriptionEntry);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entry));
        }
    }

    private void OnSearchHistoryCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs args)
    {
        if (_isDisposed || _isRestoringSearchHistory || _isUpdatingSearchHistory)
            return;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (args.NewItems is { } newItems)
                    foreach (var newItem in newItems.OfType<SearchHistoryEntry>())
                    {
                        _ = _removedSearchHistoryValues.Remove(newItem.Value);
                        _searchHistoryPersistentManager.AddOrUpdate(newItem);
                    }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is { } oldItems)
                    foreach (var oldItem in oldItems.OfType<SearchHistoryEntry>())
                    {
                        if (!_isSearchHistoryRestoreCompleted)
                            _ = _removedSearchHistoryValues.Add(oldItem.Value);
                        _ = _searchHistoryPersistentManager.TryDeleteByValue(oldItem.Value);
                    }

                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.OldItems is { } replacedItems)
                    foreach (var oldItem in replacedItems.OfType<SearchHistoryEntry>())
                        if (args.NewItems?.OfType<SearchHistoryEntry>().Any(newItem =>
                                newItem.Value == oldItem.Value) is not true)
                        {
                            if (!_isSearchHistoryRestoreCompleted)
                                _ = _removedSearchHistoryValues.Add(oldItem.Value);
                            _ = _searchHistoryPersistentManager.TryDeleteByValue(oldItem.Value);
                        }

                if (args.NewItems is { } replacementItems)
                    foreach (var newItem in replacementItems.OfType<SearchHistoryEntry>())
                    {
                        _ = _removedSearchHistoryValues.Remove(newItem.Value);
                        _searchHistoryPersistentManager.AddOrUpdate(newItem);
                    }

                break;
            case NotifyCollectionChangedAction.Reset when args.NewItems is not { Count: > 0 }:
                _searchRestoreCancellationTokenSource.Cancel();
                _searchHistoryPersistentManager.Clear();
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args.Action), args.Action, null);
        }
    }

    private void OnDownloadHistoryCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs args)
    {
        if (_isDisposed || _isRestoringDownloadHistory)
            return;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (args.NewItems is { } newItems)
                    foreach (var newItem in newItems.OfType<IDownloadTaskGroup>())
                    {
                        _ = _removedDownloadHistoryKeys.Remove(newItem.Key);
                        AddOrReplaceDownloadHistory(newItem.DatabaseEntry);
                    }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is { } oldItems)
                    foreach (var oldItem in oldItems.OfType<IDownloadTaskGroup>())
                    {
                        if (!_isDownloadHistoryRestoreCompleted)
                            _ = _removedDownloadHistoryKeys.Add(oldItem.Key);
                        _ = TryDeleteDownloadHistory(oldItem.DatabaseEntry);
                    }

                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.NewItems is { } replacementItems)
                    foreach (var newItem in replacementItems.OfType<IDownloadTaskGroup>())
                    {
                        _ = _removedDownloadHistoryKeys.Remove(newItem.Key);
                        AddOrReplaceDownloadHistory(newItem.DatabaseEntry);
                    }

                if (args.OldItems is { } replacedItems)
                    foreach (var oldItem in replacedItems.OfType<IDownloadTaskGroup>())
                        if (args.NewItems?.OfType<IDownloadTaskGroup>().Any(newItem =>
                                newItem.Key == oldItem.Key) is not true)
                        {
                            if (!_isDownloadHistoryRestoreCompleted)
                                _ = _removedDownloadHistoryKeys.Add(oldItem.Key);
                            _ = TryDeleteDownloadHistory(oldItem.DatabaseEntry);
                        }

                break;
            case NotifyCollectionChangedAction.Reset when args.NewItems is not { Count: > 0 }:
                _downloadRestoreCancellationTokenSource.Cancel();
                _downloadHistoryPersistentManager.Clear();
                _subscriptionDownloadHistoryPersistentManager.Clear();
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args.Action), args.Action, null);
        }
    }

    private void AddOrReplaceDownloadHistory(DownloadHistoryEntryBase entry)
    {
        switch (entry)
        {
            case DownloadHistoryEntry downloadEntry:
                _downloadHistoryPersistentManager.AddOrReplace(downloadEntry);
                break;
            case SubscriptionDownloadHistoryEntry subscriptionEntry:
                _subscriptionDownloadHistoryPersistentManager.AddOrReplace(subscriptionEntry);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entry));
        }
    }

    private bool TryDeleteDownloadHistory(DownloadHistoryEntryBase entry) => entry switch
    {
        DownloadHistoryEntry downloadEntry =>
            _downloadHistoryPersistentManager.TryDeleteByDestination(downloadEntry.Destination),
        SubscriptionDownloadHistoryEntry subscriptionEntry =>
            _subscriptionDownloadHistoryPersistentManager.TryDeleteByIdentity(
                subscriptionEntry.WorkSubscriptionId,
                subscriptionEntry.ArtworkId,
                subscriptionEntry.Destination),
        _ => throw new ArgumentOutOfRangeException(nameof(entry))
    };

    private async Task RestoreSafelyAsync(
        Func<CancellationToken, Task> restoreAsync,
        string operationName,
        CancellationToken token)
    {
        try
        {
            await restoreAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(operationName, e);
        }
    }

    private async Task RestoreSearchHistoryAsync(CancellationToken token)
    {
        try
        {
            await foreach (var entry in _searchHistoryPersistentManager
                               .StreamEntriesAsync(token: token)
                               .ConfigureAwait(false))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    token.ThrowIfCancellationRequested();
                    if (_removedSearchHistoryValues.Contains(entry.Value)
                        || SearchHistoryEntries.Any(item => item.Value == entry.Value))
                        return;

                    _isRestoringSearchHistory = true;
                    try
                    {
                        SearchHistoryEntries.Add(entry);
                    }
                    finally
                    {
                        _isRestoringSearchHistory = false;
                    }
                });
            }
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _isSearchHistoryRestoreCompleted = true;
                _removedSearchHistoryValues.Clear();
            });
        }
    }

    private async Task RestoreDownloadHistoryAsync(CancellationToken token)
    {
        try
        {
            await Task.WhenAll(
                    RestoreSourceAsync(_downloadHistoryPersistentManager.StreamTaskGroupsAsync(token: token)),
                    RestoreSourceAsync(_subscriptionDownloadHistoryPersistentManager.StreamTaskGroupsAsync(token: token)))
                .ConfigureAwait(false);

            return;

            async Task RestoreSourceAsync(IAsyncEnumerable<IDownloadTaskGroup> source)
            {
                await foreach (var taskGroup in source.ConfigureAwait(false))
                {
                    var isOwnedByDownloadManager = false;
                    try
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            token.ThrowIfCancellationRequested();
                            if (_removedDownloadHistoryKeys.Contains(taskGroup.Key))
                                return;

                            _isRestoringDownloadHistory = true;
                            try
                            {
                                isOwnedByDownloadManager = DownloadManager.TryRestoreTask(taskGroup);
                            }
                            finally
                            {
                                _isRestoringDownloadHistory = false;
                            }
                        });
                    }
                    finally
                    {
                        if (!isOwnedByDownloadManager)
                            taskGroup.Dispose();
                    }
                }
            }
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _isDownloadHistoryRestoreCompleted = true;
                _removedDownloadHistoryKeys.Clear();
            });
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        _searchRestoreCancellationTokenSource.Cancel();
        _downloadRestoreCancellationTokenSource.Cancel();
        SearchHistoryEntries.CollectionChanged -= OnSearchHistoryCollectionChanged;
        DownloadManager.QueuedTasks.CollectionChanged -= OnDownloadHistoryCollectionChanged;
        DownloadManager.Dispose();
        _searchRestoreCancellationTokenSource.Dispose();
        _downloadRestoreCancellationTokenSource.Dispose();
    }
}
