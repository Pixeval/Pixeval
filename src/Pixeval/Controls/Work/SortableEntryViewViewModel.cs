// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Mako.Engine;
using Mako.Model;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public abstract partial class SortableEntryViewViewModel<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel>(HashSet<string> blockedTags)
    : EntryViewViewModel<T, TViewModel>, ISortableEntryViewViewModel
    where T : class, IWorkEntry
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>, IWorkViewModel
{
    protected readonly HashSet<string> BlockedTags = [.. blockedTags];

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyEntrySelected))]
    [NotifyPropertyChangedFor(nameof(SelectionLabel))]
    public partial TViewModel[] SelectedEntries { get; set; } = [];

    IReadOnlyCollection<IWorkViewModel> ISortableEntryViewViewModel.SelectedEntries
    {
        get => SelectedEntries;
        set => SelectedEntries = (TViewModel[]) value;
    }

    public bool IsAnyEntrySelected => SelectedEntries.Length > 0;

    public string SelectionLabel => IsAnyEntrySelected
        ? WorkContainerResources.CancelSelectionButtonFormatted.Format(SelectedEntries.Length)
        : WorkContainerResources.CancelSelectionButtonDefaultLabel;

    public void SetSortDescription(SortDescription description)
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

    public void ResetEngine(IFetchEngine<IWorkEntry>? newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        DataProvider.ResetEngine((IFetchEngine<T>?) newEngine, itemsPerPage, itemLimit);
    }

    public void ResetSource(ObservableCollection<IWorkEntry>? source) => ThrowHelper.NotSupported();

    protected bool DefaultFilter(IWorkViewModel entry)
    {
        if (entry.Tags.Any(tag => BlockedTags.Contains(tag.Name)))
            return false;

        return Filter?.Invoke(entry) is not false;
    }

    protected abstract void OnFilterChanged();
}
