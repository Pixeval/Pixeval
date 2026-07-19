// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Collections;
using Pixeval.Models.Options;

namespace Pixeval.ViewModels;

public sealed partial class DownloadItemPageViewModel : ViewModelBase, IDisposable
{
    private readonly ObservableCollection<DownloadItemViewModel> _source;

    private readonly List<DownloadItemViewModel> _filteredTasks = [];

    private bool _isDisposed;

    [ObservableProperty]
    public partial DownloadListOption CurrentOption { get; set; } = DownloadListOption.AllQueued;

    [ObservableProperty]
    public partial string? FilterText { get; set; }

    public DownloadPageViewModel PageViewModel { get; }

    public DownloadFolderViewModel? Folder { get; }

    public AdvancedObservableCollection<DownloadItemViewModel> View { get; }

    partial void OnCurrentOptionChanged(DownloadListOption value) => ResetFilter(GetCustomSearchResult());

    partial void OnFilterTextChanged(string? value) => UpdateFilteredTasks(value);

    public DownloadItemPageViewModel(DownloadPageViewModel pageViewModel, DownloadFolderViewModel? folder = null)
    {
        PageViewModel = pageViewModel;
        Folder = folder;
        _source = folder?.Items ?? pageViewModel.OrdinaryItems;
        View = new AdvancedObservableCollection<DownloadItemViewModel>(_source, true);
        ResetFilter();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        View.Filters.Clear();
        View.Source = [];
        View.Dispose();
        _filteredTasks.Clear();
    }

    private void ResetFilter(IEnumerable<DownloadItemViewModel>? customSearchResult = null)
    {
        if (_isDisposed)
            return;

        var filterSource = customSearchResult?
            .Cast<IDownloadListEntryViewModel>()
            .ToHashSet();
        using (View.DeferFiltersChange())
        {
            View.Filters.Clear();
            View.Filters.Add(IFilter<DownloadItemViewModel>.Create(
                item => item.MatchesOption(CurrentOption, filterSource),
                false));
        }
    }

    private IReadOnlyCollection<DownloadItemViewModel>? GetCustomSearchResult() =>
        CurrentOption is DownloadListOption.CustomSearch && !string.IsNullOrWhiteSpace(FilterText)
            ? _filteredTasks
            : null;

    private void UpdateFilteredTasks(string? key)
    {
        _filteredTasks.Clear();
        if (!string.IsNullOrWhiteSpace(key))
            foreach (var item in _source.Where(item => item.MatchesSearch(key)))
                _filteredTasks.Add(item);

        ResetFilter(GetCustomSearchResult());
    }
}
