// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Collections;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface IOperableViewViewModel : INotifyPropertyChanged
{
    bool IsSelecting { get; set; }

    AvaloniaList<IWorkViewModel> SelectedEntries { get; }

    void SetSortDescriptions(params IEnumerable<ISortDescription<IWorkViewModel>> descriptions);

    IFilter<IWorkViewModel> BlockedTagsFilter { get; }

    IFilter<IWorkViewModel>? UserFilter { get; set; }

    IReadOnlyCollection<IWorkViewModel> View { get; }

    Range ViewRange { get; set; }

    bool RequireAdaptiveGrid { get; }
}
