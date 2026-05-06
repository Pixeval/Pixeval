// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        View.Filter = ((IOperableViewViewModel) this).DefaultFilter;
    }

    /// <inheritdoc />
    public HashSet<string> CachedBlockedTags { get; } = [.. App.AppViewModel.AppSettings.BlockedTags];

    /// <inheritdoc />
    public bool IsSelecting { get; set; }

    /// <inheritdoc />
    public AvaloniaList<IWorkViewModel> SelectedEntries { get; } = [];

    public void SetSortDescription(params IReadOnlyCollection<ISortDescription<IWorkViewModel>> descriptions)
    {
        using (View.DeferSortDescriptionsChange())
        {
            View.SortDescriptions.Clear();
            View.SortDescriptions.AddRange(descriptions);
        }
    }

    /// <inheritdoc />
    public Func<IWorkViewModel, bool>? Filter
    {
        get;
        set
        {
            if (Equals(value, field))
                return;
            field = value;
            View.RaiseFilterChanged();
            OnPropertyChanged();
        }
    }

    private AdvancedObservableAdaptor<IArtworkInfo, IWorkViewModel> View { get; }

    /// <inheritdoc />
    IReadOnlyCollection<IWorkViewModel> IOperableViewViewModel.View => View;

    /// <inheritdoc />
    public Range ViewRange { get; set; }

    /// <inheritdoc />
    public bool RequireAdaptiveGrid => false;

    private static IWorkViewModel CreateWorkViewModel(IArtworkInfo info) => info is Novel novel ? new NovelItemViewModel(novel) : new IllustrationItemViewModel(info);
}
