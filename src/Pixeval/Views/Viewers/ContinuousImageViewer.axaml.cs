// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Pixeval.Models.Options;
using Pixeval.ViewModels.Viewers;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Views.Viewers;

public partial class ContinuousImageViewer : ImageViewerBase
{
    public static readonly DirectProperty<ContinuousImageViewer, SingleViewerViewModel?> CurrentPageProperty =
        AvaloniaProperty.RegisterDirect<ContinuousImageViewer, SingleViewerViewModel?>(
            nameof(CurrentPage),
            o => o.CurrentPage);

    public static readonly DirectProperty<ContinuousImageViewer, Orientation> StackOrientationProperty =
        AvaloniaProperty.RegisterDirect<ContinuousImageViewer, Orientation>(
            nameof(StackOrientation),
            o => o.StackOrientation);

    /// <inheritdoc />
    public override double ZoomFactor
    {
        get => ViewerScrollView?.ZoomFactor ?? 1;
        set
        {
            if (ZoomFactor == value)
                return;
            ViewerScrollView?.ZoomTo(value);
            QueueViewportUpdate();
        }
    }

    private ImageViewerViewModel? _subscribedViewModel;
    private bool _isSelectingFromScroll;
    private bool _viewportUpdateQueued;
    private bool _scrollToSelectedPageQueued;
    private int _preloadedStartIndex = -1;
    private int _preloadedEndIndex = -1;

    public ContinuousImageViewer()
    {
        InitializeComponent();
        ViewerScrollView.ScrollChanged += (_, _) => QueueViewportUpdate();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateViewModelSubscription();
        QueueScrollToSelectedPage();
        QueueViewportUpdate();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromViewModel();
        SetCurrentPage(null, false);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateViewModelSubscription();
        _preloadedStartIndex = _preloadedEndIndex = -1;
        SetCurrentPage(GetSelectedPage());
        QueueScrollToSelectedPage();
        QueueViewportUpdate();
    }

    private ImageViewerViewModel? ViewModel => DataContext as ImageViewerViewModel;

    /// <inheritdoc />
    protected override void OnBrowseDirectionChanged(BrowseDirection oldValue, BrowseDirection newValue)
    {
        RaisePropertyChanged(StackOrientationProperty,
            oldValue is BrowseDirection.LeftRight or BrowseDirection.RightLeft
                ? Orientation.Horizontal
                : Orientation.Vertical, StackOrientation);
        // TODO: StackPanel does not support RightLeft/BottomUp ordering here yet.
        QueueScrollToSelectedPage();
        QueueViewportUpdate();
    }

    public Orientation StackOrientation =>
        IsHorizontal ? Orientation.Horizontal : Orientation.Vertical;

    public ScrollContentOrientation ContentOrientation =>
        IsHorizontal ? ScrollContentOrientation.Horizontal : ScrollContentOrientation.Vertical;

    private bool IsHorizontal =>
        BrowseDirection is BrowseDirection.LeftRight or BrowseDirection.RightLeft;

    private void UpdateViewModelSubscription()
    {
        var viewModel = ViewModel;
        if (ReferenceEquals(_subscribedViewModel, viewModel))
            return;

        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged -= ViewModelOnPropertyChanged;

        _subscribedViewModel = viewModel;
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void UnsubscribeFromViewModel()
    {
        if (_subscribedViewModel is null)
            return;

        _subscribedViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        _subscribedViewModel = null;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(ImageViewerViewModel.SelectedPageIndex))
            return;

        SetCurrentPage(GetSelectedPage());
        if (!_isSelectingFromScroll)
            QueueScrollToSelectedPage();
        QueueViewportUpdate();
    }

    private SingleViewerViewModel? GetSelectedPage()
    {
        if (ViewModel is not { Images.Count: > 0 } viewModel)
            return null;

        var index = Math.Clamp(viewModel.SelectedPageIndex, 0, viewModel.Images.Count - 1);
        return viewModel.Images[index];
    }

    private void SetSelectedPageIndex(int index)
    {
        if (ViewModel is not { Images.Count: > 0 } viewModel)
            return;

        index = Math.Clamp(index, 0, viewModel.Images.Count - 1);
        if (viewModel.SelectedPageIndex == index)
        {
            SetCurrentPage(viewModel.Images[index]);
            return;
        }

        _isSelectingFromScroll = true;
        try
        {
            viewModel.SelectedPageIndex = index;
        }
        finally
        {
            _isSelectingFromScroll = false;
        }
    }

    private void SetCurrentPage(SingleViewerViewModel? page, bool raiseSelectionChanged = true)
    {
        if (ReferenceEquals(CurrentPage, page))
            return;

        var old = CurrentPage;
        CurrentPage = page;
        SetCurrentPageViewModel(page);

        RaisePropertyChanged(CurrentPageProperty, old, page);
        if (raiseSelectionChanged)
            RaiseSelectionChanged(ViewModel?.SelectedPageIndex ?? -1, CurrentPage);
    }

