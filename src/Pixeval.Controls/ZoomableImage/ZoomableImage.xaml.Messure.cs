using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    private Point _lastPoint;
    private DateTime _lastTime;
    private double _centerX;
    private double _centerY;
    private double _originalImageWidth;
    private double _originalImageHeight;

    /// <summary>
    /// <see cref="CanvasControl"/>中心点在<see cref="Canvas"/>的X坐标
    /// </summary>
    private double ImageCenterX
    {
        get => _centerX;
        set
        {
            _centerX = value;
            OnPropertyChanged(nameof(ImagePositionX));
        }
    }

    /// <summary>
    /// <see cref="CanvasControl"/>中心点在<see cref="Canvas"/>的Y坐标
    /// </summary>
    private double ImageCenterY
    {
        get => _centerY;
        set
        {
            _centerY = value;
            OnPropertyChanged(nameof(ImagePositionY));
        }
    }

    /// <summary>
    /// <see cref="CanvasControl"/>左上角<see cref="Canvas"/>的X坐标
    /// </summary>
    private double ImagePositionX
    {
        get => ImageCenterX - ImageActualWidth / 2;
        set => ImageCenterX = value + ImageActualWidth / 2;
    }

    /// <summary>
    /// <see cref="CanvasControl"/>左上角<see cref="Canvas"/>的Y坐标
    /// </summary>
    private double ImagePositionY
    {
        get => ImageCenterY - ImageActualHeight / 2;
        set => ImageCenterY = value + ImageActualHeight / 2;
    }

    /// <summary>
    /// 图片是否90度旋转（即垂直）
    /// </summary>
    private bool IsVertical => ImageRotationDegree % 180 is not 0;

    /// <summary>
    /// 图片当前方向的宽度
    /// </summary>
    private double ImageWidth => IsVertical ? OriginalImageHeight : OriginalImageWidth;

    /// <summary>
    /// 图片当前方向的高度
    /// </summary>
    private double ImageHeight => IsVertical ? OriginalImageWidth : OriginalImageHeight;

    /// <summary>
    /// <see cref="CanvasControl"/>当前方向的宽度
    /// </summary>
    private double ImageActualWidth => ImageWidth * ImageScale;

    /// <summary>
    /// <see cref="CanvasControl"/>当前方向的宽度
    /// </summary>
    private double ImageActualHeight => ImageHeight * ImageScale;

    /// <summary>
    /// 原始图片的宽度
    /// </summary>
    private double OriginalImageWidth
    {
        get => _originalImageWidth;
        set
        {
            _originalImageWidth = value;
            OnPropertyChanged(nameof(ImageWidth));
            OnPropertyChanged(nameof(ImagePositionX));
            OnPropertyChanged(nameof(ImagePositionY));
        }
    }

    /// <summary>
    /// 原始图片的高度
    /// </summary>
    private double OriginalImageHeight
    {
        get => _originalImageHeight;
        set
        {
            _originalImageHeight = value;
            OnPropertyChanged(nameof(ImageHeight));
            OnPropertyChanged(nameof(ImagePositionX));
            OnPropertyChanged(nameof(ImagePositionY));
        }
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(ZoomableImagePosition position)
    {
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
                ImageCenterY = CanvasControl.ActualHeight / 2;
                break;
            case ZoomableImagePosition.TopCenter:
                ImageCenterX = CanvasControl.ActualWidth / 2;
                ImagePositionY = 0;
                break;
            case ZoomableImagePosition.AbsoluteCenter:
                ImageCenterX = CanvasControl.ActualWidth / 2;
                ImageCenterY = CanvasControl.ActualHeight / 2;
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
        var currentPoint = e.GetCurrentPoint(CanvasControl);
        if (currentPoint.Properties.IsLeftButtonPressed)
        {
            var now = DateTime.Now;
            if ((now - _lastTime).TotalMilliseconds < 50)
            {
                ImageCenterX += currentPoint.Position.X - _lastPoint.X;
                ImageCenterY += currentPoint.Position.Y - _lastPoint.Y;
            }
            _lastTime = now;
        }

        _lastPoint = currentPoint.Position;
    }
}
