// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface IOperableViewViewModel : INotifyPropertyChanged
{
    bool IsSelecting { get; set; }

    AvaloniaList<IWorkViewModel> SelectedEntries { get; }

    void SetSortDescription(ISortDescription<IWorkViewModel> description);

    void ClearSortDescription();

    Func<IWorkViewModel, bool>? Filter { get; set; }

    IReadOnlyCollection<IWorkViewModel> View { get; }

    Range ViewRange { get; set; }

    bool RequireAdaptiveGrid { get; }

    HashSet<string> CachedBlockedTags { get; }

    bool DefaultFilter(IWorkViewModel entry)
    {
        if (entry.Entry.Tags.Any(t => t.Any(tag => CachedBlockedTags.Contains(tag.Name))))
            return false;

        return Filter?.Invoke(entry) is not false;
    }
}
