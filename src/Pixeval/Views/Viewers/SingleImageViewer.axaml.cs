// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using AnimatedControls.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Pixeval.ViewModels.Viewers;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Views.Viewers;

/// <summary>
/// <see cref="SwipeImageViewer"/> 内部使用
/// </summary>
[PseudoClasses(PcScrollable, PcPlain)]
public partial class SingleImageViewer : UserControl
{
    private const string PcScrollable = ":scrollable";
    private const string PcPlain = ":plain";

    public static readonly StyledProperty<bool> UseScrollViewProperty =
        AvaloniaProperty.Register<SingleImageViewer, bool>(
            nameof(UseScrollView),
            defaultValue: true);

    private SingleViewerViewModel? _subscribedViewModel;
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
        if (ReferenceEquals(_subscribedViewModel, viewModel))
            return;

        UnsubscribeFromViewModel();
        if (VisualRoot is null || viewModel is null)
            return;

        _subscribedViewModel = viewModel;
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
        switch (e.PropertyName)
        {
            case nameof(SingleViewerViewModel.ZoomFactor):
                ApplyViewModelZoomFactor();
                break;
            case nameof(SingleViewerViewModel.LoadSuccessfully):
                NotifyCommandCanExecuteChanged();
                break;
        }
    }

    private void NotifyCommandCanExecuteChanged() => ZoomToFitCommand.NotifyCanExecuteChanged();

    private bool CanManipulateImage => DataContext is SingleViewerViewModel { LoadSuccessfully: true };

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void ZoomToFit() => ZoomToFitCore(true);

    /// <summary>
    /// 默认缩放到适应窗口大小（Uniform）
    /// </summary>
    private void ImageViewerOnSizeChanged(object? sender, SizeChangedEventArgs e) => ZoomToFitCore(false);

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
        PseudoClasses.Set(PcScrollable, UseScrollView);
        PseudoClasses.Set(PcPlain, !UseScrollView);
    }

    private void SetViewerControls(AnimatedImage? imageViewer, ScrollView? scrollView)
    {
        if (ReferenceEquals(ImageViewer, imageViewer) && ReferenceEquals(ViewerScrollView, scrollView))
            return;

        ImageViewer?.SizeChanged -= ImageViewerOnSizeChanged;

        ImageViewer = imageViewer;
        ViewerScrollView = scrollView;

        ImageViewer?.SizeChanged += ImageViewerOnSizeChanged;

        ApplyViewModelZoomFactor();
    }

    private void ZoomToFitCore(bool animation)
    {
        if (ImageViewer is not Control { Bounds.Size: { Width: not 0, Height: not 0 } imageSize }
            || ViewerScrollView is not { Bounds.Size: { Width: not 0, Height: not 0 } panelSize } scrollView)
            return;

        var ratio = panelSize / imageSize;
        scrollView.ZoomTo(double.Min(ratio.X, ratio.Y), animation);
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

    private async void SaveButton_OnRightClick(object? sender, ContextRequestedEventArgs e)
    {
        if (DataContext is SingleViewerViewModel viewModel)
            await viewModel.SaveAsCommand.ExecuteAsync(sender as Control);
    }
}
