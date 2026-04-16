// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

public partial class IllustrationViewerPage : ContentPage
{
    private IllustrationViewerPageViewModel ViewModel => (IllustrationViewerPageViewModel) DataContext!;

    public static readonly FuncValueConverter<int, string> PlusOneConverter = new(i => (i + 1).ToString());

    public IllustrationViewerPage() : this(null)
    {
    }

    public IllustrationViewerPage(IllustrationViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        if (viewModel?.CurrentIllustration.Entry.Platform is { } platform)
        {
            using var stream = AssetLoader.Open(new Uri($"avares://Pixeval/Assets/Platforms/{platform}.png"));
            LogoImage.Source = new Bitmap(stream);
        }
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

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, ViewModel));
    }

    #endregion
}
