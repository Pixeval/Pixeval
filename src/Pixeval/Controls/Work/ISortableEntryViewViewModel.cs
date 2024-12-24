#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ISortableEntryViewViewModel.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public interface ISortableEntryViewViewModel : INotifyPropertyChanged, IDisposable
{
    bool IsAnyEntrySelected { get; }

    bool IsSelecting { get; set; }

    bool HasNoItem { get; }

    string SelectionLabel { get; }

    IReadOnlyCollection<IWorkViewModel> SelectedEntries { get; set; }

    void SetSortDescription(SortDescription description);

    void ClearSortDescription();

    Func<IWorkViewModel, bool>? Filter { get; set; }

    IReadOnlyCollection<IWorkViewModel> View { get; }

    IReadOnlyCollection<IWorkViewModel> Source { get; }

    Range ViewRange { get; set; }

    void ResetEngine(IFetchEngine<IWorkEntry>? newEngine, int itemsPerPage = 20, int itemLimit = -1);

    void ResetSource(ObservableCollection<IWorkEntry>? source);
}
