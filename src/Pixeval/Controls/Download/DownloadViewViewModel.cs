// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Collections;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class DownloadViewViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    public partial DownloadListOption CurrentOption { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<DownloadItemViewModel> FilteredTasks { get; set; } = [];

    [ObservableProperty]
    public partial bool IsAnyEntrySelected { get; set; }

    [ObservableProperty]
    public partial string SelectionLabel { get; set; }

    public DownloadViewViewModel(ObservableCollection<IDownloadTaskGroup> source)
    {
        View = new(new ObservableCollectionAdapter<IDownloadTaskGroup, DownloadItemViewModel>(source), true);
        SelectionLabel = DownloadPageResources.CancelSelectionButtonDefaultLabel;
        View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public AdvancedObservableCollection<DownloadItemViewModel> View { get; }

    public bool HasNoItem => View.Count is 0;

    public DownloadItemViewModel[] SelectedEntries
    {
        get;
        set
        {
            if (Equals(value, field))
                return;
            field = value;
            var count = value.Length;
            IsAnyEntrySelected = count > 0;
            SelectionLabel = IsAnyEntrySelected
                ? DownloadPageResources.CancelSelectionButtonFormatted.Format(count)
                : DownloadPageResources.CancelSelectionButtonDefaultLabel;
            OnPropertyChanged();
        }
    } = [];

    public void PauseSelectedItems()
    {
        foreach (var downloadListEntryViewModel in SelectedEntries)
            downloadListEntryViewModel.DownloadTask.Pause();
    }

    public void ResumeSelectedItems()
    {
        foreach (var downloadListEntryViewModel in SelectedEntries)
            downloadListEntryViewModel.DownloadTask.TryResume();
    }

    public void CancelSelectedItems()
    {
        foreach (var downloadListEntryViewModel in SelectedEntries)
            downloadListEntryViewModel.DownloadTask.Cancel();
    }

    public void RemoveSelectedItems()
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        SelectedEntries.ForEach(task =>
        {
            App.AppViewModel.DownloadManager.RemoveTask(task.DownloadTask);
            _ = manager.Delete(m => m.Destination == task.DownloadTask.Destination);
        });
    }

    public void FilterTask(string key)
    {
        if (key.IsNullOrBlank())
        {
            FilteredTasks.Clear();
            return;
        }

        var newTasks = View.Source.Where(Query);
        FilteredTasks.ReplaceByUpdate(newTasks);
        return;

        bool Query(DownloadItemViewModel viewModel) =>
            viewModel.Entry.Title.Contains(key) ||
            (viewModel.DownloadTask is { } task ? task.Id : viewModel.DownloadTask.Id).ToString()
            .Contains(key);
    }

    public void ResetFilter(IEnumerable<DownloadItemViewModel>? customSearchResultTask = null)
    {
        View.Filter = o => o switch
        {
            { DownloadTask: var task } => CurrentOption switch
            {
                DownloadListOption.AllQueued => true,
                DownloadListOption.Running => task.CurrentState is DownloadState.Running,
                DownloadListOption.Completed => task.CurrentState is DownloadState.Completed,
                DownloadListOption.Cancelled => task.CurrentState is DownloadState.Cancelled,
                DownloadListOption.Error => task.CurrentState is DownloadState.Error,
                DownloadListOption.CustomSearch => customSearchResultTask?.Contains(o) ?? true,
                _ => ThrowHelper.ArgumentOutOfRange<DownloadListOption, bool>(CurrentOption)
            },
            _ => false
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var entry in View.Source)
        {
            entry.UnloadThumbnail(this);
            entry.Dispose();
        }
    }
}
