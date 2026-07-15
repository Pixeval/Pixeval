// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
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
    protected override void OnBrowseDirectionChanged(Orientation oldValue, Orientation newValue)
    {
        UpdatePageTransition();
    }

    private void UpdatePageTransition()
    {
        SwipeContent.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.3), BrowseDirection is Orientation.Horizontal ? PageSlide.SlideAxis.Horizontal : PageSlide.SlideAxis.Vertical);
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
}
