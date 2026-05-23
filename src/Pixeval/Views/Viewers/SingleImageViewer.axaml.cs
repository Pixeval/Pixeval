// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Views.Viewers;

public partial class SingleImageViewer : UserControl
{
    public static readonly DirectProperty<SingleImageViewer, double> MirrorScaleXProperty =
        AvaloniaProperty.RegisterDirect<SingleImageViewer, double>(
            nameof(MirrorScaleX),
            o => o.MirrorScaleX);

    public static readonly DirectProperty<SingleImageViewer, bool> IsMirroredProperty =
        AvaloniaProperty.RegisterDirect<SingleImageViewer, bool>(
            nameof(IsMirrored),
            o => o.IsMirrored,
            (o, v) => o.IsMirrored = v);

    public static readonly DirectProperty<SingleImageViewer, bool> IsPlayingProperty =
        AvaloniaProperty.RegisterDirect<SingleImageViewer, bool>(
            nameof(IsPlaying),
            o => o.IsPlaying,
            (o, v) => o.IsPlaying = v);

    public static readonly DirectProperty<SingleImageViewer, int> RotationDegreeProperty =
        AvaloniaProperty.RegisterDirect<SingleImageViewer, int>(
            nameof(RotationDegree),
            o => o.RotationDegree,
            (o, v) => o.RotationDegree = v);

    public static readonly DirectProperty<SingleImageViewer, double> ZoomFactorProperty =
        AvaloniaProperty.RegisterDirect<SingleImageViewer, double>(
            nameof(ZoomFactor),
            o => o.ZoomFactor,
            (o, v) => o.ZoomFactor = v);

    private SingleViewerViewModel? _subscribedViewModel;

    public SingleImageViewer()
    {
        InitializeComponent();
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
        if (e.PropertyName is nameof(SingleViewerViewModel.LoadSuccessfully)
            or nameof(SingleViewerViewModel.OriginalSource)
            or nameof(SingleViewerViewModel.DisplaySource)
            or nameof(SingleViewerViewModel.IsGifLoadSuccessfully)
            or nameof(SingleViewerViewModel.IsPicGif))
            NotifyCommandCanExecuteChanged();
    }

    private void NotifyCommandCanExecuteChanged()
    {
        MirrorCommand.NotifyCanExecuteChanged();
        RotateClockwiseCommand.NotifyCanExecuteChanged();
        RotateCounterclockwiseCommand.NotifyCanExecuteChanged();
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
        PlayPauseCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
    }

    private bool CanManipulateImage => (DataContext as SingleViewerViewModel)?.LoadSuccessfully == true;

    private bool CanPlayGif => (DataContext as SingleViewerViewModel)?.IsGifLoadSuccessfully == true;

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void Mirror()
    {
        // 仅做IsEnabled绑定，实际逻辑修改IsMirrored属性
    }

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void RotateClockwise() => RotationDegree = (RotationDegree + 90) % 360;

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void RotateCounterclockwise() => RotationDegree = (RotationDegree - 90 + 360) % 360;

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void ZoomIn() => ZoomFactor *= 1.2;

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private void ZoomOut() => ZoomFactor /= 1.2;

    [RelayCommand(CanExecute = nameof(CanPlayGif))]
    private void PlayPause() => IsPlaying = !IsPlaying;

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private async Task CopyAsync()
    {
        if ((DataContext as SingleViewerViewModel)?.DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(this) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private async Task SaveAsync()
    {
        if ((DataContext as SingleViewerViewModel)?.DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(this) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    [RelayCommand(CanExecute = nameof(CanManipulateImage))]
    private async Task SaveAsAsync()
    {
        if ((DataContext as SingleViewerViewModel)?.DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(this) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    /// <summary>
    /// 默认缩放到适应窗口大小（Uniform）
    /// </summary>
    private void ImageViewerOnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is not Control { Bounds.Size: { Width: not 0, Height: not 0 } imageSize }
            || ViewerScrollView is not { Bounds.Size: { Width: not 0, Height: not 0 } panelSize })
            return;

        var ratio = panelSize / imageSize;
        ViewerScrollView.ZoomTo(Math.Min(ratio.X, ratio.Y), false);
    }

    public bool IsMirrored
    {
        get;
        set
        {
            var mirrorScaleX = MirrorScaleX;
            SetAndRaise(IsMirroredProperty, ref field, value);
            RaisePropertyChanged(MirrorScaleXProperty, mirrorScaleX, MirrorScaleX);
        }
    }

    public bool IsPlaying
    {
        get;
        set => SetAndRaise(IsPlayingProperty, ref field, value);
    }

    public int RotationDegree
    {
        get;
        set => SetAndRaise(RotationDegreeProperty, ref field, value);
    }

    public double ZoomFactor
    {
        get => ViewerScrollView?.ZoomFactor ?? 1;
        set
        {
            var old = ZoomFactor;
            if (old != value)
                ViewerScrollView?.ZoomTo(value);
        }
    }

    /// <summary>
    /// 镜像时为-1，否则为1
    /// </summary>
    public double MirrorScaleX => IsMirrored ? -1 : 1;

    private void ViewerScrollView_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollView.ZoomFactorProperty)
            RaisePropertyChanged(ZoomFactorProperty, e.GetOldValue<double>(), e.GetNewValue<double>());
    }
}
