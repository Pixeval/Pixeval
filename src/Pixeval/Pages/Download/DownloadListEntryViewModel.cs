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

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls.IllustrationView;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.Download;

public sealed partial class DownloadListEntryViewModel(ObservableDownloadTask downloadTask, Illustration illustration)
    : IllustrationItemViewModel(illustration)
{
    [ObservableProperty]
    private ObservableDownloadTask _downloadTask = downloadTask;

    public static string GetEntryProgressMessage(DownloadState currentState, double progress, Exception? errorCause)
    {
        return currentState switch
        {
            DownloadState.Created => "",
            DownloadState.Running => DownloadListEntryResources.DownloadRunningFormatted.Format((int)progress),
            DownloadState.Queued => DownloadListEntryResources.DownloadQueued,
            DownloadState.Error => DownloadListEntryResources.DownloadErrorMessageFormatted.Format(errorCause!.Message),
            DownloadState.Completed => DownloadListEntryResources.DownloadCompleted,
            DownloadState.Cancelled => DownloadListEntryResources.DownloadCancelled,
            DownloadState.Paused => DownloadListEntryResources.DownloadPaused,
            _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(currentState)
        };
    }

    public static string GetEntryActionButtonContent(DownloadState currentState)
    {
        return currentState switch
        {
            DownloadState.Created or DownloadState.Queued => DownloadListEntryResources.DownloadCancelledAction,
            DownloadState.Running => DownloadListEntryResources.ActionButtonContentPause,
            DownloadState.Cancelled or DownloadState.Error => DownloadListEntryResources.ActionButtonContentRetry,
            DownloadState.Completed => DownloadListEntryResources.ActionButtonContentOpen,
            DownloadState.Paused => DownloadListEntryResources.ActionButtonContentResume,
            _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(currentState)
        };
    }

    public static bool GetIsRedownloadItemEnabled(DownloadState currentState)
    {
        return currentState is DownloadState.Completed or DownloadState.Error;
    }

    public static bool GetIsEntryCancelDownloadItemEnabled(DownloadState currentState)
    {
        return currentState is DownloadState.Running or DownloadState.Created or DownloadState.Queued;
    }

    public static bool GetIsShowErrorDetailDialogItemEnabled(DownloadState currentState)
    {
        return currentState is DownloadState.Error;
    }
}
