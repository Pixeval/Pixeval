// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Mako.Model;
using Pixeval.Database;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class DownloadItemViewModel(IDownloadTaskGroup downloadTask)
    : ThumbnailEntryViewModel<IWorkEntry>(downloadTask.DatabaseEntry.Entry), IFactory<IDownloadTaskGroup, DownloadItemViewModel>
{
    public IDownloadTaskGroup DownloadTask { get; } = downloadTask;

    public override async ValueTask<bool> TryLoadThumbnailAsync(object key)
    {
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
                ThumbnailSource = await IoHelper.GetFileThumbnailAsync(path);
                LoadingThumbnail = false;
                return true;
            }
        }

        return await base.TryLoadThumbnailAsync(key);
    }

    public static DownloadItemViewModel CreateInstance(IDownloadTaskGroup entry) => new(entry);

#pragma warning disable CA1822

    public string ProgressMessage(DownloadState state, double progressPercentage, IDownloadTaskGroup downloadTask) => state switch
    {
        DownloadState.Queued => DownloadItemResources.DownloadQueued,
        DownloadState.Running => DownloadItemResources.DownloadRunningFormatted.Format((int) progressPercentage),
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

    public override Uri WebsiteUri => ThrowHelper.NotSupported<Uri>();

    public override Uri PixEzUri => ThrowHelper.NotSupported<Uri>();

    protected override string ThumbnailUrl => Entry.GetThumbnailUrl();

    #endregion
}
