// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Collections;
using Pixeval.Download;
using Pixeval.Models.Download.Tasks;

namespace Pixeval.ViewModels;

public partial class DownloadViewViewModel : ObservableObject, IDisposable
{
    private readonly ObservableCollection<IDownloadTaskGroupBase> _source;

    private readonly ObservableCollection<DownloadItemViewModel> _viewSource = [];

    private readonly Dictionary<string, DownloadItemViewModel> _lookup = [];

    [ObservableProperty]
    public partial DownloadListOption CurrentOption { get; set; } = DownloadListOption.AllQueued;

    [ObservableProperty]
    public partial ObservableCollection<DownloadItemViewModel> FilteredTasks { get; private set; } = [];

    public AdvancedObservableCollection<DownloadItemViewModel> View { get; }

    public ObservableCollection<DownloadItemViewModel> SelectedEntries { get; } = [];

    partial void OnCurrentOptionChanged(DownloadListOption value) => ResetFilter();

    public DownloadViewViewModel(ObservableCollection<IDownloadTaskGroupBase> source)
    {
        _source = source;
        View = new AdvancedObservableCollection<DownloadItemViewModel>(_viewSource, true);
        foreach (var task in _source)
            AddTask(task);

        _source.CollectionChanged += SourceOnCollectionChanged;
        ResetFilter();
    }

    public void PauseSelectedItems()
    {
        foreach (var item in SelectedEntries)
            item.DownloadTask.Pause();
    }

    public void ResumeSelectedItems()
    {
        foreach (var item in SelectedEntries)
            item.DownloadTask.TryResume();
    }

    public void CancelSelectedItems()
    {
        foreach (var item in SelectedEntries)
            item.DownloadTask.Cancel();
    }

    public void RemoveSelectedItems(bool deleteLocalFiles)
    {
        foreach (var item in SelectedEntries.ToArray())
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

        bool Query(DownloadItemViewModel vm) =>
            vm.Entry.Title.Contains(key, StringComparison.OrdinalIgnoreCase)
            || vm.DownloadTask.Id.Contains(key, StringComparison.OrdinalIgnoreCase);
    }

    public void ResetFilter(IEnumerable<DownloadItemViewModel>? customSearchResult = null)
    {
        var hash = customSearchResult?.ToHashSet();

        View.Filter = vm => CurrentOption switch
        {
            DownloadListOption.AllQueued => true,
            DownloadListOption.Running => vm.CurrentState is DownloadState.Running,
            DownloadListOption.Completed => vm.CurrentState is DownloadState.Completed,
            DownloadListOption.Cancelled => vm.CurrentState is DownloadState.Cancelled,
            DownloadListOption.Error => vm.CurrentState is DownloadState.Error,
            DownloadListOption.CustomSearch => hash?.Contains(vm) ?? true,
            _ => true
        };
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
        _viewSource.Add(vm);
    }

    private void RemoveTask(IDownloadTaskGroupBase task)
    {
        if (_lookup.Remove(task.Destination, out var vm))
            _ = _viewSource.Remove(vm);
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
