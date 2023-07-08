using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Microsoft.UI.Input;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls;

[DependencyProperty<ImageSource>("Source", DependencyPropertyDefaultValue.Default, nameof(OnSourceChanged))]
[DependencyProperty<double>("ImagePositionX", "0d")]
[DependencyProperty<double>("ImagePositionY", "0d")]
[DependencyProperty<double>("OriginalImageWidth", "0d", nameof(OnOriginalSizeChanged))]
[DependencyProperty<double>("OriginalImageHeight", "0d", nameof(OnOriginalSizeChanged))]
[DependencyProperty<float>("ImageScale", "1f", nameof(OnImageScaleChanged))]
[DependencyProperty<ZoomableImageMode>("Mode", DependencyPropertyDefaultValue.Default, nameof(OnModeChanged))]
[DependencyProperty<ZoomableImageMode>("InitMode", "ZoomableImageMode.Fit")]
[DependencyProperty<ZoomableImagePosition>("InitPosition", "ZoomableImagePosition.AbsoluteCenter")]
public sealed partial class ZoomableImage : UserControl
{
    public ZoomableImage()
    {
        InitializeComponent();
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        zoomableImage.Mode = zoomableImage.InitMode;
    }

    #region ScaleRelated

    /// <summary>
    /// MouseWheelDelta = 120 when mouse wheel scrolls up
    /// </summary>
    /// <param name="delta"></param>
    public void Zoom(float delta)
    {
        ImageScale = MathF.Exp(MathF.Log(ImageScale) + delta / 5000f);
    }

    private double ScaledFactor { get; set; }

    private void CanvasOnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(Canvas);
        var originalScale = ImageScale;
        Zoom(point.Properties.MouseWheelDelta);
        var ratio = ImageScale / originalScale;
        var left = point.Position.X - ImagePositionX;
        var top = point.Position.Y - ImagePositionY;
        ImagePositionX = point.Position.X - left * ratio;
        ImagePositionY = point.Position.Y - top * ratio;
    }

    private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        CanvasRectangleGeometry.Rect = new(0, 0, Canvas.ActualWidth, Canvas.ActualHeight);
        ScaledFactor = GetImageScaledFactor();
        OnImageScaleChanged(this, ImageScale);
    }

    private static void OnImageScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        OnImageScaleChanged(d.To<ZoomableImage>(), e.NewValue.To<float>());
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        var imageScale = zoomableImage.ImageScale;
        switch (e.NewValue.To<ZoomableImageMode>())
        {
            case ZoomableImageMode.Original:
                if (imageScale is not 1)
                {
                    zoomableImage.ImageScale = 1;
                    var imageWidth = zoomableImage.OriginalImageWidth;
                    var imageHeight = zoomableImage.OriginalImageHeight;
                    zoomableImage.SetPosition(zoomableImage.InitPosition, imageWidth, imageHeight);
                }
                break;
            case ZoomableImageMode.Fit:
                var scale = zoomableImage.ScaledFactor;
                if (Math.Abs(scale - imageScale) > 0.01 || imageScale is float.NaN)
                {
                    zoomableImage.ImageScale = (float)scale;
                    var imageWidth = zoomableImage.OriginalImageWidth * zoomableImage.ScaledFactor;
                    var imageHeight = zoomableImage.OriginalImageHeight * zoomableImage.ScaledFactor;
                    zoomableImage.SetPosition(zoomableImage.InitPosition, imageWidth, imageHeight);
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

    private static void OnOriginalSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        zoomableImage.ScaledFactor = zoomableImage.GetImageScaledFactor();
    }

    /// <summary>
    /// Get the scale factor of the original image when it is contained inside an <see cref="Microsoft.UI.Xaml.Controls.Image"/> control, and the <see cref="Microsoft.UI.Xaml.Controls.Image.Stretch"/>
    /// property is set to <see cref="Stretch.UniformToFill"/> or <see cref="Stretch.Uniform"/>
    /// </summary>
    private double GetImageScaledFactor()
    {
        var canvasWidth = Canvas.ActualWidth;
        var canvasHeight = Canvas.ActualHeight;
        var imageResolution = OriginalImageWidth / OriginalImageHeight;
        var canvasResolution = canvasWidth / canvasHeight;
        return (canvasResolution - imageResolution) switch
        {
            > 0 => canvasHeight / OriginalImageHeight,
            _ => canvasWidth / OriginalImageWidth
        };
    }

    #endregion

    #region PositionRelated

    public void SetPosition(ZoomableImagePosition position, double imageWidth = 0, double imageHeight = 0)
    {
        if (imageWidth is 0)
            imageWidth = Image.ActualWidth;
        if (imageHeight is 0)
            imageHeight = Image.ActualHeight;
        switch (position)
        {
            case ZoomableImagePosition.Left:
                ImagePositionX = 0;
                break;
            case ZoomableImagePosition.Top:
                ImagePositionY = 0;
                break;
            case ZoomableImagePosition.LeftCenter:
                ImagePositionX = 0;
                ImagePositionY = (Canvas.ActualHeight - imageWidth) / 2;
                break;
            case ZoomableImagePosition.TopCenter:
                ImagePositionX = (Canvas.ActualWidth - imageHeight) / 2;
                ImagePositionY = 0;
                break;
            case ZoomableImagePosition.AbsoluteCenter:
                ImagePositionX = (Canvas.ActualWidth - imageWidth) / 2;
                ImagePositionY = (Canvas.ActualHeight - imageHeight) / 2;
                break;
            case ZoomableImagePosition.Default:
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(position);
                break;
        }
    }

    private void CanvasOnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var currentPoint = e.GetCurrentPoint(Canvas);
        if (currentPoint.Properties.IsLeftButtonPressed)
        {
            ImagePositionX += currentPoint.Position.X - _lastPoint.X;
            ImagePositionY += currentPoint.Position.Y - _lastPoint.Y;
        }

        _lastPoint = currentPoint.Position;
    }

    private Point _lastPoint;

    #endregion
}

public enum ZoomableImageMode
{
    /// <summary>
    /// Image is fitting the canvas
    /// </summary>
    Fit,

    /// <summary>
    /// Image scale is 1
    /// </summary>
    Original,

    /// <summary>
    /// Image is not fitting the canvas
    /// </summary>
    NotFit
}

public enum ZoomableImagePosition
{
    Default,
    Left,
    Top,
    LeftCenter,
    TopCenter,
    AbsoluteCenter
}
