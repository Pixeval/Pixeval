// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Mako.Model;
using Misaki;
using Pixeval.Collections;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public class SimpleOperableViewViewModel : ViewModelBase, IOperableViewViewModel
{
    public SimpleOperableViewViewModel(IReadOnlyCollection<IArtworkInfo> source)
    {
        View = new(source as ObservableCollection<IArtworkInfo> ?? [.. source], CreateWorkViewModel);
        SetFilters();
    }

    public FrozenSet<string> CachedBlockedTags { get; } = [.. App.AppViewModel.AppSettings.BlockedTags];

    public IFilter<IWorkViewModel> BlockedTagsFilter => IFilter<IWorkViewModel>.Create(
        entry => !entry.Entry.Tags.Any(t => t.Any(tag => CachedBlockedTags.Contains(tag.Name))),
        false);

    /// <inheritdoc />
    public bool IsSelecting { get; set; }

    /// <inheritdoc />
    public AvaloniaList<IWorkViewModel> SelectedEntries { get; } = [];

    public void SetSortDescriptions(params IEnumerable<ISortDescription<IWorkViewModel>> descriptions)
    {
        using (View.DeferSortDescriptionsChange())
        {
            View.SortDescriptions.Clear();
            View.SortDescriptions.AddRange(descriptions);
        }
    }

    private void SetFilters()
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

    private AdvancedObservableAdaptor<IArtworkInfo, IWorkViewModel> View { get; }

    /// <inheritdoc />
    IReadOnlyCollection<IWorkViewModel> IOperableViewViewModel.View => View;

    /// <inheritdoc />
    public Range ViewRange
    {
        get => View.Range;
        set => View.Range = value;
    }

    /// <inheritdoc />
    public bool RequireAdaptiveGrid => false;

    private static IWorkViewModel CreateWorkViewModel(IArtworkInfo info) => info is Novel novel ? new NovelItemViewModel(novel) : new IllustrationItemViewModel(info);
}
