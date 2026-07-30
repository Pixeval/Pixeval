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
        AvaloniaProperty.Register<SingleImageViewer, bool>(
            nameof(UseScrollView),
            defaultValue: true);

    private SingleViewerViewModel? _subscribedViewModel;
    private SingleViewerViewModel? _fitViewModel;
    private IAnimatedBitmap? _fitSource;
    private bool _initialFitApplied;
    private bool _initialFitQueued;
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
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateViewModelSubscription();
        NotifyCommandCanExecuteChanged();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseScrollViewProperty)
        {
            UpdateViewMode();
        }
    }

    private void UpdateViewModelSubscription()
    {
        var viewModel = DataContext as SingleViewerViewModel;
        UpdateFitTarget(viewModel);

        if (ReferenceEquals(_subscribedViewModel, viewModel))
            return;

        UnsubscribeFromViewModel();
        if (VisualRoot is null || viewModel is null)
            return;

        _subscribedViewModel = viewModel;
        _subscribedViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        QueueInitialZoomToFit();
    }

    private void UnsubscribeFromViewModel()
    {
        _subscribedViewModel?.PropertyChanged -= ViewModelOnPropertyChanged;

        _subscribedViewModel = null;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SingleViewerViewModel.ZoomFactor):
                _initialFitApplied = true;
                ApplyViewModelZoomFactor();
                break;
            case nameof(SingleViewerViewModel.LoadSuccessfully):
                NotifyCommandCanExecuteChanged();
                QueueInitialZoomToFit();
                break;
            case nameof(SingleViewerViewModel.DisplaySource):
                UpdateFitTarget(_subscribedViewModel);
                QueueInitialZoomToFit();
                break;
        }
    }

    private void NotifyCommandCanExecuteChanged() => ZoomToFitCommand.NotifyCanExecuteChanged();

    private bool CanManipulateImage => DataContext is SingleViewerViewModel { LoadSuccessfully: true };

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void ZoomToFit()
    {
        if (TryZoomToFit(true))
            _initialFitApplied = true;
    }

    /// <summary>
    /// 默认缩放到适应窗口大小（Uniform）
    /// </summary>
    private void ImageViewerOnSizeChanged(object? sender, SizeChangedEventArgs e) => QueueInitialZoomToFit();

    private void ImagePresenter_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
            switch (e.GetNewValue<Control?>())
            {
                case null:
                    SetViewerControls(null, null);
                    break;
                case ScrollView { Content: AnimatedImage animatedImage } scrollView:
                    SetViewerControls(animatedImage, scrollView);
                    break;
                case AnimatedImage animatedImage2:
                    SetViewerControls(animatedImage2, null);
                    break;
            }
    }

    private void UpdateViewMode()
    {
        var templateKey = UseScrollView ? ScrollableImageTemplateKey : PlainImageTemplateKey;
        if (!this.TryFindResource(templateKey, out var resource) || resource is not IDataTemplate template)
            throw new InvalidOperationException($"Resource '{templateKey}' must be an {nameof(IDataTemplate)}.");

        // Hold the stateful viewer as direct content so a logical-tree reattach cannot rebuild its data template.
        ImagePresenter.Content = template.Build(DataContext);
        _initialFitApplied = false;
        QueueInitialZoomToFit();
    }

    private void UpdateFitTarget(SingleViewerViewModel? viewModel)
    {
        var source = viewModel?.DisplaySource;
        if (ReferenceEquals(_fitViewModel, viewModel) && ReferenceEquals(_fitSource, source))
            return;

        _fitViewModel = viewModel;
        _fitSource = source;
        _initialFitApplied = false;
        _initialFitQueued = false;
    }

    private void SetViewerControls(AnimatedImage? imageViewer, ScrollView? scrollView)
    {
        if (ReferenceEquals(ImageViewer, imageViewer) && ReferenceEquals(ViewerScrollView, scrollView))
            return;

        ImageViewer?.SizeChanged -= ImageViewerOnSizeChanged;
        if (ViewerScrollView is not null)
            ViewerScrollView.PropertyChanged -= ViewerScrollViewOnPropertyChanged;

        ImageViewer = imageViewer;
        ViewerScrollView = scrollView;

        ImageViewer?.SizeChanged += ImageViewerOnSizeChanged;

        if (scrollView is not null)
        {
            scrollView.PropertyChanged += ViewerScrollViewOnPropertyChanged;
            scrollView.GestureBindings = ImageViewerScrollGestureProfiles.Paging;
        }

        ApplyViewModelZoomFactor();
        QueueInitialZoomToFit();
    }

    private void ViewerScrollViewOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ScrollView.ZoomFactorProperty)
            return;

        var zoomFactor = e.GetNewValue<double>();
        if (DataContext is SingleViewerViewModel viewModel && viewModel.ZoomFactor != zoomFactor)
            viewModel.ZoomFactor = zoomFactor;
    }

    private bool TryZoomToFit(bool animation)
    {
        if (ImageViewer is not Control { Bounds.Size: { Width: not 0, Height: not 0 } imageSize }
            || ViewerScrollView is not { } scrollView)
            return false;

        var panelSize = scrollView.Viewport is { Width: > 0, Height: > 0 } viewport
            ? viewport
            : scrollView.Bounds.Size;
        if (panelSize is not { Width: > 0, Height: > 0 })
            return false;

        var ratio = panelSize / imageSize;
        var fitFactor = double.Min(ratio.X, ratio.Y);
        scrollView.MinZoomFactor = double.Min(scrollView.MinZoomFactor, fitFactor);
        scrollView.MaxZoomFactor = double.Max(scrollView.MaxZoomFactor, fitFactor);
        scrollView.ZoomTo(fitFactor, animation);
        return true;
    }

    private void QueueInitialZoomToFit()
    {
        if (!UseScrollView
            || _initialFitApplied
            || _initialFitQueued
            || _fitViewModel is not { LoadSuccessfully: true } viewModel)
            return;

        var source = _fitSource;
        _initialFitQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            if (!ReferenceEquals(_fitViewModel, viewModel)
                || !ReferenceEquals(_fitSource, source))
                return;

            _initialFitQueued = false;
            if (VisualRoot is null
                || !ReferenceEquals(DataContext, viewModel)
                || _initialFitApplied)
                return;

            if (TryZoomToFit(false))
                _initialFitApplied = true;
        }, DispatcherPriority.Loaded);
    }

    public bool UseScrollView
    {
        get => GetValue(UseScrollViewProperty);
        set => SetValue(UseScrollViewProperty, value);
    }

    private void ApplyViewModelZoomFactor()
    {
        if (DataContext is not SingleViewerViewModel viewModel
            || ViewerScrollView is not { } scrollView
            // 防止动画中被绑定反向影响
            || scrollView.ZoomFactor == viewModel.ZoomFactor)
            return;

        scrollView.ZoomTo(viewModel.ZoomFactor);
    }
}
