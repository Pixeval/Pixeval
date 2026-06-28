// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public abstract partial class WorkViewViewModelBase<T, TViewModel>(FrozenSet<string>? blockedTags) : EntryViewViewModel<T, TViewModel>, IWorkViewViewModel
    where T : class, IArtworkInfo
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>, IWorkViewModel
{
    public FrozenSet<string> CachedBlockedTags { get; private set; } = blockedTags ?? App.AppViewModel.AppSettings.BrowsingExperienceSettings.BlockedTags.ToFrozenSet();

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
        using (View.DeferSortDescriptionsChange())
        {
            View.SortDescriptions.Clear();
            View.SortDescriptions.AddRange(descriptions);
        }
    }

    protected void SetFilters()
    {
        using (View.DeferFiltersChange())
        {
            View.Filters.Clear();
            View.Filters.Add(BlockedTagsFilter);
            if (UserFilter is not null)
                View.Filters.Add(UserFilter);
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

    IReadOnlyCollection<IWorkViewModel> IOperableViewViewModel.View => View;

    IReadOnlyCollection<IWorkViewModel> IOperableViewViewModel.Source => Source;

    public abstract bool RequireAdaptiveGrid { get; }

    public void ResetEngine(IAsyncEnumerable<IArtworkInfo>? newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        CachedBlockedTags = [.. App.AppViewModel.AppSettings.BrowsingExperienceSettings.BlockedTags.ToFrozenSet()];
        ResetEngine((IAsyncEnumerable<T>?) newEngine, (info, _) => TViewModel.CreateInstance(info), itemsPerPage, itemLimit);
        SetFilters();
    }
}
