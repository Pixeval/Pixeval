// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Misaki;
using Pixeval.Collections;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public class SimpleOperableViewViewModel<TViewModel> : ViewModelBase, IOperableViewViewModel, IDisposable
    where TViewModel : class, IWorkViewModel
{
    public SimpleOperableViewViewModel(IReadOnlyCollection<IArtworkInfo> source, bool needRefreshOnOpen = false)
    {
        NeedRefreshOnOpen = needRefreshOnOpen;
        SourceView = new(source);
        SetFilters();
    }

    public SimpleOperableSourceView<TViewModel> SourceView { get; }

    public bool NeedRefreshOnOpen { get; }

    public FrozenSet<string> CachedBlockedTags { get; } = [.. App.AppViewModel.AppSettings.BlockedTags];

    public IFilter<IWorkViewModel> BlockedTagsFilter => IFilter<IWorkViewModel>.Create(
        entry => !entry.Entry.Tags.Any(t => t.Any(tag => CachedBlockedTags.Contains(tag.Name))),
        false);

    private static IFilter<IWorkViewModel> TypeFilter { get; } = IFilter<IWorkViewModel>.Create(entry => entry is TViewModel, false);

    /// <inheritdoc />
    public bool IsSelecting { get; set; }

    /// <inheritdoc />
    public AvaloniaList<IWorkViewModel> SelectedEntries { get; } = [];

    public void SetSortDescriptions(params IEnumerable<ISortDescription<IWorkViewModel>> descriptions)
    {
        using (SourceView.View.DeferSortDescriptionsChange())
        {
            SourceView.View.SortDescriptions.Clear();
            SourceView.View.SortDescriptions.AddRange(descriptions);
        }
    }

    private void SetFilters()
    {
        using (SourceView.View.DeferFiltersChange())
        {
            SourceView.View.Filters.Clear();
            SourceView.View.Filters.Add(TypeFilter);
            SourceView.View.Filters.Add(BlockedTagsFilter);
            if (UserFilter is not null)
                SourceView.View.Filters.Add(UserFilter);
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

    /// <inheritdoc />
    IReadOnlyCollection<IWorkViewModel> IOperableViewViewModel.View => SourceView.View;

    /// <inheritdoc />
    public IReadOnlyCollection<IWorkViewModel> Source => SourceView.Source;

    /// <inheritdoc />
    public bool RequireAdaptiveGrid => typeof(TViewModel) == typeof(NovelItemViewModel);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SourceView.Dispose();
    }
}
