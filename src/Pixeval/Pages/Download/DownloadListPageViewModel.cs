#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DownloadListPageViewModel.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Download;
using Pixeval.Misc;
using Pixeval.Utilities;

namespace Pixeval.Pages.Download
{
    public class DownloadListPageViewModel : ObservableObject
    {
        public static readonly IEnumerable<DownloadListOption> AvailableDownloadListOptions = Enum.GetValues<DownloadListOption>();

        private DownloadListOption _currentOption;

        public DownloadListOption CurrentOption
        {
            get => _currentOption;
            set => SetProperty(ref _currentOption, value);
        }

        private IList<DownloadListEntryViewModel> _downloadTasks;

        public IList<DownloadListEntryViewModel> DownloadTasks
        {
            get => _downloadTasks;
            set => SetProperty(ref _downloadTasks, value);
        }

        private AdvancedCollectionView _downloadTasksView;

        public AdvancedCollectionView DownloadTasksView
        {
            get => _downloadTasksView;
            set => SetProperty(ref _downloadTasksView, value);
        }

        private ObservableCollection<IDownloadTask> _filteredTasks;

        public ObservableCollection<IDownloadTask> FilteredTasks
        {
            get => _filteredTasks;
            set => SetProperty(ref _filteredTasks, value);
        }

        private string _selectionLabel;

        public string SelectionLabel
        {
            get => _selectionLabel;
            set => SetProperty(ref _selectionLabel, value);
        }

        private ObservableCollection<ObservableDownloadTask> _selectedTasks;

        public ObservableCollection<ObservableDownloadTask> SelectedTasks
        {
            get => _selectedTasks;
            set => SetProperty(ref _selectedTasks, value);
        }

        private bool _isAnyEntrySelected;

        public bool IsAnyEntrySelected
        {
            get => _isAnyEntrySelected;
            set => SetProperty(ref _isAnyEntrySelected, value);
        }

        public DownloadListPageViewModel(IList<DownloadListEntryViewModel> downloadTasks)
        {
            _downloadTasks = downloadTasks;
            _filteredTasks = new ObservableCollection<IDownloadTask>();
            _downloadTasksView = new AdvancedCollectionView(downloadTasks as IList);
            _selectedTasks = new ObservableCollection<ObservableDownloadTask>();
            _selectionLabel = DownloadListPageResources.CancelSelectionButtonDefaultLabel;
        }

        public void UpdateSelection()
        {
            var count = SelectedTasks.Count;
            SelectionLabel = count == 0
                ? DownloadListPageResources.CancelSelectionButtonDefaultLabel
                : DownloadListPageResources.CancelSelectionButtonFormatted.Format(count);
            IsAnyEntrySelected = count != 0;
        }

        public void PauseAll()
        {
            foreach (var downloadListEntryViewModel in _selectedTasks.Where(t => t.CurrentState == DownloadState.Running))
            {
                downloadListEntryViewModel.CancellationHandle.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var downloadListEntryViewModel in _selectedTasks.Where(t => t.CurrentState == DownloadState.Paused))
            {
                downloadListEntryViewModel.CancellationHandle.Resume();
            }
        }

        public void CancelAll()
        {
            foreach (var downloadListEntryViewModel in _selectedTasks.Where(t => t.CurrentState is DownloadState.Queued or DownloadState.Created or DownloadState.Running or DownloadState.Paused))
            {
                downloadListEntryViewModel.CancellationHandle.Cancel();
            }
        }

        public void ClearDownloadList()
        {
            App.AppViewModel.DownloadManager.ClearTasks();
            DownloadTasksView.Clear();
            DownloadTasksView.Refresh();
        }

        public void FilterTask(string key)
        {
            FilteredTasks.Clear();
            if (key.IsNullOrBlank())
            {
                return;
            }

            FilteredTasks.AddRange(DownloadTasks.Where(Query).Select(t => t.DownloadTask));

            bool Query(DownloadListEntryViewModel viewModel)
            {
                return (viewModel.DownloadTask.Title?.Contains(key) ?? false)
                       || viewModel.DownloadTask is not IllustrationDownloadTask task
                       || task.IllustrationViewModel.Id.Contains(key);
            }
        }

        public void ResetFilter(IEnumerable<IDownloadTask>? customSearchResultTask = null)
        {
            DownloadTasksView.Filter = o => o switch
            {
                DownloadListEntryViewModel { DownloadTask: var task } => CurrentOption switch
                {
                    DownloadListOption.AllQueued => true,
                    DownloadListOption.Running => task.CurrentState is DownloadState.Running,
                    DownloadListOption.Completed => task.CurrentState is DownloadState.Completed,
                    DownloadListOption.Cancelled => task.CurrentState is DownloadState.Cancelled,
                    DownloadListOption.Error => task.CurrentState is DownloadState.Error,
                    DownloadListOption.CustomSearch => customSearchResultTask?.Contains(task) ?? true,
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => false
            };
            DownloadTasksView.Refresh();
            SelectedTasks.RemoveAll(x =>
            {
                if (!DownloadTasksView.Any(entry => Equals(((DownloadListEntryViewModel) entry).DownloadTask, x)))
                {
                    x.Selected = false;
                    return false;
                }

                return true;
            });
            UpdateSelection();
        }

        public string? SubtitleText(DownloadListOption option)
        {
            return option.GetLocalizedResourceContent();
        }
    }
}