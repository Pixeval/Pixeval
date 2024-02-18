using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using System.Numerics;
using System;
using Windows.Foundation;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
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
    /// <remarks>当图片按原比例显示，并占满画布时，图片的缩放比例</remarks>>
    private double ScaledFactor
    {
        get
        {
            var canvasWidth = CanvasControl.ActualWidth;
            var canvasHeight = CanvasControl.ActualHeight;
            var imageResolution = ImageWidth / ImageHeight;
            var canvasResolution = canvasWidth / canvasHeight;
            return (canvasResolution - imageResolution) switch
            {
                > 0 => canvasHeight / ImageHeight,
                _ => canvasWidth / ImageWidth
            };
        }
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
        OnImageScaleChanged(this, ImageScale);
    }

    private static void OnImageScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        OnImageScaleChanged(zoomableImage, e.NewValue.To<float>());
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        var imageScale = zoomableImage.ImageScale;
        switch (zoomableImage.Mode)
        {
            case ZoomableImageMode.Original:
                if (imageScale is not 1)
                {
                    zoomableImage.ImageScale = 1;
                    zoomableImage.SetPosition(zoomableImage.InitPosition);
                }
                break;
            case ZoomableImageMode.Fit:
                var scale = zoomableImage.ScaledFactor;
                if (Math.Abs(scale - imageScale) > 0.01 || imageScale is float.NaN)
                {
                    zoomableImage.ImageScale = (float)scale;
                    zoomableImage.SetPosition(zoomableImage.InitPosition);
                }
                break;
            case ZoomableImageMode.NotFit:
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(e.NewValue.To<ZoomableImageMode>());
                break;
        }
    }

    private static void OnImageScaleChanged(ZoomableImage zoomableImage, float newScale)
    {
        var mode = zoomableImage.Mode;
        switch (newScale)
        {
            case 1:
                if (mode is not ZoomableImageMode.Original)
                    zoomableImage.Mode = ZoomableImageMode.Original;
                break;
            case var _ when Math.Abs(newScale - zoomableImage.ScaledFactor) <= 0.01:
                if (mode is not ZoomableImageMode.Fit)
                    zoomableImage.Mode = ZoomableImageMode.Fit;
                break;
            default:
                if (mode is not ZoomableImageMode.NotFit)
                    zoomableImage.Mode = ZoomableImageMode.NotFit;
                break;
        }
    }
}
