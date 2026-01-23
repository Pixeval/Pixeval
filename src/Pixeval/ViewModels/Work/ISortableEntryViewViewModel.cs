// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Mako.Engine;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface ISortableEntryViewViewModel : INotifyPropertyChanged, IDisposable
{
    bool IsSelecting { get; set; }

    IReadOnlyCollection<IWorkViewModel> SelectedEntries { get; set; }

    void SetSortDescription(ISortDescription<IWorkViewModel> description);

    void ClearSortDescription();

    Func<IWorkViewModel, bool>? Filter { get; set; }

    IReadOnlyCollection<IWorkViewModel> View { get; }

    IReadOnlyCollection<IWorkViewModel> Source { get; }

    Range ViewRange { get; set; }

    void ResetEngine(IFetchEngine<IArtworkInfo>? newEngine, int itemsPerPage = 20, int itemLimit = -1);

    void ResetSource(ObservableCollection<IArtworkInfo>? source);
}
