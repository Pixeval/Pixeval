// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;

namespace Pixeval.Views.Viewers;

public partial class ImageViewerPage : UserControl
{
    public double ZoomFactor
    {
        get;
        set => SetAndRaise(ZoomFactorProperty, ref field, value);
    }

    public static readonly DirectProperty<ImageViewerPage, double> ZoomFactorProperty =
        AvaloniaProperty.RegisterDirect<ImageViewerPage, double>(
            nameof(ZoomFactor),
            o => o.ZoomFactor,
            (o, v) => o.ZoomFactor = v);

    public ImageViewerPage() => InitializeComponent();

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
