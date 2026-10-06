// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using AnimatedControls.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Pixeval.ViewModels.Viewers;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Views.Viewers;

/// <summary>
/// <see cref="SwipeImageViewer"/> 内部使用
/// </summary>
public partial class SingleImageViewer : UserControl
{
    private const string ScrollableImageTemplateKey = "ScrollableImageTemplate";
    private const string PlainImageTemplateKey = "PlainImageTemplate";

    public static readonly StyledProperty<bool> UseScrollViewProperty =
        AvaloniaProperty.Register<SingleImageViewer, bool>(nameof(UseScrollView), defaultValue: true);

    private SingleViewerViewModel? _subscribedViewModel;
    private bool InitialFitApplied
    {
        get;
        set
        {
            field = value;
            // The compositor applies zoom asynchronously. Do not flash the initial 100% frame.
            ImageViewer?.Opacity = value || !UseScrollView ? 1 : 0;
        }
    }
    private bool _viewUpdateQueued;
    private bool _updatingZoomFactor;
    private int? _zoomOperation;
    private PendingView? _pendingView;
    internal AnimatedImage? ImageViewer;
    internal ScrollView? ViewerScrollView;

    public SingleImageViewer()
    {
        InitializeComponent();
        UpdateViewMode();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateViewModelSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromViewModel();
        // Keep the target across virtualization; the presenter interrupts its active operation on detach.
        _zoomOperation = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        InitialFitApplied = false;
        _pendingView = null;
        UpdateViewModelSubscription();
        NotifyCommandCanExecuteChanged();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseScrollViewProperty)
            UpdateViewMode();
    }

    private void UpdateViewModelSubscription()
    {
        var viewModel = DataContext as SingleViewerViewModel;
        if (ReferenceEquals(_subscribedViewModel, viewModel))
            return;

        UnsubscribeFromViewModel();
        if (VisualRoot is null || viewModel is null)
            return;

        _subscribedViewModel = viewModel;
        viewModel.AttachPreview();
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        QueueViewUpdate();
    }

    private void UnsubscribeFromViewModel()
    {
        _subscribedViewModel?.PropertyChanged -= ViewModelOnPropertyChanged;
        _subscribedViewModel?.DetachPreview();
        _subscribedViewModel = null;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SingleViewerViewModel.ZoomFactor) || _updatingZoomFactor
            || sender is not SingleViewerViewModel viewModel || ViewerScrollView is not { } scrollView)
            return;
        _pendingView = null;
        InitialFitApplied = true;
        _ = scrollView.ZoomTo(viewModel.ZoomFactor);
    }

    private void NotifyCommandCanExecuteChanged() => ZoomToFitCommand.NotifyCanExecuteChanged();

    private bool CanManipulateImage => ImageViewer?.Source is { IsInitialized: true };

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void ZoomToFit()
    {
        if (ImageViewer?.Source is { Size: { Width: > 0, Height: > 0 } size }
            && ViewerScrollView is { Viewport: { Width: > 0, Height: > 0 } viewport })
            _pendingView = new(size, new(0.5, 0.5), GetPixelScale(viewport, size), true);
        QueueViewUpdate();
    }

    private void ImageViewerOnSourceSizeChanged(object? sender, SourceSizeChangedEventArgs e)
    {
        NotifyCommandCanExecuteChanged();
        if ((InitialFitApplied || _pendingView is not null) && ViewerScrollView is { } scrollView
            && e.OldSize is { Width: > 0, Height: > 0 }
            && e.NewSize is { Width: > 0, Height: > 0 })
        {
            // Capture the viewport center before layout changes; restore it after the new extent is known.
            _pendingView = _pendingView is { } pending
                ? pending.Resize(e.NewSize)
                : new(e.NewSize,
                    GetViewportAnchor(e.OldSize, scrollView.Viewport, scrollView.Offset, scrollView.ZoomFactor),
                    GetReplacementZoomFactor(scrollView.ZoomFactor, e.OldSize, e.NewSize));
        }
        else
        {
            if (e.NewSize is not { Width: > 0, Height: > 0 })
            {
                _pendingView = null;
                InitialFitApplied = false;
            }
        }

        QueueViewUpdate();
    }

    private void ImagePresenter_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
            switch (e.GetNewValue<Control?>())
            {
                case null:
                    SetViewerControls(null, null);
                    break;
                case ScrollView { Content: AnimatedImage image } scrollView:
                    SetViewerControls(image, scrollView);
                    break;
                case AnimatedImage image:
                    SetViewerControls(image, null);
                    break;
            }
    }

    private void UpdateViewMode()
    {
        var templateKey = UseScrollView ? ScrollableImageTemplateKey : PlainImageTemplateKey;
        if (!this.TryFindResource(templateKey, out var resource) || resource is not IDataTemplate template)
            throw new InvalidOperationException($"Resource '{templateKey}' must be an {nameof(IDataTemplate)}.");

        InitialFitApplied = false;
        _pendingView = null;
        // Keep direct content so a logical-tree reattach cannot rebuild the stateful viewer.
        ImagePresenter.Content = template.Build(DataContext);
        QueueViewUpdate();
    }

    internal static double GetPixelScale(Size bounds, Size imageSize) =>
        imageSize is { Width: > 0, Height: > 0 } && bounds is { Width: > 0, Height: > 0 }
            ? double.Min(bounds.Width / imageSize.Width, bounds.Height / imageSize.Height) : 1;

    internal static double GetReplacementZoomFactor(double zoomFactor, Size oldSize, Size newSize) =>
        zoomFactor * GetPixelScale(oldSize, newSize);

    internal static Point GetViewportAnchor(Size imageSize, Size viewport, Vector offset, double zoomFactor)
    {
        var rendered = imageSize * zoomFactor;
        return new(
            (offset.X + (viewport.Width / 2) - double.Max(0, (viewport.Width - rendered.Width) / 2)) / rendered.Width,
            (offset.Y + (viewport.Height / 2) - double.Max(0, (viewport.Height - rendered.Height) / 2)) / rendered.Height);
    }

    internal static Vector GetAnchorOffset(Point anchor, Size imageSize, Size viewport, double zoomFactor)
    {
        var rendered = imageSize * zoomFactor;
        return new(
            (anchor.X * rendered.Width) - (viewport.Width / 2) + double.Max(0, (viewport.Width - rendered.Width) / 2),
            (anchor.Y * rendered.Height) - (viewport.Height / 2) + double.Max(0, (viewport.Height - rendered.Height) / 2));
    }

    private void SetViewerControls(AnimatedImage? imageViewer, ScrollView? scrollView)
    {
        if (ReferenceEquals(ImageViewer, imageViewer) && ReferenceEquals(ViewerScrollView, scrollView))
            return;
        ImageViewer?.SourceSizeChanged -= ImageViewerOnSourceSizeChanged;
        if (ViewerScrollView is { } previous)
        {
            previous.PropertyChanged -= ViewerScrollViewOnPropertyChanged;
            previous.LayoutUpdated -= ViewerScrollViewOnLayoutUpdated;
            previous.StateChanged -= ViewerScrollViewOnLayoutUpdated;
            previous.ZoomStarting -= ViewerScrollViewOnZoomStarting;
            previous.ZoomCompleted -= ViewerScrollViewOnZoomCompleted;
        }

        ImageViewer = imageViewer;
        ImageViewer?.Opacity = InitialFitApplied || !UseScrollView ? 1 : 0;
        ViewerScrollView = scrollView;
        ImageViewer?.SourceSizeChanged += ImageViewerOnSourceSizeChanged;
        if (scrollView is not null)
        {
            scrollView.PropertyChanged += ViewerScrollViewOnPropertyChanged;
            scrollView.LayoutUpdated += ViewerScrollViewOnLayoutUpdated;
            scrollView.StateChanged += ViewerScrollViewOnLayoutUpdated;
            scrollView.ZoomStarting += ViewerScrollViewOnZoomStarting;
            scrollView.ZoomCompleted += ViewerScrollViewOnZoomCompleted;
            scrollView.GestureBindings = ImageViewerScrollGestureProfiles.Paging;
        }

        _zoomOperation = null;
        NotifyCommandCanExecuteChanged();
        QueueViewUpdate();
    }

    private void ViewerScrollViewOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollView.ZoomFactorProperty && _pendingView is null && InitialFitApplied)
            PublishZoomFactor();
    }

    private void ViewerScrollViewOnZoomStarting(object? sender, ScrollingZoomStartingEventArgs e)
    {
        _zoomOperation = e.CorrelationId;
    }

    private void ViewerScrollViewOnZoomCompleted(object? sender, ScrollingZoomCompletedEventArgs e)
    {
        if (_zoomOperation != e.CorrelationId)
            return;
        _zoomOperation = null;
        // Completed, ignored, and interrupted requests all release the pending target for re-evaluation.
        QueueViewUpdate();
    }

    private void ViewerScrollViewOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (ViewerScrollView is { State: ScrollingInteractionState.Interaction })
        {
            _pendingView = null;
            InitialFitApplied = true;
        }
        QueueViewUpdate();
    }

    private void UpdateView()
    {
        if (VisualRoot is null || !UseScrollView
            || ViewerScrollView is not { Viewport: { Width: > 0, Height: > 0 } viewport } scrollView
            || scrollView.State is ScrollingInteractionState.Interaction
            || ImageViewer is not
            {
                IsMeasureValid: true, IsArrangeValid: true,
                Source: { IsInitialized: true, Size: { Width: > 0, Height: > 0 } size }
            })
            return;

        if (!InitialFitApplied && _pendingView is null)
            _pendingView = new(size, new(0.5, 0.5), GetPixelScale(viewport, size));
        if (_pendingView is not { } pending || pending.Size != size || _zoomOperation is not null)
            return;

        var zoom = ConstrainZoomFactor(scrollView, pending.ZoomFactor);
        if (Math.Abs(scrollView.ZoomFactor - zoom) > 0.000001)
        {
            _ = scrollView.ZoomTo(zoom, pending.Animated);
            return;
        }
        _pendingView = null;
        _ = scrollView.ScrollTo(GetAnchorOffset(pending.Anchor, size, viewport, zoom), false);
        InitialFitApplied = true;
        PublishZoomFactor();
    }

    private void QueueViewUpdate()
    {
        if (!UseScrollView || _viewUpdateQueued || (InitialFitApplied && _pendingView is null))
            return;
        _viewUpdateQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _viewUpdateQueued = false;
            // Templates are built before their DataContext is assigned. Fit the current page, not a captured one.
            // Presenter metric notifications occur inside an update transaction. Change zoom only after it finishes.
            UpdateView();
        }, DispatcherPriority.Render);
    }

    public bool UseScrollView
    {
        get => GetValue(UseScrollViewProperty);
        set => SetValue(UseScrollViewProperty, value);
    }

    private void PublishZoomFactor()
    {
        if (DataContext is not SingleViewerViewModel viewModel || ViewerScrollView is not { } scrollView)
            return;
        _updatingZoomFactor = true;
        try
        {
            viewModel.ZoomFactor = scrollView.ZoomFactor;
        }
        finally
        {
            _updatingZoomFactor = false;
        }
    }

    private static double ConstrainZoomFactor(ScrollView scrollView, double zoomFactor) =>
        double.Clamp(zoomFactor, double.Min(scrollView.MinZoomFactor, scrollView.MaxZoomFactor),
            double.Max(scrollView.MinZoomFactor, scrollView.MaxZoomFactor));

    internal sealed record PendingView(Size Size, Point Anchor, double ZoomFactor, bool Animated = false)
    {
        // A replacement may arrive before the previous asynchronous zoom has completed.
        public PendingView Resize(Size newSize) =>
            new(newSize, Anchor, GetReplacementZoomFactor(ZoomFactor, Size, newSize));
    }
}
