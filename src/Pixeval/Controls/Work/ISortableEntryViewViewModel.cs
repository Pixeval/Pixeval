// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
