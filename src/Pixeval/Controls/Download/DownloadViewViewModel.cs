#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Utilities;
using WinUI3Utilities;
using DownloadItemDataProvider = Pixeval.Controls.SimpleViewDataProvider<Pixeval.Download.Models.IDownloadTaskGroup, Pixeval.Controls.DownloadItemViewModel>;

namespace Pixeval.Controls;

public partial class DownloadViewViewModel : ObservableObject, IDisposable
{
    public static readonly IEnumerable<DownloadListOption> AvailableDownloadListOptions = Enum.GetValues<DownloadListOption>();

    [ObservableProperty]
    private DownloadListOption _currentOption;

    [ObservableProperty]
    private ObservableCollection<DownloadItemViewModel> _filteredTasks = [];

    [ObservableProperty]
    private bool _isAnyEntrySelected;

    [ObservableProperty]
    private string _selectionLabel;

    private DownloadItemViewModel[] _selectedEntries = [];

    public DownloadViewViewModel(IEnumerable<IDownloadTaskGroup> source)
    {
        DataProvider.ResetEngine(App.AppViewModel.MakoClient.Computed(source.ToAsyncEnumerable()));
        _selectionLabel = DownloadPageResources.CancelSelectionButtonDefaultLabel;
        DataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public DownloadItemDataProvider DataProvider { get; } = new DownloadItemDataProvider();

    public bool HasNoItem => DataProvider.View.Count is 0;

    public DownloadItemViewModel[] SelectedEntries
    {
        get => _selectedEntries;
        set
        {
            if (Equals(value, _selectedEntries))
                return;
            _selectedEntries = value;
            var count = value.Length;
            IsAnyEntrySelected = count > 0;
            SelectionLabel = IsAnyEntrySelected
                ? DownloadPageResources.CancelSelectionButtonFormatted.Format(count)
                : DownloadPageResources.CancelSelectionButtonDefaultLabel;
            OnPropertyChanged();
        }
    }

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
            _ = DataProvider.View.Remove(task);
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

        var newTasks = DataProvider.Source.Where(Query);
        FilteredTasks.ReplaceByUpdate(newTasks);
        return;

        bool Query(DownloadItemViewModel viewModel) =>
            viewModel.Title.Contains(key) ||
            (viewModel.DownloadTask is { } task ? task.Id : viewModel.DownloadTask.Id).ToString()
            .Contains(key);
    }

    public void ResetFilter(IEnumerable<DownloadItemViewModel>? customSearchResultTask = null)
    {
        DataProvider.View.Filter = o => o switch
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
        DataProvider.Dispose();
    }
}
