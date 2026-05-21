// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Collections;
using Pixeval.Download;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Download.Tasks;

namespace Pixeval.ViewModels;

public partial class DownloadViewViewModel : ViewModelBase, IDisposable
{
    private readonly ObservableCollection<IDownloadTaskGroupBase> _source;

    private readonly ObservableCollection<IDownloadListEntryViewModel> _viewSource = [];

    private readonly Dictionary<string, DownloadItemViewModel> _lookup = [];

    private readonly Dictionary<int, DownloadFolderViewModel> _subscriptionFolders = [];

    [ObservableProperty]
    public partial DownloadListOption CurrentOption { get; set; } = DownloadListOption.AllQueued;

    [ObservableProperty]
    public partial ObservableCollection<IDownloadListEntryViewModel> FilteredTasks { get; private set; } = [];

    public AdvancedObservableCollection<IDownloadListEntryViewModel> View { get; }

    public ObservableCollection<IDownloadListEntryViewModel> SelectedEntries { get; } = [];

    partial void OnCurrentOptionChanged(DownloadListOption value) => ResetFilter();

    public DownloadViewViewModel(ObservableCollection<IDownloadTaskGroupBase> source)
    {
        _source = source;
        View = new AdvancedObservableCollection<IDownloadListEntryViewModel>(_viewSource, true);
        foreach (var task in _source)
            AddTask(task);

        _source.CollectionChanged += SourceOnCollectionChanged;
        ResetFilter();
    }

    public void PauseSelectedItems()
    {
        foreach (var item in SelectedEntries)
            foreach (var downloadItem in item.DownloadItems)
                downloadItem.DownloadTask.Pause();
    }

    public void ResumeSelectedItems()
    {
        foreach (var item in SelectedEntries)
            foreach (var downloadItem in item.DownloadItems)
                downloadItem.DownloadTask.Resume();
    }

    public void CancelSelectedItems()
    {
        foreach (var item in SelectedEntries)
            foreach (var downloadItem in item.DownloadItems)
                downloadItem.DownloadTask.Cancel();
    }

    public void ResetSelectedItems()
    {
        foreach (var item in SelectedEntries)
            foreach (var downloadItem in item.DownloadItems)
                downloadItem.DownloadTask.Reset();
    }

    public void RemoveSelectedItems(bool deleteLocalFiles)
    {
        foreach (var item in SelectedEntries.SelectMany(t => t.DownloadItems).Distinct().ToArray())
        {
            if (deleteLocalFiles)
            {
                try
                {
                    item.DownloadTask.Delete();
                }
                catch
                {
                    // ignored
                }
            }

            App.AppViewModel.HistoryPersistHelper.DownloadManager.RemoveTask(item.DownloadTask);
        }

        SelectedEntries.Clear();
    }

    public void FilterTask(string? key)
    {
        FilteredTasks.Clear();
        if (string.IsNullOrWhiteSpace(key))
            return;

        foreach (var vm in _viewSource.Where(Query))
            FilteredTasks.Add(vm);

        return;

        bool Query(IDownloadListEntryViewModel vm) => vm.MatchesSearch(key);
    }

    public void ResetFilter(IEnumerable<IDownloadListEntryViewModel>? customSearchResult = null)
    {
        var hash = customSearchResult?.ToHashSet();

        using (View.DeferFiltersChange())
        {
            View.Filters.Clear();
            View.Filters.Add(IFilter<IDownloadListEntryViewModel>.Create(vm => vm.MatchesOption(CurrentOption, hash), false));
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _source.CollectionChanged -= SourceOnCollectionChanged;
    }

    private void AddTask(IDownloadTaskGroupBase task)
    {
        if (task is not IDownloadTaskGroup group)
            return;

        if (_lookup.ContainsKey(group.Destination))
            return;

        var vm = new DownloadItemViewModel(group);
        _lookup[group.Destination] = vm;
        if (group.DatabaseEntry.WorkSubscriptionId <= 0
            || GetOrCreateFolder(group.DatabaseEntry.WorkSubscriptionId) is not { } folder)
        {
            _viewSource.Add(vm);
            return;
        }

        folder.Add(vm);
    }

    private void RemoveTask(IDownloadTaskGroupBase task)
    {
        if (_lookup.Remove(task.Destination, out var vm))
        {
            if (vm.DownloadTask.DatabaseEntry.WorkSubscriptionId > 0
                && _subscriptionFolders.TryGetValue(vm.DownloadTask.DatabaseEntry.WorkSubscriptionId, out var folder))
            {
                _ = folder.Remove(vm);
                if (!folder.HasItems)
                {
                    _ = _subscriptionFolders.Remove(folder.Subscription.HistoryEntryId);
                    _ = _viewSource.Remove(folder);
                }
                return;
            }

            _ = _viewSource.Remove(vm);
        }
    }

    private DownloadFolderViewModel? GetOrCreateFolder(int subscriptionEntryId)
    {
        if (_subscriptionFolders.TryGetValue(subscriptionEntryId, out var folder))
            return folder;

        var provider = App.AppViewModel.AppServiceProvider;
        if (provider.GetRequiredService<WorkSubscriptionPersistentManager>().GetByKey(subscriptionEntryId) is not { } subscription)
            return null;

        folder = new(subscription);
        _subscriptionFolders[subscriptionEntryId] = folder;
        _viewSource.Add(folder);
        return folder;
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems is { } newItems:
                foreach (IDownloadTaskGroupBase item in newItems)
                    AddTask(item);
                break;
            case NotifyCollectionChangedAction.Remove when e.OldItems is { } oldItems:
                foreach (IDownloadTaskGroupBase item in oldItems)
                    RemoveTask(item);
                break;
            case NotifyCollectionChangedAction.Reset:
                _viewSource.Clear();
                _lookup.Clear();
                _subscriptionFolders.Clear();
                foreach (var task in _source)
                    AddTask(task);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                break;
        }

        ResetFilter();
        FilterTask(string.Empty);
    }
}
