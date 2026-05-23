// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Pixeval.Controls;
using Pixeval.Download;
using Pixeval.Models.Database;

namespace Pixeval.ViewModels;

public sealed class DownloadFolderViewModel(WorkSubscriptionEntry subscription)
    : ViewModelBase, IDownloadListEntryViewModel
{
    public WorkSubscriptionEntry Subscription { get; } = subscription;

    public ObservableCollection<DownloadItemViewModel> Items { get; } = [];

    public IReadOnlyList<DownloadItemViewModel> DownloadItems => [.. Items];

    public string Title => GetDisplayName(Subscription);

    public static string GetDisplayName(WorkSubscriptionEntry subscription) =>
        $"{subscription.DisplayName} · " +
        $"{SymbolComboBoxItem.GetResource(subscription.SubscriptionType)} · " +
        $"{SymbolComboBoxItem.GetResource(subscription.WorkKind)}";

    public string Subtitle => $"{Items.Count} 个下载任务";

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

    public double ProgressPercentage => Items.Count is 0
        ? 100
        : Items.Average(t => t.DownloadTask.ProgressPercentage);

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
        item.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;
        var result = Items.Remove(item);
        if (result)
            OnItemsChanged();
        return result;
    }

    private void DownloadTaskOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IDownloadTaskBase.CurrentState))
            OnPropertyChanged(nameof(CurrentState));
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
        OnPropertyChanged(nameof(ProgressPercentage));
    }
}
