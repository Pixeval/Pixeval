// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
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
    private readonly SearchHistoryPersistentManager _searchHistoryPersistentManager;
    private readonly WatchLaterPersistentManager _watchLaterPersistentManager;
    private readonly CancellationTokenSource _downloadRestoreCancellationTokenSource = new();
    private readonly CancellationTokenSource _searchRestoreCancellationTokenSource = new();
    private readonly FileLogger _logger;
    private bool _isDisposed;
    private bool _isRestoringDownloadHistory;
    private bool _isRestoringSearchHistory;
    private bool _isUpdatingSearchHistory;

    public HistoryPersistHelper(
        [FromKeyedServices(IPlatformInfo.Pixiv)]
        IDownloadHttpClientService service,
        DownloadHistoryPersistentManager downloadHistoryPersistentManager,
        SearchHistoryPersistentManager searchHistoryPersistentManager,
        BrowseHistoryPersistentManager browseHistoryPersistentManager,
        WatchLaterPersistentManager watchLaterPersistentManager,
        FileLogger logger)
    {
        _browseHistoryPersistentManager = browseHistoryPersistentManager;
        _downloadHistoryPersistentManager = downloadHistoryPersistentManager;
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
                        _searchHistoryPersistentManager.AddOrUpdate(newItem);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is { } oldItems)
                    foreach (var oldItem in oldItems.OfType<SearchHistoryEntry>())
                        _ = _searchHistoryPersistentManager.TryDeleteByValue(oldItem.Value);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.NewItems is { } replacementItems)
                    foreach (var newItem in replacementItems.OfType<SearchHistoryEntry>())
                        _searchHistoryPersistentManager.AddOrUpdate(newItem);
                if (args.OldItems is { } replacedItems)
                    foreach (var oldItem in replacedItems.OfType<SearchHistoryEntry>())
                        if (args.NewItems?.OfType<SearchHistoryEntry>().Any(newItem =>
                                newItem.Value == oldItem.Value) is not true)
                            _ = _searchHistoryPersistentManager.TryDeleteByValue(oldItem.Value);
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
                        _downloadHistoryPersistentManager.AddOrReplace(newItem.DatabaseEntry);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldItems is { } oldItems)
                    foreach (var oldItem in oldItems.OfType<IDownloadTaskGroup>())
                        _ = _downloadHistoryPersistentManager.TryDeleteByDestination(oldItem.DatabaseEntry.Destination);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.NewItems is { } replacementItems)
                    foreach (var newItem in replacementItems.OfType<IDownloadTaskGroup>())
                        _downloadHistoryPersistentManager.AddOrReplace(newItem.DatabaseEntry);
                if (args.OldItems is { } replacedItems)
                    foreach (var oldItem in replacedItems.OfType<IDownloadTaskGroup>())
                        if (args.NewItems?.OfType<IDownloadTaskGroup>().Any(newItem =>
                                newItem.DatabaseEntry.Destination == oldItem.DatabaseEntry.Destination) is not true)
                            _ = _downloadHistoryPersistentManager.TryDeleteByDestination(oldItem.DatabaseEntry.Destination);
                break;
            case NotifyCollectionChangedAction.Reset when args.NewItems is not { Count: > 0 }:
                _downloadRestoreCancellationTokenSource.Cancel();
                _downloadHistoryPersistentManager.Clear();
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args.Action), args.Action, null);
        }
    }

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
        await foreach (var entry in _searchHistoryPersistentManager
                           .StreamEntriesAsync(token: token)
                           .ConfigureAwait(false))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                token.ThrowIfCancellationRequested();
                if (SearchHistoryEntries.Any(item => item.Value == entry.Value))
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

    private async Task RestoreDownloadHistoryAsync(CancellationToken token)
    {
        await foreach (var taskGroup in _downloadHistoryPersistentManager
                           .StreamTaskGroupsAsync(token: token)
                           .ConfigureAwait(false))
        {
            var isOwnedByDownloadManager = false;
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    token.ThrowIfCancellationRequested();
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
