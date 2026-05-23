// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class SwipeImageViewer : ImageViewerBase
{
    public SwipeImageViewer() => InitializeComponent();

    public static readonly DirectProperty<SwipeImageViewer, SingleImageViewer?> CurrentPageProperty =
        AvaloniaProperty.RegisterDirect<SwipeImageViewer, SingleImageViewer?>(
            nameof(CurrentPage),
            o => o.CurrentPage);

    public static readonly DirectProperty<SwipeImageViewer, double> MirrorScaleXProperty =
        AvaloniaProperty.RegisterDirect<SwipeImageViewer, double>(
            nameof(MirrorScaleX),
            o => o.MirrorScaleX);

    public static readonly DirectProperty<SwipeImageViewer, bool> IsMirroredProperty =
        AvaloniaProperty.RegisterDirect<SwipeImageViewer, bool>(
            nameof(IsMirrored),
            o => o.IsMirrored,
            (o, v) => o.IsMirrored = v);

    public static readonly DirectProperty<SwipeImageViewer, int> RotationDegreeProperty =
        AvaloniaProperty.RegisterDirect<SwipeImageViewer, int>(
            nameof(RotationDegree),
            o => o.RotationDegree,
            (o, v) => o.RotationDegree = v);

    public SingleImageViewer? CurrentPage
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value))
                return;

            var old = field;
            field = value;
            SetCurrentPageViewModel(field?.DataContext as SingleViewerViewModel);
            RaisePropertyChanged(CurrentPageProperty, old, field);
        }
    }

    /// <inheritdoc />
    protected override void OnBrowseDirectionChanged(BrowseDirection oldValue, BrowseDirection newValue)
    {
        UpdatePageTransition();
    }

    private void UpdatePageTransition()
    {
        SwipeContent.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.3), BrowseDirection.ToSlideAxis());
        SwipeContent.IsTransitionDirectionReversed = BrowseDirection is BrowseDirection.RightLeft or BrowseDirection.BottomUp;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePageTransition();
        CurrentPage = SwipeContent.Content as SingleImageViewer;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CurrentPage = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    public override double ZoomFactor
    {
        get;
        set => SetAndRaise(ZoomFactorProperty, ref field, value);
    }

    protected override void NotifyCommandCanExecuteChanged()
    {
        base.NotifyCommandCanExecuteChanged();
        MirrorCommand.NotifyCanExecuteChanged();
        RotateClockwiseCommand.NotifyCanExecuteChanged();
        RotateCounterclockwiseCommand.NotifyCanExecuteChanged();
    }

    private void SwipeContent_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentProperty)
            CurrentPage = SwipeContent.Content as SingleImageViewer;
    }

    private void SelectingMultiPage_OnSelectionChanged(Control sender, ImageViewerSelectionChangedEventArgs e)
    {
        if (sender.DataContext is ImageViewerViewModel viewModel)
        {
            var preloadPageCount = 2;
            var load = viewModel.Images.Skip(e.NewIndex - preloadPageCount).Take((preloadPageCount * 2) + 1);
            foreach (var loadableBitmap in load)
                _ = loadableBitmap.LoadOriginalImageAsync();
        }

        RaiseSelectionChanged(e.NewIndex, e.NewItem);
    }

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private void Mirror()
    {
        // 仅做IsEnabled绑定，实际逻辑修改IsMirrored属性
    }

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private void RotateClockwise() => RotationDegree = (RotationDegree + 90) % 360;

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private void RotateCounterclockwise() => RotationDegree = (RotationDegree - 90 + 360) % 360;

    public bool IsMirrored
    {
        get;
        set
        {
            if (field == value)
                return;

            var mirrorScaleX = MirrorScaleX;
            SetAndRaise(IsMirroredProperty, ref field, value);
            RaisePropertyChanged(MirrorScaleXProperty, mirrorScaleX, MirrorScaleX);
        }
    }

    public int RotationDegree
    {
        get;
        set => SetAndRaise(RotationDegreeProperty, ref field, value);
    }

    /// <summary>
    /// 镜像时为-1，否则为1
    /// </summary>
    public double MirrorScaleX => IsMirrored ? -1 : 1;
}

file static class BrowseDirectionExtensions
{
    public static PageSlide.SlideAxis ToSlideAxis(this BrowseDirection direction) =>
        direction is BrowseDirection.TopDown or BrowseDirection.BottomUp
            ? PageSlide.SlideAxis.Vertical
            : PageSlide.SlideAxis.Horizontal;
}
