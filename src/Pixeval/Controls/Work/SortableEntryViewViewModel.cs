#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SortableIllustrateViewViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public abstract partial class SortableEntryViewViewModel<T, TViewModel> : EntryViewViewModel<T, TViewModel>, ISortableEntryViewViewModel where T : class, IWorkEntry where TViewModel : EntryViewModel<T>, IWorkViewModel
{
    [ObservableProperty]
    private bool _isSelecting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyEntrySelected))]
    [NotifyPropertyChangedFor(nameof(SelectionLabel))]
    private TViewModel[] _selectedEntries = [];

    private Func<IWorkViewModel, bool>? _filter;

    IReadOnlyCollection<IWorkViewModel> ISortableEntryViewViewModel.SelectedEntries
    {
        get => SelectedEntries;
        set => SelectedEntries = (TViewModel[])value;
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
        get => _filter;
        set
        {
            if (Equals(value, _filter))
                return;
            _filter = value;
            OnFilterChanged();
            OnPropertyChanged();
        }
    }

    public IReadOnlyCollection<IWorkViewModel> View => DataProvider.View;

    public IReadOnlyCollection<IWorkViewModel> Source => DataProvider.Source;

    public void ResetEngine(IFetchEngine<IWorkEntry>? newEngine, int itemLimit = -1)
    {
        DataProvider.ResetEngine((IFetchEngine<T>?)newEngine, itemLimit);
    }

    protected bool DefaultFilter(IWorkViewModel entry)
    {
        if (entry.Tags.Any(tag => App.AppViewModel.AppSettings.BlockedTags.Contains(tag.Name)))
            return false;

        return Filter?.Invoke(entry) is not false;
    }

    protected abstract void OnFilterChanged();
}
