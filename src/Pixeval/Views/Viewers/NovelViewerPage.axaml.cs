// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

public partial class NovelViewerPage : ContentPage
{
    private NovelViewerPageViewModel ViewModel => (NovelViewerPageViewModel) DataContext!;

    public NovelViewerPage() : this(null)
    {
    }

    public NovelViewerPage(NovelViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    private void PrevButton_OnRightClick(object? sender, TappedEventArgs e)
    {
        ViewModel.CurrentWorkIndex--;
    }

    private void NextButton_OnRightClick(object? sender, TappedEventArgs e)
    {
        ViewModel.CurrentWorkIndex++;
    }

    private async void AddToBookmarkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (BookmarkTagSelector.IsVisible)
        {
            BookmarkTagSelector.IsVisible = false;
            return;
        }

        BookmarkTagSelector.IsVisible = true;
        await BookmarkTagSelector.ResetSourceAsync();
    }

    private async void BookmarkTagSelector_OnTagsSelected(TagSelector sender, (bool isPrivate, IReadOnlyList<string> tags) e)
    {
        if (ViewModel.CurrentNovel is IWorkViewModel current)
        {
            await current.AddToBookmarkCommand.ExecuteAsync((e.tags, e.isPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(EntryViewerPageResources.AddedToBookmark));
        }

        BookmarkTagSelector.IsVisible = false;
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, ViewModel));
    }

    #endregion
}
