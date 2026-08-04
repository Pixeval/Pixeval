// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Pixeval.Download;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Download.Tasks;
using Pixeval.Models.Subscriptions;

namespace Pixeval.ViewModels;

public sealed class DownloadPageViewModel : ViewModelBase, IDisposable
{
    private readonly ObservableCollection<IDownloadTaskGroupBase> _source;

    private readonly Dictionary<DownloadTaskKey, DownloadItemViewModel> _lookup = [];

    private readonly Dictionary<int, DownloadFolderViewModel> _subscriptionFolderLookup = [];

    private readonly WorkSubscriptionPersistentManager _workSubscriptionPersistentManager;

    private readonly IWorkSubscriptionService _workSubscriptionService;

    private readonly CancellationTokenSource _subscriptionFolderLoadCancellationTokenSource = new();

    private readonly bool _createdOnUiThread;

    public Task SubscriptionFoldersLoadTask { get; }

    private bool _isDisposed;

    public ObservableCollection<DownloadItemViewModel> OrdinaryItems { get; } = [];

    public ObservableCollection<DownloadFolderViewModel> SubscriptionFolders { get; } = [];

    public DownloadPageViewModel(
        ObservableCollection<IDownloadTaskGroupBase> source,
        WorkSubscriptionPersistentManager workSubscriptionPersistentManager,
        IWorkSubscriptionService workSubscriptionService)
    {
        _source = source;
        _workSubscriptionPersistentManager = workSubscriptionPersistentManager;
        _workSubscriptionService = workSubscriptionService;
        _createdOnUiThread = Dispatcher.UIThread.CheckAccess() && Application.Current is not null;
        _workSubscriptionService.FetchStateChanged += WorkSubscriptionServiceOnFetchStateChanged;
        _workSubscriptionService.SubscriptionUpdated += WorkSubscriptionServiceOnSubscriptionUpdated;
        _workSubscriptionService.SubscriptionRemoved += WorkSubscriptionServiceOnSubscriptionRemoved;
        foreach (var task in _source)
            AddTask(task, false);

        _source.CollectionChanged += SourceOnCollectionChanged;
        SubscriptionFoldersLoadTask = LoadSubscriptionFoldersAsync(_subscriptionFolderLoadCancellationTokenSource.Token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        _source.CollectionChanged -= SourceOnCollectionChanged;
        _workSubscriptionService.FetchStateChanged -= WorkSubscriptionServiceOnFetchStateChanged;
        _workSubscriptionService.SubscriptionUpdated -= WorkSubscriptionServiceOnSubscriptionUpdated;
        _workSubscriptionService.SubscriptionRemoved -= WorkSubscriptionServiceOnSubscriptionRemoved;
        _subscriptionFolderLoadCancellationTokenSource.Cancel();
        _subscriptionFolderLoadCancellationTokenSource.Dispose();
        DisposeEntries();
    }

    private void AddTask(IDownloadTaskGroupBase task, bool insertAtFront)
    {
        if (_isDisposed || task is not IDownloadTaskGroup group)
            return;

        if (_lookup.ContainsKey(group.Key))
            return;

        var vm = new DownloadItemViewModel(group);
        _lookup[group.Key] = vm;
        switch (group.DatabaseEntry)
        {
            case DownloadHistoryEntry:
                Insert(OrdinaryItems, vm, insertAtFront);
                break;
            case SubscriptionDownloadHistoryEntry subscriptionEntry
                when GetOrCreateFolder(subscriptionEntry.WorkSubscriptionId) is { } folder:
                folder.Add(vm, insertAtFront);
                break;
            case SubscriptionDownloadHistoryEntry:
                vm.Dispose();
                _ = _lookup.Remove(group.Key);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(group.DatabaseEntry));
        }

        return;

        static void Insert(
            ObservableCollection<DownloadItemViewModel> target,
            DownloadItemViewModel item,
            bool insertAtFront)
        {
            if (insertAtFront)
                target.Insert(0, item);
            else
                target.Add(item);
        }
    }

    private DownloadFolderViewModel? GetOrCreateFolder(int subscriptionEntryId)
    {
        if (subscriptionEntryId <= 0)
            return null;

        if (_subscriptionFolderLookup.TryGetValue(subscriptionEntryId, out var folder))
            return folder;

        if (_workSubscriptionPersistentManager.GetByKey(subscriptionEntryId) is not { } subscription)
            return null;

        return AddSubscriptionFolder(subscription);
    }

    private DownloadFolderViewModel AddSubscriptionFolder(WorkSubscriptionEntry subscription)
    {
        if (_subscriptionFolderLookup.TryGetValue(subscription.HistoryEntryId, out var existing))
            return existing;

        var folder = new DownloadFolderViewModel(subscription);
        folder.UpdateFetchState(_workSubscriptionService.CurrentFetchState);
        _subscriptionFolderLookup[subscription.HistoryEntryId] = folder;
        var index = 0;
        while (index < SubscriptionFolders.Count
               && SubscriptionFolders[index].Subscription.HistoryEntryId > subscription.HistoryEntryId)
            index++;
        SubscriptionFolders.Insert(index, folder);
        return folder;
    }

    private void RemoveTask(IDownloadTaskGroupBase task)
    {
        if (_isDisposed || !_lookup.Remove(task.Key, out var vm))
            return;

        if (vm.DownloadTask.DatabaseEntry is SubscriptionDownloadHistoryEntry subscriptionEntry
            && GetFolder(subscriptionEntry.WorkSubscriptionId) is { } folder)
        {
            _ = folder.Remove(vm);
        }
        else
        {
            _ = OrdinaryItems.Remove(vm);
        }

        vm.Dispose();
    }

