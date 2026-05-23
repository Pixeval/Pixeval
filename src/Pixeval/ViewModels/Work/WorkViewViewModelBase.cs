// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public abstract partial class WorkViewViewModelBase<T, TViewModel>(FrozenSet<string>? blockedTags) : EntryViewViewModel<T, TViewModel>, IWorkViewViewModel
    where T : class, IArtworkInfo
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>, IWorkViewModel
{
    public FrozenSet<string> CachedBlockedTags { get; private set; } = blockedTags ?? App.AppViewModel.AppSettings.BlockedTags.ToFrozenSet();

    public IFilter<IWorkViewModel> BlockedTagsFilter
    {
        get
        {
            var cachedBlockedTags = CachedBlockedTags;
            return IFilter<IWorkViewModel>.Create(
                entry => !entry.Entry.Tags.Any(t => t.Any(tag => cachedBlockedTags.Contains(tag.Name))),
                false);
        }
    }

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    public AvaloniaList<IWorkViewModel> SelectedEntries { get; } = [];

    public void SetSortDescriptions(params IEnumerable<ISortDescription<IWorkViewModel>> descriptions)
    {
        using (DataProvider.View.DeferSortDescriptionsChange())
        {
            DataProvider.View.SortDescriptions.Clear();
            DataProvider.View.SortDescriptions.AddRange(descriptions);
        }
    }

    protected void SetFilters()
    {
        using (DataProvider.View.DeferFiltersChange())
        {
            DataProvider.View.Filters.Clear();
            DataProvider.View.Filters.Add(BlockedTagsFilter);
            if (UserFilter is not null)
                DataProvider.View.Filters.Add(UserFilter);
        }
    }

    public IFilter<IWorkViewModel>? UserFilter
    {
        get;
        set
        {
            if (Equals(field, value))
                return;

            field = value;
            SetFilters();
        }
    }

    public IReadOnlyCollection<IWorkViewModel> View => DataProvider.View;

    public IReadOnlyCollection<IWorkViewModel> Source => DataProvider.Source;

    public Range ViewRange
    {
        get => DataProvider.View.Range;
        set => DataProvider.View.Range = value;
    }

    public abstract bool RequireAdaptiveGrid { get; }

    public void ResetEngine(IFetchEngine<IArtworkInfo>? newEngine, bool isBookmarkEnabled = true, int itemsPerPage = 20, int itemLimit = -1)
    {
        CachedBlockedTags = [.. App.AppViewModel.AppSettings.BlockedTags.ToFrozenSet()];
        ResetEngine((IFetchEngine<T>?) newEngine, (info, _) => TViewModel.CreateInstance(info).Apply(t => t.IsBookmarkEnabled = isBookmarkEnabled), itemsPerPage, itemLimit);
        SetFilters();
    }
}
