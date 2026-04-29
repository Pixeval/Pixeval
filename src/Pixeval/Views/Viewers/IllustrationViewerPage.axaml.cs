// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pixeval.Controls;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class IllustrationViewerPage : ContentPage
{
    private IllustrationViewerPageViewModel ViewModel => (IllustrationViewerPageViewModel) DataContext!;

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
    
    private void ImageViewerPage_OnSelectionChanged(Control sender, ImageViewerSelectionChangedEventArgs e)
    {
        EntryViewerFloatingPaneView.ShowPaneTemporarily();
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
