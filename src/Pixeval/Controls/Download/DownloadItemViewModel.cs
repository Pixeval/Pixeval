#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadItemViewModel.cs
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
using System.IO;
using System.Threading.Tasks;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Input;
using Pixeval.Database;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

/// <inheritdoc/>
public sealed partial class DownloadItemViewModel : WorkEntryViewModel<IWorkEntry>
{
    public DownloadTaskBase DownloadTask { get; }

    public DownloadItemViewModel(DownloadTaskBase downloadTask) : base(downloadTask.ViewModel.Entry)
    {
        DownloadTask = downloadTask;
        DownloadTask.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(IllustrationDownloadTask.CurrentState):
                    OnPropertyChanged(nameof(ProgressMessage));
                    OnPropertyChanged(nameof(ActionButtonContent));
                    OnPropertyChanged(nameof(ActionButtonSymbol));
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
        DownloadState.Queued => DownloadItemResources.DownloadQueued,
        DownloadState.Running => DownloadItemResources.DownloadRunningFormatted.Format((int)DownloadTask.ProgressPercentage),
        DownloadState.Error => DownloadItemResources.DownloadErrorMessageFormatted.Format(DownloadTask.ErrorCause?.Message),
        DownloadState.Completed => DownloadItemResources.DownloadCompleted,
        DownloadState.Cancelled => DownloadItemResources.DownloadCancelled,
        DownloadState.Paused => DownloadItemResources.DownloadPaused,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(DownloadTask.CurrentState)
    };

    public string ActionButtonContent => DownloadTask.CurrentState switch
    {
        DownloadState.Queued => DownloadItemResources.DownloadCancelledAction,
        DownloadState.Running => DownloadItemResources.ActionButtonContentPause,
        DownloadState.Cancelled or DownloadState.Error => DownloadItemResources.ActionButtonContentRetry,
        DownloadState.Completed => DownloadItemResources.ActionButtonContentOpen,
        DownloadState.Paused => DownloadItemResources.ActionButtonContentResume,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(DownloadTask.CurrentState)
    };

    public Symbol ActionButtonSymbol => DownloadTask.CurrentState switch
    {
        DownloadState.Queued => Symbol.Dismiss,
        DownloadState.Running => Symbol.Pause,
        DownloadState.Cancelled or DownloadState.Error => Symbol.ArrowRepeatAll,
        DownloadState.Completed => Symbol.Open,
        DownloadState.Paused => Symbol.Play,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, Symbol>(DownloadTask.CurrentState)
    };

    public override async ValueTask<bool> TryLoadThumbnailAsync(IDisposable key)
    {
        if (ThumbnailSourceRef is not null)
        {
            _ = ThumbnailSourceRef.MakeShared(key);

            // 之前已加载
            return false;
        }

        if (DownloadTask.CurrentState is DownloadState.Completed || DownloadTask.Type is DownloadItemType.Manga or DownloadItemType.Novel)
        {
            var path = DownloadTask.Destination.Format(0);
            if (File.Exists(path) && !LoadingThumbnail)
            {
                LoadingThumbnail = true;
                var s = await IoHelper.GetFileThumbnailAsync(path);
                ThumbnailStream = s;
                ThumbnailSourceRef = new SharedRef<SoftwareBitmapSource>(await s.GetSoftwareBitmapSourceAsync(false), key);
                LoadingThumbnail = false;
                return true;
            }
        }

        return await base.TryLoadThumbnailAsync(key);
    }
    public bool IsRedownloadItemEnabled => DownloadTask.CurrentState is DownloadState.Completed or DownloadState.Error;

    public bool IsCancelItemEnabled => DownloadTask.CurrentState is DownloadState.Running or DownloadState.Queued;

    public bool IsError => DownloadTask.CurrentState is DownloadState.Error;

    public bool IsPaused => DownloadTask.CurrentState is DownloadState.Paused;

    public override Uri AppUri => ThrowHelper.NotSupported<Uri>();

    public override Uri WebUri => ThrowHelper.NotSupported<Uri>();

    public override Uri PixEzUri => ThrowHelper.NotSupported<Uri>();

    protected override Task<bool> SetBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null) => ThrowHelper.NotSupported<Task<bool>>();

    protected override void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) => ThrowHelper.NotSupported();

    protected override void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) => ThrowHelper.NotSupported();

    protected override void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) => ThrowHelper.NotSupported();
}
