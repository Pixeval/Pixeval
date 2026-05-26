// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Collections;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface ISimpleViewViewModel : INotifyPropertyChanged
{
    IReadOnlyCollection<INotifyPropertyChanged> View { get; }

    IReadOnlyCollection<INotifyPropertyChanged> Source { get; }
}

public interface IOperableViewViewModel : ISimpleViewViewModel
{
    bool IsSelecting { get; set; }

    AvaloniaList<IWorkViewModel> SelectedEntries { get; }

    void SetSortDescriptions(params IEnumerable<ISortDescription<IWorkViewModel>> descriptions);

    IFilter<IWorkViewModel> BlockedTagsFilter { get; }

    IFilter<IWorkViewModel>? UserFilter { get; set; }

    Range ViewRange { get; set; }

    bool RequireAdaptiveGrid { get; }

    new IReadOnlyCollection<IWorkViewModel> View { get; }

    new IReadOnlyCollection<IWorkViewModel> Source { get; }

    IReadOnlyCollection<INotifyPropertyChanged> ISimpleViewViewModel.View => View;

    IReadOnlyCollection<INotifyPropertyChanged> ISimpleViewViewModel.Source => Source;
}