    private void MoveTaskToFront(IDownloadTaskGroupBase task)
    {
        if (task is not IDownloadTaskGroup group
            || !_lookup.TryGetValue(group.Key, out var vm))
            return;

        if (group.DatabaseEntry is SubscriptionDownloadHistoryEntry subscriptionEntry
            && GetFolder(subscriptionEntry.WorkSubscriptionId) is { } folder)
        {
            var itemIndex = folder.Items.IndexOf(vm);
            if (itemIndex > 0)
                folder.Items.Move(itemIndex, 0);
            var folderIndex = SubscriptionFolders.IndexOf(folder);
            if (folderIndex > 0)
                SubscriptionFolders.Move(folderIndex, 0);
            return;
        }

        var viewIndex = OrdinaryItems.IndexOf(vm);
        if (viewIndex > 0)
            OrdinaryItems.Move(viewIndex, 0);
    }

    private DownloadFolderViewModel? GetFolder(int subscriptionEntryId) =>
        _subscriptionFolderLookup.GetValueOrDefault(subscriptionEntryId);

    private void WorkSubscriptionServiceOnFetchStateChanged(
        object? sender,
        WorkSubscriptionFetchState state)
    {
        if (_isDisposed)
            return;

        if (!_createdOnUiThread || Dispatcher.UIThread.CheckAccess())
        {
            ApplyFetchState(state);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_isDisposed)
                ApplyFetchState(state);
        });
    }

    private void ApplyFetchState(WorkSubscriptionFetchState state) =>
        (state.IsFetching
            ? GetOrCreateFolder(state.WorkSubscriptionId)
            : GetFolder(state.WorkSubscriptionId))
        ?.UpdateFetchState(state);

    private void WorkSubscriptionServiceOnSubscriptionRemoved(object? sender, int workSubscriptionId)
    {
        if (_isDisposed)
            return;

        if (!_createdOnUiThread || Dispatcher.UIThread.CheckAccess())
        {
            RemoveSubscriptionFolder(workSubscriptionId);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_isDisposed)
                RemoveSubscriptionFolder(workSubscriptionId);
        });
    }

    private void WorkSubscriptionServiceOnSubscriptionUpdated(
        object? sender,
        WorkSubscriptionEntry subscription)
    {
        if (_isDisposed)
            return;

        if (!_createdOnUiThread || Dispatcher.UIThread.CheckAccess())
        {
            GetFolder(subscription.HistoryEntryId)?.UpdateSubscription(subscription);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_isDisposed)
                GetFolder(subscription.HistoryEntryId)?.UpdateSubscription(subscription);
        });
    }

    private void RemoveSubscriptionFolder(int workSubscriptionId)
    {
        if (!_subscriptionFolderLookup.Remove(workSubscriptionId, out var folder))
            return;

        _ = SubscriptionFolders.Remove(folder);
        folder.Dispose();
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems is { } newItems:
                foreach (IDownloadTaskGroupBase item in newItems)
                {
                    AddTask(item, e.NewStartingIndex is 0);
                    if (e.NewStartingIndex is 0)
                        MoveTaskToFront(item);
                }

                break;
            case NotifyCollectionChangedAction.Remove when e.OldItems is { } oldItems:
                foreach (IDownloadTaskGroupBase item in oldItems)
                    RemoveTask(item);
                break;
            case NotifyCollectionChangedAction.Reset:
                ClearTaskEntries();
                foreach (var task in _source)
                    AddTask(task, false);
                break;
            case NotifyCollectionChangedAction.Replace
                when e is { OldItems: { } replacedItems, NewItems: { } replacementItems }:
                foreach (IDownloadTaskGroupBase item in replacedItems)
                    RemoveTask(item);
                foreach (IDownloadTaskGroupBase item in replacementItems)
                {
                    AddTask(item, e.NewStartingIndex is 0);
                    if (e.NewStartingIndex is 0)
                        MoveTaskToFront(item);
                }

                break;
            case NotifyCollectionChangedAction.Move when e.NewItems is { } movedItems:
                if (e.NewStartingIndex is 0)
                    foreach (IDownloadTaskGroupBase item in movedItems)
                        MoveTaskToFront(item);
                break;
        }
    }

    private void ClearTaskEntries()
    {
        foreach (var item in _lookup.Values)
            item.Dispose();

        OrdinaryItems.Clear();
        foreach (var folder in SubscriptionFolders)
        {
            foreach (var item in folder.Items.ToArray())
                _ = folder.Remove(item);
        }

        _lookup.Clear();
    }

    private void DisposeEntries()
    {
        ClearTaskEntries();
        foreach (var folder in SubscriptionFolders)
            folder.Dispose();
        SubscriptionFolders.Clear();
        _subscriptionFolderLookup.Clear();
    }

    private async Task LoadSubscriptionFoldersAsync(CancellationToken token)
    {
        try
        {
            await foreach (var subscription in _workSubscriptionPersistentManager.StreamEntriesAsync(token: token))
            {
                if (_isDisposed)
                    return;

                await AddSubscriptionFolderAsync(subscription, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

    private async Task AddSubscriptionFolderAsync(WorkSubscriptionEntry subscription, CancellationToken token)
    {
        if (!_createdOnUiThread || Dispatcher.UIThread.CheckAccess())
        {
            if (_workSubscriptionPersistentManager.GetByKey(subscription.HistoryEntryId) is not null)
                _ = AddSubscriptionFolder(subscription);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            token.ThrowIfCancellationRequested();
            if (!_isDisposed
                && _workSubscriptionPersistentManager.GetByKey(subscription.HistoryEntryId) is not null)
                _ = AddSubscriptionFolder(subscription);
        });
    }
}
