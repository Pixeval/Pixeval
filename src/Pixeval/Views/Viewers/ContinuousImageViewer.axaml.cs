// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    public SingleViewerViewModel? CurrentPage { get; private set; }

    private ImageViewerViewModel? _subscribedViewModel;
    private bool _isSelectingFromScroll;
    private bool _viewportUpdateQueued;
    private bool _scrollToSelectedPageQueued;
    private int _preloadedStartIndex = -1;
    private int _preloadedEndIndex = -1;

    public ContinuousImageViewer()
    {
        InitializeComponent();
    }

    private void ViewerScrollView_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        QueueViewportUpdate();
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
        QueueScrollToSelectedPage();
        QueueViewportUpdate();
    }

    public ScrollContentOrientation ContentOrientation =>
        IsHorizontal ? ScrollContentOrientation.Horizontal : ScrollContentOrientation.Vertical;

    private bool IsHorizontal =>
        BrowseDirection is BrowseDirection.LeftRight or BrowseDirection.RightLeft;

    private bool IsReversed =>
        BrowseDirection is BrowseDirection.RightLeft or BrowseDirection.BottomUp;

    private void UpdateViewModelSubscription()
    {
        var viewModel = ViewModel;
        if (ReferenceEquals(_subscribedViewModel, viewModel))
            return;

        _subscribedViewModel?.PropertyChanged -= ViewModelOnPropertyChanged;

        _subscribedViewModel = viewModel;
        _subscribedViewModel?.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void UnsubscribeFromViewModel()
    {
        _subscribedViewModel?.PropertyChanged -= ViewModelOnPropertyChanged;
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

        var index = int.Clamp(viewModel.SelectedPageIndex, 0, viewModel.Images.Count - 1);
        return viewModel.Images[index];
    }

    private void SetSelectedPageIndex(int index)
    {
        if (ViewModel is not { Images.Count: > 0 } viewModel)
            return;

        index = int.Clamp(index, 0, viewModel.Images.Count - 1);
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

    /// <summary>
    /// 滚动到指定页码
    /// </summary>
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
        return;

        void ScrollToSelectedPage()
        {
            if (ViewModel is not { Images.Count: > 0 } viewModel)
                return;

            var index = int.Clamp(viewModel.SelectedPageIndex, 0, viewModel.Images.Count - 1);
            if (ImageItemsControl.ContainerFromIndex(index) is not { } container)
                return;

            if (GetPageOriginInContent(container) is not { } origin)
            {
                container.BringIntoView();
                return;
            }

            var offset = double.Clamp(
                GetScrollOffset(origin, container.Bounds.Size),
                0,
                IsHorizontal ? ViewerScrollView.ScrollBarMaximum.X : ViewerScrollView.ScrollBarMaximum.Y);
            ViewerScrollView.Offset = IsHorizontal
                ? new(offset, ViewerScrollView.Offset.Y)
                : new(ViewerScrollView.Offset.X, offset);
        }
    }

    /// <summary>
    /// 用于更新页码和加载页码周围的图片
    /// </summary>
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
        return;

        void UpdateSelectedPageFromViewport()
        {
            if (ViewModel is not { Images.Count: > 0 } viewModel
                || ViewportLength <= 0)
                return;

            var bestIndex = -1;
            var bestAnchorDistance = double.MaxValue;
            var nearestIndex = -1;
            var nearestAnchorDistance = double.MaxValue;
            var viewportLength = ViewportLength;
            var viewportAnchor = ViewportAnchor;

            for (var i = 0; i < viewModel.Images.Count; i++)
            {
                if (GetPageBoundsInViewport(i) is not { } bounds)
                    continue;

                var pageStart = GetStart(bounds);
                var pageEnd = GetEnd(bounds);
                var visibleStart = double.Max(pageStart, 0);
                var visibleEnd = double.Min(pageEnd, viewportLength);
                var visibleLength = double.Max(0, visibleEnd - visibleStart);
                var anchorDistance = double.Abs(GetPageAnchor(bounds) - viewportAnchor);

                if (visibleLength > 0 && anchorDistance < bestAnchorDistance)
                {
                    bestAnchorDistance = anchorDistance;
                    bestIndex = i;
                }

                if (anchorDistance < nearestAnchorDistance)
                {
                    nearestAnchorDistance = anchorDistance;
                    nearestIndex = i;
                }
            }

            var selectedIndex = bestIndex >= 0 ? bestIndex : nearestIndex;
            if (selectedIndex >= 0)
                SetSelectedPageIndex(selectedIndex);
        }

        void PreloadPagesAroundViewport()
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

                startIndex = int.Min(startIndex, i);
                endIndex = int.Max(endIndex, i);
            }

            if (endIndex < 0)
            {
                startIndex = int.Max(0, viewModel.SelectedPageIndex - 1);
                endIndex = int.Min(viewModel.Images.Count - 1, viewModel.SelectedPageIndex + 1);
            }

            if (startIndex == _preloadedStartIndex && endIndex == _preloadedEndIndex)
                return;

            _preloadedStartIndex = startIndex;
            _preloadedEndIndex = endIndex;

            for (var i = startIndex; i <= endIndex; i++)
                _ = viewModel.Images[i].LoadOriginalImageAsync();
        }
    }

    private Rect? GetPageBoundsInViewport(int index)
    {
        if (ImageItemsControl.ContainerFromIndex(index) is not { } container
            || GetPageOriginInContent(container) is not { } origin)
            return null;

        return new Rect(
            (origin * ZoomFactor) - ViewerScrollView.Offset,
            container.Bounds.Size * ZoomFactor);
    }

    private Point? GetPageOriginInContent(Control container) => container.TranslatePoint(default, ImageItemsControl);

    private double GetScrollOffset(Point origin, Size size)
    {
        var anchor = IsHorizontal
            ? IsReversed ? origin.X + size.Width : origin.X
            : IsReversed ? origin.Y + size.Height : origin.Y;
        return (anchor * ZoomFactor) - ViewportAnchor;
    }

    private double ViewportLength => IsHorizontal ? ViewerScrollView.Viewport.Width : ViewerScrollView.Viewport.Height;

    private double ViewportAnchor => IsReversed ? ViewportLength : 0;

    private double GetStart(Rect bounds) => IsHorizontal ? bounds.Left : bounds.Top;

    private double GetEnd(Rect bounds) => IsHorizontal ? bounds.Right : bounds.Bottom;

    private double GetPageAnchor(Rect bounds) => IsReversed ? GetEnd(bounds) : GetStart(bounds);
    
    private void ViewerScrollView_OnSizeChanged(object? sender, SizeChangedEventArgs e) => QueueViewportUpdate();

    private void ImageItem_OnSizeChanged(object? sender, SizeChangedEventArgs e) => QueueViewportUpdate();

    private void ImageItem_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { DataContext: SingleViewerViewModel { Index: >= 0 and var index } })
            SetSelectedPageIndex(index);
    }

    private void ViewerScrollView_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollView.ZoomFactorProperty)
        {
            RaisePropertyChanged(ZoomFactorProperty, e.GetOldValue<double>(), e.GetNewValue<double>());
            QueueViewportUpdate();
        }
    }

    private void ImageItemsControl_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e) => ViewerScrollView.RegisterAnchorCandidate(e.Container);

    private void ImageItemsControl_OnContainerClearing(object? sender, ContainerClearingEventArgs e) => ViewerScrollView.UnregisterAnchorCandidate(e.Container);

    private async void SaveButton_OnRightClick(object? sender, ContextRequestedEventArgs e)
    {
        await SaveAsCommand.ExecuteAsync(null);
    }
}
