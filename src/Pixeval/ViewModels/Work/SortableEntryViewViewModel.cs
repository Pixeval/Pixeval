// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;

namespace Pixeval.ViewModels;

public abstract partial class SortableEntryViewViewModel<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel>(HashSet<string>? blockedTags)
    : EntryViewViewModel<T, TViewModel>, ISortableEntryViewViewModel
    where T : class, IArtworkInfo
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>, IWorkViewModel
{
    protected HashSet<string> BlockedTags = blockedTags?.ToHashSet() ?? App.AppViewModel.AppSettings.BlockedTags.ToHashSet();

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    [ObservableProperty]
    public partial IReadOnlyCollection<IWorkViewModel> SelectedEntries { get; set; } = new AvaloniaList<IWorkViewModel>();

    public void SetSortDescription(ISortDescription<IWorkViewModel> description)
    {
        if (DataProvider.View.SortDescriptions.Count is 0)
            DataProvider.View.SortDescriptions.Add(description);
        else
            DataProvider.View.SortDescriptions[0] = description;
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
            OnFilterChanged();
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

    public void ResetEngine(IFetchEngine<IArtworkInfo>? newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        BlockedTags = [.. App.AppViewModel.AppSettings.BlockedTags];
        DataProvider.ResetEngine((IFetchEngine<T>?) newEngine, itemsPerPage, itemLimit);
    }

    public void ResetSource(ObservableCollection<IArtworkInfo>? source) => throw new NotSupportedException();

    protected bool DefaultFilter(IWorkViewModel entry)
    {
        if (entry.Entry.Tags.Any(t => t.Any(tag => BlockedTags.Contains(tag.Name))))
            return false;

        return Filter?.Invoke(entry) is not false;
    }

    protected abstract void OnFilterChanged();
}
