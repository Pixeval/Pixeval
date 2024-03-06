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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public abstract partial class SortableEntryViewViewModel<T, TViewModel> : EntryViewViewModel<T, TViewModel>, ISortableEntryViewViewModel where T : class, IEntry where TViewModel : EntryViewModel<T>, IBookmarkableViewModel
{
    [ObservableProperty]
    private bool _isSelecting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyEntrySelected))]
    [NotifyPropertyChangedFor(nameof(SelectionLabel))]
    private TViewModel[] _selectedEntries = [];

    IReadOnlyCollection<IBookmarkableViewModel> ISortableEntryViewViewModel.SelectedEntries => SelectedEntries;

    public bool IsAnyEntrySelected => SelectedEntries.Length > 0;

    public string SelectionLabel => IsAnyEntrySelected
        ? EntryContainerResources.CancelSelectionButtonFormatted.Format(SelectedEntries.Length)
        : EntryContainerResources.CancelSelectionButtonDefaultLabel;

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

    public Func<IBookmarkableViewModel, bool>? Filter
    {
        get => (Func<IBookmarkableViewModel, bool>?)DataProvider.View.Filter;
        set => DataProvider.View.Filter = value;
    }
}
