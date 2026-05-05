// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
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

public abstract partial class WorkViewViewModelBase<T, TViewModel>(HashSet<string>? blockedTags)
    : EntryViewViewModel<T, TViewModel>, IWorkViewViewModel
    where T : class, IArtworkInfo
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>, IWorkViewModel
{
    public HashSet<string> CachedBlockedTags { get; private set; } = blockedTags?.ToHashSet() ?? App.AppViewModel.AppSettings.BlockedTags.ToHashSet();

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    public AvaloniaList<IWorkViewModel> SelectedEntries { get; } = [];

    public void SetSortDescription(ISortDescription<IWorkViewModel> description)
    {
        var sortDescriptions = DataProvider.View.SortDescriptions;
        if (sortDescriptions.Count is 0)
            sortDescriptions.Add(description);
        else
            sortDescriptions[0] = description;
    }

    public void ClearSortDescription()
    {
        DataProvider.View.SortDescriptions.Clear();
    }

    public Func<IWorkViewModel, bool>? Filter
    {
        get;
        set
        {
            if (Equals(value, field))
                return;
            field = value;
            DataProvider.View.RaiseFilterChanged();
            OnPropertyChanged();
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
        CachedBlockedTags = [.. App.AppViewModel.AppSettings.BlockedTags];
        ResetEngine((IFetchEngine<T>?) newEngine, (info, _) => TViewModel.CreateInstance(info).Apply(t => t.IsBookmarkEnabled = isBookmarkEnabled), itemsPerPage, itemLimit);
    }
}
