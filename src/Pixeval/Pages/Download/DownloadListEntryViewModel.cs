#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/DownloadListEntryViewModel.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net;
using Pixeval.Download;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Pages.Download;

public partial class DownloadListEntryViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private ObservableDownloadTask _downloadTask;

    [ObservableProperty]
    private BitmapImage? _thumbnail;

    public DownloadListEntryViewModel(ObservableDownloadTask downloadTask)
    {
        _downloadTask = downloadTask;
        LoadThumbnail();
    }

    public void Dispose()
    {
        _thumbnail = null;
    }

    public async void LoadThumbnail()
    {
        if (DownloadTask.Thumbnail is { } url)
        {
            var stream = (await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url))
                .GetOrElse(null);
            if (stream is not null)
            {
                Thumbnail = await stream.GetBitmapImageAsync(true, 50);
            }
        }
    }

    public static string GetEntryProgressMessage(DownloadState currentState, double progress, Exception? errorCause)
    {
        return currentState switch
        {
            DownloadState.Created => string.Empty,
            DownloadState.Running => DownloadListEntryResources.DownloadRunningFormatted.Format((int)progress),
            DownloadState.Queued => DownloadListEntryResources.DownloadQueued,
            DownloadState.Error => DownloadListEntryResources.DownloadErrorMessageFormatted.Format(errorCause!.Message),
            DownloadState.Completed => DownloadListEntryResources.DownloadCompleted,
            DownloadState.Cancelled => DownloadListEntryResources.DownloadCancelled,
            DownloadState.Paused => DownloadListEntryResources.DownloadPaused,
            _ => throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null)
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
            _ => throw new ArgumentOutOfRangeException()
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
        return currentState == DownloadState.Error;
    }

    public static Brush GetActionButtonBackground(DownloadState currentState)
    {
        return GetSelectedBackground(currentState is DownloadState.Running or DownloadState.Paused);
    }

    public static Brush GetSelectedBackground(bool selected)
    {
        return selected
            ? (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"]
            : (Brush)Application.Current.Resources["CardBackground"];
    }
}