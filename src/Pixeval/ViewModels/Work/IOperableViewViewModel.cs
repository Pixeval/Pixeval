// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

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

    IFilter<IWorkViewModel>? UserFilter { get; set; }

    bool RequireAdaptiveGrid { get; }

    /// <summary>
    /// 按当前屏蔽列表，将已加载但新近被屏蔽的作品替换为占位项
    /// </summary>
    void ReplaceBlockedEntries();

    new IReadOnlyCollection<IWorkViewModel> View { get; }

    new IReadOnlyCollection<IWorkViewModel> Source { get; }

    IReadOnlyCollection<INotifyPropertyChanged> ISimpleViewViewModel.View => View;

    IReadOnlyCollection<INotifyPropertyChanged> ISimpleViewViewModel.Source => Source;
}
