using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using System;
using Windows.Foundation;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Media;

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
    private double ScaledFactor => GetScaledFactor(new Size(CanvasControl.ActualWidth, CanvasControl.ActualHeight));

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

    private void CanvasOnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(CanvasControl);
        var originalScale = ImageScale;
        Zoom(point.Properties.MouseWheelDelta);
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
            OnImageScaleChanged(ImageScale);
        else
#pragma warning disable CA2245
            Mode = Mode;
#pragma warning restore CA2245
    }

    private static void OnImageScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        zoomableImage.OnImageScaleChanged(e.OldValue.To<float>());
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        switch (zoomableImage.Mode)
        {
            case ZoomableImageMode.Original:
                zoomableImage.ImageScale = 1;
                zoomableImage.SetPosition(zoomableImage.InitPosition);
                break;
            case ZoomableImageMode.Fit:
                zoomableImage.ImageScale = (float)zoomableImage.ScaledFactor;
                zoomableImage.SetPosition(zoomableImage.InitPosition);
                break;
            case ZoomableImageMode.NotFit:
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(e.NewValue.To<ZoomableImageMode>());
                break;
        }
    }

    private void OnImageScaleChanged(float oldScale)
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
