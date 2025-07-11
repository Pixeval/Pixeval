// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    private bool _isInitMode;

    /// <summary>
    /// 缩放
    /// </summary>
    /// <remarks>MouseWheelDelta = 120 when mouse wheel scrolls up</remarks>
    /// <param name="delta"></param>
    public void Zoom(float delta)
    {
        ImageScale = Zoom(delta, ImageScale);
    }

    /// <summary>
    /// 缩放
    /// </summary>
    /// <remarks>MouseWheelDelta = 120 when mouse wheel scrolls up</remarks>
    /// <param name="delta"></param>
    /// <param name="scale"></param>
    public static float Zoom(float delta, float scale)
    {
        return MathF.Exp(MathF.Log(scale) + delta / 5000f);
    }

    /// <summary>
    /// Get the scale factor of the original image when it is contained inside a <see cref="Image"/> control, and the <see cref="Image.Stretch"/>
    /// property is set to <see cref="Stretch.UniformToFill"/> or <see cref="Stretch.Uniform"/>
    /// </summary>
    /// <remarks>当图片按原比例显示，并占满画布时，图片的缩放比例</remarks>
    private double ScaledFactor => GetScaledFactor(CanvasSize);

    private Size CanvasSize { get; set; }

    /// <summary>
    /// Get the scale factor of the original image when it is contained inside a <see cref="Image"/> control, and the <see cref="Image.Stretch"/>
    /// property is set to <see cref="Stretch.UniformToFill"/> or <see cref="Stretch.Uniform"/>
    /// </summary>
    /// <remarks>当图片按原比例显示，并占满画布时，图片的缩放比例</remarks>
    private double GetScaledFactor(Size canvasSize)
    {
        var canvasWidth = canvasSize.Width;
        var canvasHeight = canvasSize.Height;
        var imageResolution = ImageWidth / ImageHeight;
        var canvasResolution = canvasWidth / canvasHeight;
        return (canvasResolution - imageResolution) switch
        {
            > 0 => canvasHeight / ImageHeight,
            _ => canvasWidth / ImageWidth
        };
    }
    /// <summary>
    /// 记录上次鼠标事件事件，用于无操作时自动暂停
    /// </summary>
    private DateTime _lastPointerActivityTime = DateTime.Now;
    private void CanvasOnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        _lastPointerActivityTime = DateTime.Now;
        CanvasControl.Paused = false;
        var point = e.GetCurrentPoint(CanvasControl);
        var originalScale = ImageScale;
        Zoom(point.Properties.MouseWheelDelta * 5);
        var ratio = ImageScale / originalScale;
        var left = point.Position.X - ImageCenterX;
        var top = point.Position.Y - ImageCenterY;
        ImageCenterX = point.Position.X - left * ratio;
        ImageCenterY = point.Position.Y - top * ratio;
    }

    private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // 是NotFit则自动调整
        if (ImageScale is 1 or float.NaN || Math.Abs(GetScaledFactor(e.PreviousSize) - ImageScale) > 0.01)
            OnImageScaleChangedInternal(ImageScale);
        else
#pragma warning disable CA2245
            Mode = Mode;
#pragma warning restore CA2245
        CanvasSize = e.NewSize;
    }

    private void OnImageScaleChangedInternal(float oldScale)
    {
        // 初始化时抑制动画
        if (_isInitMode || Source is null)
            _isInitMode = false;
        else
            StartZoomAnimation(oldScale);
        switch (ImageScale)
        {
            case 1:
                if (Mode is not ZoomableImageMode.Original)
                    Mode = ZoomableImageMode.Original;
                break;
            case var _ when Math.Abs(ImageScale - ScaledFactor) <= 0.01:
                if (Mode is not ZoomableImageMode.Fit)
                    Mode = ZoomableImageMode.Fit;
                break;
            default:
                if (Mode is not ZoomableImageMode.NotFit)
                    Mode = ZoomableImageMode.NotFit;
                break;
        }
    }
}
