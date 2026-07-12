// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Pixeval.Controls;
using Pixeval.Download;
using Pixeval.I18N;
using Pixeval.Models.Database;
using Pixeval.Models.Options;

namespace Pixeval.ViewModels;

public sealed class DownloadFolderViewModel(WorkSubscriptionEntry subscription)
    : ViewModelBase, IDownloadListEntryViewModel, IDisposable
{
    private bool _isDisposed;

    public WorkSubscriptionEntry Subscription { get; } = subscription;

    public ObservableCollection<DownloadItemViewModel> Items { get; } = [];

    public IReadOnlyList<DownloadItemViewModel> DownloadItems => [.. Items];

    public string Title => GetDisplayName(Subscription);

    public static string GetDisplayName(WorkSubscriptionEntry subscription) =>
        $"{subscription.DisplayName} · " +
        $"{SymbolComboBoxItem.GetResource(subscription.SubscriptionType)} · " +
        $"{SymbolComboBoxItem.GetResource(subscription.WorkKind)}";

    public string Subtitle => I18NManager.GetResource(DownloadPageResources.FolderSubtitleFormatted, Items.Count);

    public bool HasItems => Items.Count is not 0;

    public DownloadState CurrentState
    {
        get
        {
            var states = Items.Select(t => t.CurrentState).ToArray();
            return states switch
            {
                [] => DownloadState.Completed,
                _ when states.Any(t => t is DownloadState.Error) => DownloadState.Error,
                _ when states.Any(t => t is DownloadState.Cancelled) => DownloadState.Cancelled,
                _ when states.Any(t => t is DownloadState.Paused) => DownloadState.Paused,
                _ when states.Any(t => t is DownloadState.Running) => DownloadState.Running,
                _ when states.Any(t => t is DownloadState.Queued) => DownloadState.Queued,
                _ when states.Any(t => t is DownloadState.Pending) => DownloadState.Pending,
                _ => DownloadState.Completed
            };
        }
    }

    public int ActiveCount => Items.Sum(t => t.DownloadTask.ActiveCount);

    public int CompletedCount => Items.Sum(t => t.DownloadTask.CompletedCount);

    public int ErrorCount => Items.Sum(t => t.DownloadTask.ErrorCount);

    public double ProgressPercentage => Items.Count is 0
        ? 100
        : Items.Average(t => t.DownloadTask.ProgressPercentage);

    public string StateBrushKey => CurrentState switch
    {
        DownloadState.Paused => "SystemFillColorCautionBrush",
        DownloadState.Cancelled => "SystemFillColorNeutralBrush",
        _ => "SystemFillColorAttentionBrush",
    };

    public bool MatchesSearch(string key) =>
        Title.Contains(key, StringComparison.OrdinalIgnoreCase)
        || Items.Any(t => t.MatchesSearch(key));

    public bool MatchesOption(DownloadListOption option, ISet<IDownloadListEntryViewModel>? customSearchResult) => option switch
    {
        DownloadListOption.AllQueued => true,
        DownloadListOption.CustomSearch => customSearchResult?.Contains(this) ?? true,
        _ => Items.Any(t => t.MatchesOption(option, null))
    };

    public void Add(DownloadItemViewModel item)
    {
        Items.Insert(0, item);
        item.DownloadTask.PropertyChanged += DownloadTaskOnPropertyChanged;
        OnItemsChanged();
    }

    public bool Remove(DownloadItemViewModel item)
    {
        if (!Items.Remove(item))
            return false;

        item.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;
        OnItemsChanged();
        return true;
    }

    private void DownloadTaskOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState))
        {
            OnPropertyChanged(nameof(CurrentState));
            OnPropertyChanged(nameof(StateBrushKey));
            OnPropertyChanged(nameof(ActiveCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(ErrorCount));
        }

        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState)
            or nameof(IDownloadTaskBase.ProgressPercentage))
            OnPropertyChanged(nameof(ProgressPercentage));
    }

    private void OnItemsChanged()
    {
        OnPropertyChanged(nameof(DownloadItems));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(StateBrushKey));
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(ErrorCount));
        OnPropertyChanged(nameof(ProgressPercentage));
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        foreach (var item in Items)
            item.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;
        Items.Clear();
    }
}
