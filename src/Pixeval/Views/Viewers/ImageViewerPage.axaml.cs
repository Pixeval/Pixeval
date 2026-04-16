// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Views.Viewers;

public partial class ImageViewerPage : UserControl
{
    public ImageViewerPage() => InitializeComponent();

    public event EventHandler? ZoomChanged;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        ZoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        ZoomBorder.PropertyChanged -= ZoomBorderOnPropertyChanged;
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if(e.Property == ScrollView.ZoomFactorProperty)
            ZoomChanged?.Invoke(sender, e);
    }

    /// <summary>
    /// 默认缩放到适应窗口大小（Uniform）
    /// </summary>
    private void ImageViewer_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is not Control { Bounds.Size: { Width: not 0, Height: not 0 } imageSize } ||
            ZoomBorder is not { Bounds.Size: { Width: not 0, Height: not 0 } panelSize })
            return;
        var ratio = panelSize / imageSize;
        var zoomFactor = Math.Min(ratio.X, ratio.Y);
        ZoomBorder.ZoomTo(zoomFactor);
    }
}
