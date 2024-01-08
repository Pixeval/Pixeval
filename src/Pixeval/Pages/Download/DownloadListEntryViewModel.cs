#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadListEntryViewModel.cs
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

using Pixeval.Controls.IllustrationView;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.Download;

/// <inheritdoc/>
public sealed class DownloadListEntryViewModel : IllustrationItemViewModel
{
    public IllustrationDownloadTask DownloadTask { get; }

    public DownloadListEntryViewModel(IllustrationDownloadTask downloadTask) : base(downloadTask.IllustrationViewModel.Illustrate)
    {
        DownloadTask = downloadTask;
        DownloadTask.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(IllustrationDownloadTask.CurrentState):
                    OnPropertyChanged(nameof(ProgressMessage));
                    OnPropertyChanged(nameof(ActionButtonContent));
                    OnPropertyChanged(nameof(IsRedownloadItemEnabled));
                    OnPropertyChanged(nameof(IsCancelItemEnabled));
                    OnPropertyChanged(nameof(IsError));
                    OnPropertyChanged(nameof(IsPaused));
                    break;
                case nameof(IllustrationDownloadTask.ProgressPercentage):
                    OnPropertyChanged(nameof(ProgressMessage));
                    break;
            }
        };
    }

    public string ProgressMessage => DownloadTask.CurrentState switch
    {
        DownloadState.Queued => DownloadListEntryResources.DownloadQueued,
        DownloadState.Running => DownloadListEntryResources.DownloadRunningFormatted.Format((int)DownloadTask.ProgressPercentage),
        DownloadState.Error => DownloadListEntryResources.DownloadErrorMessageFormatted.Format(DownloadTask.ErrorCause?.Message),
        DownloadState.Completed => DownloadListEntryResources.DownloadCompleted,
        DownloadState.Cancelled => DownloadListEntryResources.DownloadCancelled,
        DownloadState.Paused => DownloadListEntryResources.DownloadPaused,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(DownloadTask.CurrentState)
    };

    public string ActionButtonContent => DownloadTask.CurrentState switch
    {
        DownloadState.Queued => DownloadListEntryResources.DownloadCancelledAction,
        DownloadState.Running => DownloadListEntryResources.ActionButtonContentPause,
        DownloadState.Cancelled or DownloadState.Error => DownloadListEntryResources.ActionButtonContentRetry,
        DownloadState.Completed => DownloadListEntryResources.ActionButtonContentOpen,
        DownloadState.Paused => DownloadListEntryResources.ActionButtonContentResume,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(DownloadTask.CurrentState)
    };

    public bool IsRedownloadItemEnabled => DownloadTask.CurrentState is DownloadState.Completed or DownloadState.Error;

    public bool IsCancelItemEnabled => DownloadTask.CurrentState is DownloadState.Running or DownloadState.Queued;

    public bool IsError => DownloadTask.CurrentState is DownloadState.Error;

    public bool IsPaused => DownloadTask.CurrentState is DownloadState.Paused;
}