    private void QueueScrollToSelectedPage()
    {
        if (_scrollToSelectedPageQueued)
            return;

        _scrollToSelectedPageQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _scrollToSelectedPageQueued = false;
            ScrollToSelectedPage();
        }, DispatcherPriority.Loaded);
    }

    private void ScrollToSelectedPage()
    {
        if (ViewModel is not { Images.Count: > 0 } viewModel)
            return;

        var index = Math.Clamp(viewModel.SelectedPageIndex, 0, viewModel.Images.Count - 1);
        if (ImageItemsControl.ContainerFromIndex(index) is not { } container)
            return;

        if (container.TranslatePoint(default, ImageItemsControl) is not { } origin)
        {
            container.BringIntoView();
            return;
        }

        var offset = IsHorizontal
            ? Math.Clamp(origin.X * ZoomFactor, 0, ViewerScrollView.ScrollBarMaximum.X)
            : Math.Clamp(origin.Y * ZoomFactor, 0, ViewerScrollView.ScrollBarMaximum.Y);
        ViewerScrollView.Offset = IsHorizontal
            ? new(offset, ViewerScrollView.Offset.Y)
            : new(ViewerScrollView.Offset.X, offset);
    }

    private void QueueViewportUpdate()
    {
        if (_viewportUpdateQueued)
            return;

        _viewportUpdateQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _viewportUpdateQueued = false;
            UpdateSelectedPageFromViewport();
            PreloadPagesAroundViewport();
        }, DispatcherPriority.Background);
    }

    private void UpdateSelectedPageFromViewport()
    {
        if (ViewModel is not { Images.Count: > 0 } viewModel
            || ViewportLength <= 0)
            return;

        var bestIndex = -1;
        var bestVisibleHeight = 0d;
        var nearestIndex = -1;
        var nearestDistance = double.MaxValue;
        var viewportLength = ViewportLength;
        var viewportCenter = viewportLength / 2;

        for (var i = 0; i < viewModel.Images.Count; i++)
        {
            if (GetPageBoundsInViewport(i) is not { } bounds)
                continue;

            var pageStart = GetStart(bounds);
            var pageEnd = GetEnd(bounds);
            var visibleStart = Math.Max(pageStart, 0);
            var visibleEnd = Math.Min(pageEnd, viewportLength);
            var visibleLength = Math.Max(0, visibleEnd - visibleStart);
            if (visibleLength > bestVisibleHeight)
            {
                bestVisibleHeight = visibleLength;
                bestIndex = i;
            }

            var distance = Math.Abs(((pageStart + pageEnd) / 2) - viewportCenter);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        var selectedIndex = bestIndex >= 0 ? bestIndex : nearestIndex;
        if (selectedIndex >= 0)
            SetSelectedPageIndex(selectedIndex);
    }

    private void PreloadPagesAroundViewport()
    {
        if (ViewModel is not { Images.Count: > 0 } viewModel
            || ViewportLength <= 0)
            return;

        var startIndex = viewModel.Images.Count;
        var endIndex = -1;
        var viewportLength = ViewportLength;

        for (var i = 0; i < viewModel.Images.Count; i++)
        {
            if (GetPageBoundsInViewport(i) is not { } bounds)
                continue;

            if (GetEnd(bounds) < -viewportLength || GetStart(bounds) > viewportLength * 2)
                continue;

            startIndex = Math.Min(startIndex, i);
            endIndex = Math.Max(endIndex, i);
        }

        if (endIndex < 0)
        {
            startIndex = Math.Max(0, viewModel.SelectedPageIndex - 1);
            endIndex = Math.Min(viewModel.Images.Count - 1, viewModel.SelectedPageIndex + 1);
        }

        if (startIndex == _preloadedStartIndex && endIndex == _preloadedEndIndex)
            return;

        _preloadedStartIndex = startIndex;
        _preloadedEndIndex = endIndex;

        for (var i = startIndex; i <= endIndex; i++)
            _ = viewModel.Images[i].LoadOriginalImageAsync();
    }

    private Rect? GetPageBoundsInViewport(int index)
    {
        if (ImageItemsControl.ContainerFromIndex(index) is not { } container
            || container.TranslatePoint(default, ViewerScrollView) is not { } origin)
            return null;

        return new(origin, new Size(container.Bounds.Width * ZoomFactor, container.Bounds.Height * ZoomFactor));
    }

    private double ViewportLength => IsHorizontal ? ViewerScrollView.Viewport.Width : ViewerScrollView.Viewport.Height;

    private double GetStart(Rect bounds) => IsHorizontal ? bounds.Left : bounds.Top;

    private double GetEnd(Rect bounds) => IsHorizontal ? bounds.Right : bounds.Bottom;

    private void ViewerScrollView_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        QueueViewportUpdate();
    }

    private void ImageItem_OnSizeChanged(object? sender, SizeChangedEventArgs e) => QueueViewportUpdate();

    private void ImageItem_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: SingleViewerViewModel page }
            || ViewModel is not { } viewModel)
            return;

        var index = IndexOfPage(viewModel, page);
        if (index >= 0)
            SetSelectedPageIndex(index);
    }

    private static int IndexOfPage(ImageViewerViewModel viewModel, SingleViewerViewModel page)
    {
        for (var i = 0; i < viewModel.Images.Count; i++)
            if (ReferenceEquals(viewModel.Images[i], page))
                return i;

        return -1;
    }

    private void ViewerScrollView_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollView.ZoomFactorProperty)
            RaisePropertyChanged(ZoomFactorProperty, e.GetOldValue<double>(), e.GetNewValue<double>());
    }

    public SingleViewerViewModel? CurrentPage { get; private set; }
}
