// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

public partial class IllustrationViewerPage : ContentPage
{
    private IllustrationViewerPageViewModel ViewModel => (IllustrationViewerPageViewModel) DataContext!;

    private ImageViewerBase? _imageViewerPage;
    private IllustrationViewerPageViewModel? _subscribedViewModel;
    private SwipeImageViewer? _swipeImageViewer;

    public IllustrationViewerPage() : this(null)
    {
    }

    public IllustrationViewerPage(IllustrationViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        InitializeImageViewer();
        if (viewModel?.CurrentIllustration.Entry.Platform is { } platform)
        {
            using var stream = AssetLoader.Open(new Uri($"avares://Pixeval/Assets/Platforms/{platform}.png"));
            LogoImage.Source = new Bitmap(stream);
        }
    }

    private void InitializeImageViewer()
    {
        var settings = App.AppViewModel.AppSettings;
        _imageViewerPage = settings.BrowseMode switch
        {
            BrowseMode.Continuous => new ContinuousImageViewer { BrowseDirection = settings.BrowseDirection },
            _ => new SwipeImageViewer { BrowseDirection = settings.BrowseDirection }
        };
        _imageViewerPage.Name = "ImageViewerPage";

        if (DataContext is IllustrationViewerPageViewModel viewModel)
            InitializeImageViewerDataContext(viewModel);

        _imageViewerPage.SelectionChanged += ImageViewerPage_OnSelectionChanged;
        ImageViewerPageHost.Content = _imageViewerPage;
        ConfigureImageViewerCommands(_imageViewerPage);
    }

    private void ConfigureImageViewerCommands(ImageViewerBase imageViewer)
    {
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.Space), Command = imageViewer.PlayPauseCommand });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.C, KeyModifiers.Control), Command = imageViewer.CopyCommand });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.S, KeyModifiers.Control), Command = imageViewer.SaveCommand });

        ZoomOutButton.Command = imageViewer.ZoomOutCommand;
        ZoomInButton.Command = imageViewer.ZoomInCommand;
        imageViewer.PropertyChanged += ImageViewerPage_OnPropertyChanged;
        UpdateZoomFactorText();

        if (imageViewer is SwipeImageViewer swipeImageViewer)
        {
            _swipeImageViewer = swipeImageViewer;
            RotateCounterclockwiseButton.Command = swipeImageViewer.RotateCounterclockwiseCommand;
            MirrorButton.Command = swipeImageViewer.MirrorCommand;
            RotateClockwiseButton.Command = swipeImageViewer.RotateClockwiseCommand;
            MirrorButton.PropertyChanged += MirrorButton_OnPropertyChanged;
            swipeImageViewer.PropertyChanged += SwipeImageViewer_OnPropertyChanged;
            UpdateMirrorButton();
            return;
        }

        RotateCounterclockwiseButton.IsVisible = false;
        MirrorButton.IsVisible = false;
        RotateClockwiseButton.IsVisible = false;
        SwipeTransformSeparator.IsVisible = false;
    }

    private void InitializeImageViewerDataContext(IllustrationViewerPageViewModel viewModel)
    {
        _subscribedViewModel = viewModel;
        _subscribedViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        if (_imageViewerPage is not null)
            _imageViewerPage.DataContext = viewModel.CurrentImage;
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IllustrationViewerPageViewModel.CurrentImage) && _imageViewerPage is not null)
            _imageViewerPage.DataContext = ViewModel.CurrentImage;
    }

    private void ImageViewerPage_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ImageViewerBase.ZoomFactorProperty)
            UpdateZoomFactorText();
    }

    private void SwipeImageViewer_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == SwipeImageViewer.IsMirroredProperty)
            UpdateMirrorButton();
    }

    private void MirrorButton_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ToggleButton.IsCheckedProperty && _swipeImageViewer is not null)
            _swipeImageViewer.IsMirrored = MirrorButton.IsChecked == true;
    }

    private void UpdateZoomFactorText()
    {
        ZoomFactorTextBlock.Text = (_imageViewerPage?.ZoomFactor ?? 1).ToString("P0", CultureInfo.CurrentCulture);
    }

    private void UpdateMirrorButton()
    {
        if (_swipeImageViewer is null || MirrorButton.IsChecked == _swipeImageViewer.IsMirrored)
            return;

        MirrorButton.IsChecked = _swipeImageViewer.IsMirrored;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        if (_imageViewerPage is not null)
        {
            _imageViewerPage.SelectionChanged -= ImageViewerPage_OnSelectionChanged;
            _imageViewerPage.PropertyChanged -= ImageViewerPage_OnPropertyChanged;
        }
        if (_swipeImageViewer is not null)
        {
            _swipeImageViewer.PropertyChanged -= SwipeImageViewer_OnPropertyChanged;
            MirrorButton.PropertyChanged -= MirrorButton_OnPropertyChanged;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void PrevButton_OnRightClick(object? sender, TappedEventArgs e)
    {
        ViewModel.CurrentWorkIndex--;
    }

    private void NextButton_OnRightClick(object? sender, TappedEventArgs e)
    {
        ViewModel.CurrentWorkIndex++;
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

    private void ImageViewerPage_OnSelectionChanged(Control sender, ImageViewerSelectionChangedEventArgs e)
    {
        if (e.NewIndex >= 0)
            ViewModel.CurrentPageIndex = e.NewIndex;
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
