// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Linq;
using FluentIcons.Common;
using Misaki;
using Pixeval.Download;
using Pixeval.I18N;
using Pixeval.Models.Download.Tasks;

namespace Pixeval.ViewModels;

public sealed class DownloadItemViewModel : ThumbnailEntryViewModel<IArtworkInfo>
{
    public IDownloadTaskGroup DownloadTask { get; }

    public DownloadItemViewModel(IDownloadTaskGroup downloadTask)
        : base(downloadTask.DatabaseEntry.Entry)
    {
        DownloadTask = downloadTask;
        DownloadTask.PropertyChanged += DownloadTaskOnPropertyChanged;
    }

    public string AuthorsText => string.Join(", ", Entry.Authors.Select(t => t.Name));

    public override Uri AppUri => Entry.AppUri;

    public override Uri WebsiteUri => Entry.WebsiteUri;

    public bool ShowGroupStats => DownloadTask.Count > 1;

    public DownloadState CurrentState => DownloadTask.CurrentState;

    public bool IsPending => CurrentState is DownloadState.Pending;

    public bool IsError => CurrentState is DownloadState.Error;

    public bool IsPaused => CurrentState is DownloadState.Paused;

    public string ProgressMessage => CurrentState switch
    {
        DownloadState.Queued => I18NManager.GetResource(DownloadItemResources.DownloadQueued),
        DownloadState.Running => I18NManager.GetResource(DownloadItemResources.DownloadRunningFormatted, (int) DownloadTask.ProgressPercentage),
        DownloadState.Error => I18NManager.GetResource(DownloadItemResources.DownloadErrorMessageFormatted, DownloadTask.ErrorCause?.Message),
        DownloadState.Completed => I18NManager.GetResource(DownloadItemResources.DownloadCompleted),
        DownloadState.Cancelled => I18NManager.GetResource(DownloadItemResources.DownloadCancelled),
        DownloadState.Pending => I18NManager.GetResource(DownloadItemResources.DownloadPending),
        DownloadState.Paused => I18NManager.GetResource(DownloadItemResources.DownloadPaused),
        _ => string.Empty
    };

    public Symbol ActionButtonSymbol => CurrentState switch
    {
        DownloadState.Pending => Symbol.Dismiss,
        DownloadState.Queued or DownloadState.Running => Symbol.Pause,
        DownloadState.Cancelled or DownloadState.Error => Symbol.ArrowRepeatAll,
        DownloadState.Completed => Symbol.Open,
        DownloadState.Paused => Symbol.Play,
        _ => Symbol.Question
    };

    public string ActionButtonContent => ActionButtonSymbol switch
    {
        Symbol.Dismiss => I18NManager.GetResource(DownloadItemResources.ActionDownloadCancelled),
        Symbol.Pause => I18NManager.GetResource(DownloadItemResources.ActionButtonContentPause),
        Symbol.ArrowRepeatAll => I18NManager.GetResource(DownloadItemResources.ActionButtonContentRetry),
        Symbol.Open => I18NManager.GetResource(DownloadItemResources.ActionButtonContentOpen),
        Symbol.Play => I18NManager.GetResource(DownloadItemResources.ActionButtonContentResume),
        _ => string.Empty
    };

    public bool IsItemEnabled => !DownloadTask.IsProcessing || CurrentState is DownloadState.Completed;

    public bool IsRedownloadItemEnabled => !DownloadTask.IsProcessing && CurrentState is DownloadState.Completed or DownloadState.Error;

    public bool IsCancelItemEnabled => !DownloadTask.IsProcessing && CurrentState is DownloadState.Running or DownloadState.Queued or DownloadState.Paused;

    public string StateBrushKey => CurrentState switch
    {
        DownloadState.Paused => "SystemFillColorCautionBrush",
        DownloadState.Cancelled => "SystemFillColorNeutralBrush",
        _ => "SystemFillColorAttentionBrush",
    };

    public void ExecutePrimaryAction()
    {
        switch (ActionButtonSymbol)
        {
            case Symbol.Dismiss:
                DownloadTask.Cancel();
                break;
            case Symbol.Pause:
                DownloadTask.Pause();
                break;
            case Symbol.ArrowRepeatAll:
                DownloadTask.TryReset();
                break;
            case Symbol.Play:
                DownloadTask.TryResume();
                break;
            case Symbol.Open:
                break;
        }
    }

    public override string ThumbnailUrl => Entry.Thumbnails.PickMax()?.ImageUri.OriginalString ?? string.Empty;

    private void DownloadTaskOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState))
        {
            OnPropertyChanged(nameof(CurrentState));
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(IsError));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(ShowGroupStats));
            OnPropertyChanged(nameof(StateBrushKey));
            OnPropertyChanged(nameof(ActionButtonSymbol));
            OnPropertyChanged(nameof(ActionButtonContent));
        }
        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState)
            or nameof(IDownloadTaskBase.IsProcessing))
        {
            OnPropertyChanged(nameof(IsItemEnabled));
            OnPropertyChanged(nameof(IsRedownloadItemEnabled));
            OnPropertyChanged(nameof(IsCancelItemEnabled));
        }
        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState)
            or nameof(IDownloadTaskBase.ProgressPercentage)
            or nameof(IDownloadTaskBase.ErrorCause))
        {
            OnPropertyChanged(nameof(ProgressMessage));
        }
    }
}
