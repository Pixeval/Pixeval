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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Database;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;
using Pixeval.Util;

namespace Pixeval.Controls;

public sealed partial class DownloadItemViewModel(IDownloadTaskGroup downloadTask)
    : ThumbnailEntryViewModel<IWorkEntry>(downloadTask.DatabaseEntry.Entry), IFactory<IDownloadTaskGroup, DownloadItemViewModel>
{
    public IDownloadTaskGroup DownloadTask { get; } = downloadTask;

    public override async ValueTask<bool> TryLoadThumbnailAsync(IDisposable key)
    {
        if (ThumbnailSourceRef is not null)
        {
            _ = ThumbnailSourceRef.MakeShared(key);

            // 之前已加载
            return false;
        }

        if (DownloadTask.CurrentState is DownloadState.Completed || DownloadTask.DatabaseEntry.Type is DownloadItemType.Manga or DownloadItemType.Novel)
        {
            var path = null as string;
            if (File.Exists(DownloadTask.OpenLocalDestination))
                path = DownloadTask.OpenLocalDestination;
            var destination = DownloadTask.FirstOrDefault()?.Destination;
            if (File.Exists(destination))
                path = destination;
            if (path is not null && !LoadingThumbnail)
            {
                LoadingThumbnail = true;
                var s = await IoHelper.GetFileThumbnailAsync(path);
                ThumbnailSourceRef = new SharedRef<SoftwareBitmapSource>(await s.GetSoftwareBitmapSourceAsync(true), key);
                LoadingThumbnail = false;
                return true;
            }
        }

        return await base.TryLoadThumbnailAsync(key);
    }

    public static DownloadItemViewModel CreateInstance(IDownloadTaskGroup entry, int index) => new(entry);

#pragma warning disable CA1822

    public string ProgressMessage(DownloadState state, double progressPercentage, IDownloadTaskGroup downloadTask) => state switch
    {
        DownloadState.Queued => DownloadItemResources.DownloadQueued,
        DownloadState.Running => DownloadItemResources.DownloadRunningFormatted.Format((int)progressPercentage),
        DownloadState.Error => DownloadItemResources.DownloadErrorMessageFormatted.Format(downloadTask.ErrorCause?.Message),
        DownloadState.Completed => DownloadItemResources.DownloadCompleted,
        DownloadState.Cancelled => DownloadItemResources.DownloadCancelled,
        DownloadState.Pending => DownloadItemResources.DownloadPending,
        DownloadState.Paused => DownloadItemResources.DownloadPaused,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(state)
    };

    public string ActionButtonContent(DownloadState state) => ActionButtonSymbol(state) switch
    {
        Symbol.Dismiss => DownloadItemResources.ActionDownloadCancelled,
        Symbol.Pause => DownloadItemResources.ActionButtonContentPause,
        Symbol.ArrowRepeatAll => DownloadItemResources.ActionButtonContentRetry,
        Symbol.Open => DownloadItemResources.ActionButtonContentOpen,
        Symbol.Play => DownloadItemResources.ActionButtonContentResume,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, string>(state)
    };

    public Symbol ActionButtonSymbol(DownloadState state) => state switch
    {
        DownloadState.Pending => Symbol.Dismiss,
        DownloadState.Queued or DownloadState.Running => Symbol.Pause,
        DownloadState.Cancelled or DownloadState.Error => Symbol.ArrowRepeatAll,
        DownloadState.Completed => Symbol.Open,
        DownloadState.Paused => Symbol.Play,
        _ => ThrowHelper.ArgumentOutOfRange<DownloadState, Symbol>(state)
    };

    public bool IsItemEnabled(bool isProcessing, DownloadState state) => !isProcessing || state is DownloadState.Completed;

    public bool IsRedownloadItemEnabled(bool isProcessing, DownloadState state) => !isProcessing && state is DownloadState.Completed or DownloadState.Error;

    public bool IsCancelItemEnabled(bool isProcessing, DownloadState state) => !isProcessing && state is DownloadState.Running or DownloadState.Queued or DownloadState.Paused;

    public bool IsError(DownloadState state) => state is DownloadState.Error;

    public bool IsPending(DownloadState state) => state is DownloadState.Pending;

    public bool IsPaused(DownloadState state) => state is DownloadState.Paused;

    public Visibility IsGroup(IDownloadTaskGroup group) => C.ToVisibility(group is DownloadTaskGroup);

    public Brush CurrentStateBrush(DownloadState state) => Application.Current.GetResource<Brush>(state switch
    {
        DownloadState.Paused => "SystemFillColorCautionBrush",
        DownloadState.Cancelled => "SystemFillColorNeutralBrush",
        _ => "SystemFillColorAttentionBrush"
    });

#pragma warning restore CA1822

    #region Not supported

    public override Uri AppUri => ThrowHelper.NotSupported<Uri>();

    public override Uri WebUri => ThrowHelper.NotSupported<Uri>();

    public override Uri PixEzUri => ThrowHelper.NotSupported<Uri>();

    protected override string ThumbnailUrl => Entry.GetThumbnailUrl();

    #endregion
}
