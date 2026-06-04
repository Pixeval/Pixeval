// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pixeval.I18N;

namespace Pixeval.ViewModels.Viewers;

public abstract partial class PagedViewerViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsBottomListOpen { get; set; }

    public abstract int CurrentWorkIndex { get; set; }

    public abstract int CurrentPageIndex { get; set; }

    public PagedBehavior PrevAction
    {
        get
        {
            if (CurrentWorkIndex < 0)
                return PagedBehavior.None;

            var behavior = PagedBehavior.None;
            if (CurrentPageIndex > 0)
                behavior |= PagedBehavior.Page;
            if (CurrentWorkIndex > 0)
                behavior |= PagedBehavior.Work;
            return behavior;
        }
    }

    public PagedBehavior NextAction
    {
        get
        {
            if (CurrentWorkIndex < 0)
                return PagedBehavior.None;

            var behavior = PagedBehavior.None;
            if (CurrentPageIndex < PageCount - 1)
                behavior |= PagedBehavior.Page;
            if (CurrentWorkIndex < WorkCount - 1)
                behavior |= PagedBehavior.Work;
            return behavior;
        }
    }

    public abstract int PageCount { get; }

    public abstract int WorkCount { get; }

    public string? PrevButtonText => PrevAction switch
    {
        PagedBehavior.PageAndWork => I18NManager.GetResource(EntryViewerPageResources.PrevPageOrWork),
        PagedBehavior.Work => I18NManager.GetResource(EntryViewerPageResources.PrevWork),
        PagedBehavior.Page => I18NManager.GetResource(EntryViewerPageResources.PrevPage),
        _ => null
    };

    public string? NextButtonText => NextAction switch
    {
        PagedBehavior.PageAndWork => I18NManager.GetResource(EntryViewerPageResources.NextPageOrWork),
        PagedBehavior.Work => I18NManager.GetResource(EntryViewerPageResources.NextWork),
        PagedBehavior.Page => I18NManager.GetResource(EntryViewerPageResources.NextPage),
        _ => null
    };

    #region Commands

    private bool CanPrev => PrevAction is not PagedBehavior.None;

    [RelayCommand(CanExecute = nameof(CanPrev))]
    private void Prev()
    {
        if (PrevAction is PagedBehavior.Work)
            CurrentWorkIndex--;
        else
            CurrentPageIndex--;
    }

    private bool CanNext => NextAction is not PagedBehavior.None;

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next()
    {
        if (NextAction is PagedBehavior.Work)
            CurrentWorkIndex++;
        else
            CurrentPageIndex++;
    }

    private bool CanPrevWork => PrevAction.HasFlag(PagedBehavior.Work);

    [RelayCommand(CanExecute = nameof(CanPrevWork))]
    private void PrevWork() => CurrentWorkIndex--;

    private bool CanNextWork => NextAction.HasFlag(PagedBehavior.Work);

    [RelayCommand(CanExecute = nameof(CanNextWork))]
    private void NextWork() => CurrentWorkIndex++;

    #endregion
}

[Flags]
public enum PagedBehavior
{
    None,
    Page,
    Work,
    PageAndWork = Page | Work
}
