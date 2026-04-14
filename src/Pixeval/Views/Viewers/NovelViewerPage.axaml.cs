// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Data.Converters;
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

    public static readonly FuncValueConverter<int, string> PlusOneConverter = new(i => (i + 1).ToString());

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
        if (!BookmarkTagSelector.IsVisible)
            await BookmarkTagSelector.ResetSourceAsync();
        BookmarkTagSelector.IsVisible = !BookmarkTagSelector.IsVisible;
    }

    private void BookmarkTagSelector_OnTagsSelected(TagSelector sender, (bool IsPrivate, IReadOnlyList<string> Tags) e)
    {
        if (ViewModel.CurrentNovel.AddToBookmarkCommand is { } command)
        {
            command.Execute((e.Tags, e.IsPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(EntryViewerPageResources.AddedToBookmark));
        }

        BookmarkTagSelector.IsVisible = false;
    }

    private async void Thumbnail_OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (sender is not Control { DataContext: NovelItemViewModel viewModel })
            return;

        _ = await viewModel.TryLoadThumbnailAsync(ViewModel);
    }
}
