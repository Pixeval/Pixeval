// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pixeval.Controls;
using Pixeval.Models.Options;
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

    private void PrevButton_OnRightClick(object? sender, ContextRequestedEventArgs e) => ViewModel.PrevWorkCommand.Execute(null);

    private void NextButton_OnRightClick(object? sender, ContextRequestedEventArgs e) => ViewModel.NextWorkCommand.Execute(null);

    private void AutoPlayMenuFlyout_OnOpened(object? sender, EventArgs e)
    {
        SetMenuItem(AutoPlayInterval1SecondMenuItem, ViewModel.AutoPlayInterval);
        SetMenuItem(AutoPlayInterval3SecondsMenuItem, ViewModel.AutoPlayInterval);
        SetMenuItem(AutoPlayInterval5SecondsMenuItem, ViewModel.AutoPlayInterval);
        SetMenuItem(AutoPlayInterval10SecondsMenuItem, ViewModel.AutoPlayInterval);
        SetMenuItem(AutoPlayInterval15SecondsMenuItem, ViewModel.AutoPlayInterval);
        SetMenuItem(AutoPlayInterval30SecondsMenuItem, ViewModel.AutoPlayInterval);

        SetMenuItem(AutoPlaySequentialModeMenuItem, ViewModel.AutoPlayMode);
        SetMenuItem(AutoPlayLoopPlaybackModeMenuItem, ViewModel.AutoPlayMode);
        SetMenuItem(AutoPlayCurrentWorkScopeMenuItem, ViewModel.AutoPlayScope);
        SetMenuItem(AutoPlayAllWorksScopeMenuItem, ViewModel.AutoPlayScope);
        return;

        static void SetMenuItem(MenuItem menuItem, object value) => menuItem.IsChecked = Equals(value, menuItem.Tag);
    }

    private void AutoPlayIntervalMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: int interval })
            ViewModel.AutoPlayInterval = interval;
    }

    private void AutoPlayModeMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: IllustrationViewerAutoPlayMode mode })
            ViewModel.AutoPlayMode = mode;
    }

    private void AutoPlayScopeMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: IllustrationViewerAutoPlayScope scope })
            ViewModel.AutoPlayScope = scope;
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
