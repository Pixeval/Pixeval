#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadListPageViewModel.cs
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
using Pixeval.Controls;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Options;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.Download;

public partial class DownloadListPageViewModel : IllustrateViewViewModel<Illustration, DownloadListEntryViewModel>
{
    public static readonly IEnumerable<DownloadListOption> AvailableDownloadListOptions = Enum.GetValues<DownloadListOption>();

    [ObservableProperty]
    private DownloadListOption _currentOption;

    [ObservableProperty]
    private ObservableCollection<DownloadListEntryViewModel> _filteredTasks = [];

    [ObservableProperty]
    private bool _isAnyEntrySelected;

    [ObservableProperty]
    private string _selectionLabel;

    public override DataProvider<Illustration, DownloadListEntryViewModel> DataProvider { get; } = new DownloadListEntryDataProvider();

    public void ResetEngineAndFillAsync(IEnumerable<ObservableDownloadTask> source) => DataProvider.To<DownloadListEntryDataProvider>().ResetAndFillAsync(source);

    public DownloadListPageViewModel(IEnumerable<ObservableDownloadTask> source)
    {
        ResetEngineAndFillAsync(source);
        _selectionLabel = DownloadListPageResources.CancelSelectionButtonDefaultLabel;
    }

    public IEnumerable<DownloadListEntryViewModel> SelectedTasks => DataProvider.View.Where(x => x.DownloadTask.Selected);

    public void UpdateSelection()
    {
        var count = SelectedTasks.Count();
        SelectionLabel = count is 0
            ? DownloadListPageResources.CancelSelectionButtonDefaultLabel
            : DownloadListPageResources.CancelSelectionButtonFormatted.Format(count);
        IsAnyEntrySelected = count != 0;
    }

    public void PauseSelectedItems()
    {
        foreach (var downloadListEntryViewModel in SelectedTasks.Where(t => t.DownloadTask.CurrentState == DownloadState.Running))
            downloadListEntryViewModel.DownloadTask.CancellationHandle.Pause();
    }

    public void ResumeSelectedItems()
    {
        foreach (var downloadListEntryViewModel in SelectedTasks.Where(t => t.DownloadTask.CurrentState == DownloadState.Paused))
            downloadListEntryViewModel.DownloadTask.CancellationHandle.Resume();
    }

    public void CancelSelectedItems()
    {
        foreach (var downloadListEntryViewModel in SelectedTasks.Where(t => t.DownloadTask.CurrentState is DownloadState.Queued or DownloadState.Created or DownloadState.Running or DownloadState.Paused))
            downloadListEntryViewModel.DownloadTask.CancellationHandle.Cancel();
    }

    public void RemoveSelectedItems()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        SelectedTasks.ToList().ForEach(task =>
        {
            App.AppViewModel.DownloadManager.RemoveTask(task.DownloadTask);
            _ = DataProvider.View.Remove(task);
            _ = manager.Delete(m => m.Destination == task.DownloadTask.Destination);
        });

        UpdateSelection();
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

        bool Query(DownloadListEntryViewModel viewModel) =>
            (viewModel.Illustrate.Title?.Contains(key) ?? false) ||
                   ((viewModel.DownloadTask is IllustrationDownloadTask task ? task.IllustrationViewModel.Id : viewModel.DownloadTask.Id)?.Contains(key) ?? false);
    }

    public void ResetFilter(IEnumerable<DownloadListEntryViewModel>? customSearchResultTask = null)
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
                _ => throw new ArgumentOutOfRangeException()
            },
            _ => false
        };
        foreach (var downloadListEntryViewModel in DataProvider.Source)
            if (!DataProvider.View.Any(downloadListEntryViewModel.DownloadTask.Equals))
                downloadListEntryViewModel.DownloadTask.Selected = false;

        UpdateSelection();
    }

    public override void Dispose()
    {
        foreach (var illustrationViewModel in DataProvider.Source)
        {
            illustrationViewModel.UnloadThumbnail(this, ThumbnailUrlOption.SquareMedium);
            illustrationViewModel.Dispose();
        }
    }
}
