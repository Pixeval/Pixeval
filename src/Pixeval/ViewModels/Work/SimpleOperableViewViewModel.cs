// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Collections;
using Mako.Model;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public class SimpleOperableViewViewModel(IReadOnlyCollection<IArtworkInfo> source)
    : ViewModelBase, IOperableViewViewModel
{
    /// <inheritdoc />
    public HashSet<string> CachedBlockedTags { get; } = [.. App.AppViewModel.AppSettings.BlockedTags];

    /// <inheritdoc />
    public bool IsSelecting { get; set; }

    /// <inheritdoc />
    public AvaloniaList<IWorkViewModel> SelectedEntries { get; } = [];

    /// <inheritdoc />
    public void SetSortDescription(ISortDescription<IWorkViewModel> description)
    {
        var sortDescriptions = View.SortDescriptions;
        if (sortDescriptions.Count is 0)
            sortDescriptions.Add(description);
        else
            sortDescriptions[0] = description;
    }

    /// <inheritdoc />
    public void ClearSortDescription()
    {
        View.SortDescriptions.Clear();
    }

    /// <inheritdoc />
    public Func<IWorkViewModel, bool>? Filter { get; set; }

    private AdvancedObservableAdaptor<IArtworkInfo, IWorkViewModel> View { get; } = new(source as ObservableCollection<IArtworkInfo> ?? [.. source], CreateWorkViewModel);

    /// <inheritdoc />
    IReadOnlyCollection<IWorkViewModel> IOperableViewViewModel.View => View;

    /// <inheritdoc />
    public Range ViewRange { get; set; }

    /// <inheritdoc />
    public bool RequireAdaptiveGrid => false;

    private static IWorkViewModel CreateWorkViewModel(IArtworkInfo info) => info is Novel novel ? new NovelItemViewModel(novel) : new IllustrationItemViewModel(info);
}
