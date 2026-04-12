// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

public partial class IllustrationViewerPage : UserControl
{
    private IllustrationViewerPageViewModel ViewModel => (IllustrationViewerPageViewModel) DataContext!;

    public static readonly FuncValueConverter<int, string> PlusOneConverter = new(i => (i + 1).ToString());

    public IllustrationViewerPage()
    {
        InitializeComponent();

        AddHandler(Frame.NavigatedToEvent, (_, e) =>
        {
            if (e.Parameter is IllustrationViewerPageViewModel viewModel)
                DataContext = viewModel;
        });
    }

    private void PrevButton_OnClick(object? sender, RoutedEventArgs e)
    {
        switch (ViewModel.PrevButtonAction)
        {
            case true:
                ViewModel.CurrentPageIndex--;
                break;
            case false:
                ViewModel.CurrentIllustrationIndex--;
                break;
        }
    }

    private void NextButton_OnClick(object? sender, RoutedEventArgs e)
    {
        switch (ViewModel.NextButtonAction)
        {
            case true:
                ViewModel.CurrentPageIndex++;
                break;
            case false:
                ViewModel.CurrentIllustrationIndex++;
                break;
        }
    }

    private async void AddToBookmarkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!BookmarkTagSelector.IsVisible)
            await BookmarkTagSelector.ResetSourceAsync();
        BookmarkTagSelector.IsVisible = !BookmarkTagSelector.IsVisible;
    }

    private void BookmarkTagSelector_OnTagsSelected(TagSelector sender, (bool IsPrivate, IReadOnlyList<string> Tags) e)
    {
        if (ViewModel.CurrentIllustration.AddToBookmarkCommand is { } command)
        {
            command.Execute((e.Tags, e.IsPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(EntryViewerPageResources.AddedToBookmark));
        }

        BookmarkTagSelector.IsVisible = false;
    }

    private void ZoomInButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ImageViewerPage.ZoomBorder.ZoomBy(0.1);
    }

    private void ZoomOutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ImageViewerPage.ZoomBorder.ZoomBy(-0.1);
    }
}
