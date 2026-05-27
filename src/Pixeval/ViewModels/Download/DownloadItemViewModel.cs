// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentIcons.Common;
using Misaki;
using Pixeval.Download;
using Pixeval.I18N;
using Pixeval.Models.Download.Tasks;
using Pixeval.Models.Options;

namespace Pixeval.ViewModels;

public sealed class DownloadItemViewModel : ThumbnailEntryViewModel<IArtworkInfo>, IDownloadListEntryViewModel
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

    public IReadOnlyList<DownloadItemViewModel> DownloadItems => [this];

    public bool IsPending => CurrentState is DownloadState.Pending;

    public bool IsError => CurrentState is DownloadState.Error;

    public bool IsPaused => CurrentState is DownloadState.Paused;

    public string ProgressMessage => CurrentState switch
    {
        DownloadState.Queued => I18NManager.GetResource(DownloadItemResources.DownloadQueued),
        DownloadState.Running => I18NManager.GetResource(DownloadItemResources.DownloadRunningFormatted, (int) DownloadTask.ProgressPercentage),
        DownloadState.Error => I18NManager.GetResource(DownloadItemResources.DownloadErrorMessageFormatted, ErrorMessage),
        DownloadState.Completed => I18NManager.GetResource(DownloadItemResources.DownloadCompleted),
        DownloadState.Cancelled => I18NManager.GetResource(DownloadItemResources.DownloadCancelled),
        DownloadState.Pending => I18NManager.GetResource(DownloadItemResources.DownloadPending),
        DownloadState.Paused => I18NManager.GetResource(DownloadItemResources.DownloadPaused),
        _ => ""
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
        _ => ""
    };

    public bool IsItemEnabled => !DownloadTask.IsProcessing || CurrentState is DownloadState.Completed;

    public bool IsResetItemEnabled => !DownloadTask.IsProcessing && CurrentState is DownloadState.Completed or DownloadState.Error;

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
                DownloadTask.Reset();
                break;
            case Symbol.Play:
                DownloadTask.Resume();
                break;
            case Symbol.Open:
                break;
        }
    }

    public override string ThumbnailUrl => Entry.Thumbnails.PickMax()?.ImageUri.OriginalString ?? "";

    public string? ErrorMessage => DownloadTask.ErrorMessage;

    public bool MatchesSearch(string key) =>
        Entry.Title.Contains(key, StringComparison.OrdinalIgnoreCase)
        || DownloadTask.Id.Contains(key, StringComparison.OrdinalIgnoreCase);

    public bool MatchesOption(DownloadListOption option, ISet<IDownloadListEntryViewModel>? customSearchResult) => option switch
    {
        DownloadListOption.AllQueued => true,
        DownloadListOption.Running => CurrentState is DownloadState.Running,
        DownloadListOption.Completed => CurrentState is DownloadState.Completed,
        DownloadListOption.Cancelled => CurrentState is DownloadState.Cancelled,
        DownloadListOption.Error => CurrentState is DownloadState.Error,
        DownloadListOption.CustomSearch => customSearchResult?.Contains(this) ?? true,
        _ => true
    };

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
            OnPropertyChanged(nameof(IsResetItemEnabled));
            OnPropertyChanged(nameof(IsCancelItemEnabled));
        }
        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState)
            or nameof(IDownloadTaskBase.ProgressPercentage)
            or nameof(IDownloadTaskBase.ErrorMessage))
        {
            OnPropertyChanged(nameof(ProgressMessage));
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
}
