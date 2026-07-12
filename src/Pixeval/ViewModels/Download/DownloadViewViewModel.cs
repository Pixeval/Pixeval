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
using Pixeval.Models.Options;

namespace Pixeval.ViewModels;

public sealed partial class DownloadViewViewModel : ViewModelBase, IDisposable
{
    private readonly ObservableCollection<IDownloadTaskGroupBase> _source;

    private readonly ObservableCollection<IDownloadListEntryViewModel> _viewSource = [];

    private readonly Dictionary<string, DownloadItemViewModel> _lookup = [];

    private readonly List<IDownloadListEntryViewModel> _filteredTasks = [];

    private readonly Dictionary<int, DownloadFolderViewModel> _subscriptionFolders = [];

    private bool _isDisposed;

    [ObservableProperty] public partial DownloadListOption CurrentOption { get; set; } = DownloadListOption.AllQueued;

    [ObservableProperty] public partial string? FilterText { get; set; }

    public AdvancedObservableCollection<IDownloadListEntryViewModel> View { get; }

    partial void OnCurrentOptionChanged(DownloadListOption value) => ResetFilter(GetCustomSearchResult());

    partial void OnFilterTextChanged(string? value) => UpdateFilteredTasks(value);

    public DownloadViewViewModel(ObservableCollection<IDownloadTaskGroupBase> source)
    {
        _source = source;
        View = new AdvancedObservableCollection<IDownloadListEntryViewModel>(_viewSource, true);
        foreach (var task in _source)
            AddTask(task);

        _source.CollectionChanged += SourceOnCollectionChanged;
        ResetFilter();
    }

    private void ResetFilter(IEnumerable<IDownloadListEntryViewModel>? customSearchResult = null)
    {
        var filterSource = customSearchResult?.ToHashSet();

        using (View.DeferFiltersChange())
        {
            View.Filters.Clear();
            View.Filters.Add(IFilter<IDownloadListEntryViewModel>.Create(vm => vm.MatchesOption(CurrentOption, filterSource), false));
        }
    }

    public bool MatchesFolderItem(DownloadItemViewModel vm) =>
        CurrentOption is DownloadListOption.CustomSearch && !string.IsNullOrWhiteSpace(FilterText)
            ? vm.MatchesSearch(FilterText)
            : vm.MatchesOption(CurrentOption, null);

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _source.CollectionChanged -= SourceOnCollectionChanged;
        DisposeEntries();
        View.Filters.Clear();
        View.Source = [];
        View.Dispose();
    }

    private void AddTask(IDownloadTaskGroupBase task)
    {
        if (_isDisposed || task is not IDownloadTaskGroup group)
            return;

        if (_lookup.ContainsKey(group.Destination))
            return;

        var vm = new DownloadItemViewModel(group);
        _lookup[group.Destination] = vm;
        if (group.DatabaseEntry.WorkSubscriptionId <= 0
            || GetOrCreateFolder(group.DatabaseEntry.WorkSubscriptionId) is not { } folderViewModel)
        {
            _viewSource.Add(vm);
            return;
        }

        folderViewModel.Add(vm);
        return;

        DownloadFolderViewModel? GetOrCreateFolder(int subscriptionEntryId)
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
    }

    private void RemoveTask(IDownloadTaskGroupBase task)
    {
        if (_isDisposed || !_lookup.Remove(task.Destination, out var vm))
            return;

        if (vm.DownloadTask.DatabaseEntry.WorkSubscriptionId > 0
            && _subscriptionFolders.TryGetValue(vm.DownloadTask.DatabaseEntry.WorkSubscriptionId, out var folder))
        {
            _ = folder.Remove(vm);
            vm.Dispose();
            if (!folder.HasItems)
            {
                _ = _subscriptionFolders.Remove(folder.Subscription.HistoryEntryId);
                _ = _viewSource.Remove(folder);
                folder.Dispose();
            }

            return;
        }

        _ = _viewSource.Remove(vm);
        vm.Dispose();
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDisposed)
            return;

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
                DisposeEntries();
                foreach (var task in _source)
                    AddTask(task);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                break;
        }

        UpdateFilteredTasks(FilterText);
    }

    private IReadOnlyCollection<IDownloadListEntryViewModel>? GetCustomSearchResult() =>
        CurrentOption is DownloadListOption.CustomSearch && !string.IsNullOrWhiteSpace(FilterText)
            ? _filteredTasks
            : null;

    private void UpdateFilteredTasks(string? key)
    {
        _filteredTasks.Clear();
        if (!string.IsNullOrWhiteSpace(key))
            foreach (var vm in _viewSource.Where(t => t.MatchesSearch(key)))
                _filteredTasks.Add(vm);

        ResetFilter(GetCustomSearchResult());
    }

    private void DisposeEntries()
    {
        foreach (var folder in _subscriptionFolders.Values)
            folder.Dispose();
        foreach (var item in _lookup.Values)
            item.Dispose();

        _filteredTasks.Clear();
        _viewSource.Clear();
        _subscriptionFolders.Clear();
        _lookup.Clear();
    }
}
