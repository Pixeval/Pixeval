using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;
using Microsoft.Graphics.Canvas.UI.Xaml;

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
    /// 图片中心点在<see cref="CanvasControl"/>的X坐标
    /// </summary>
    private double ImageCenterX
    {
        get => _centerX;
        set
        {
            if (_centerX == value)
                return;
            _centerX = value;
            var actualWidth = CanvasControl.ActualWidth;
            var bigger = actualWidth < ImageActualWidth;
            if (ImagePositionLeft < 0 && ImagePositionRight < actualWidth)
                if (bigger)
                    ImagePositionRight = actualWidth;
                else
                    ImagePositionLeft = 0;
            else if (0 < ImagePositionLeft && actualWidth < ImagePositionRight)
                if (bigger)
                    ImagePositionLeft = 0;
                else
                    ImagePositionRight = actualWidth;
            else
            {
                OnPropertyChanged(nameof(ImagePositionLeft));
                OnPropertyChanged(nameof(ImagePositionRight));
            }
        }
    }

    /// <summary>
    /// 图片中心点在<see cref="CanvasControl"/>的Y坐标
    /// </summary>
    private double ImageCenterY
    {
        get => _centerY;
        set
        {
            if (_centerY == value)
                return;
            _centerY = value;
            var actualHeight = CanvasControl.ActualHeight;
            var bigger = actualHeight < ImageActualHeight;
            if (ImagePositionTop < 0 && ImagePositionBottom < actualHeight)
                if (bigger)
                    ImagePositionBottom = actualHeight;
                else
                    ImagePositionTop = 0;
            else if (0 < ImagePositionTop && actualHeight < ImagePositionBottom)
                if (bigger)
                    ImagePositionTop = 0;
                else
                    ImagePositionBottom = actualHeight;
            else
            {
                OnPropertyChanged(nameof(ImagePositionTop));
                OnPropertyChanged(nameof(ImagePositionBottom));
            }
        }
    }

    /// <summary>
    /// 图片左上角<see cref="CanvasControl"/>的左边距
    /// </summary>
    private double ImagePositionLeft
    {
        get => ImageCenterX - ImageActualWidth / 2;
        set => ImageCenterX = value + ImageActualWidth / 2;
    }

    /// <summary>
    /// 图片左上角<see cref="CanvasControl"/>的上边距
    /// </summary>
    private double ImagePositionTop
    {
        get => ImageCenterY - ImageActualHeight / 2;
        set => ImageCenterY = value + ImageActualHeight / 2;
    }

    /// <summary>
    /// 图片左上角<see cref="CanvasControl"/>的左边距
    /// </summary>
    private double ImagePositionRight
    {
        get => ImageCenterX + ImageActualWidth / 2;
        set => ImageCenterX = value - ImageActualWidth / 2;
    }

    /// <summary>
    /// 图片左上角<see cref="CanvasControl"/>的上边距
    /// </summary>
    private double ImagePositionBottom
    {
        get => ImageCenterY + ImageActualHeight / 2;
        set => ImageCenterY = value - ImageActualHeight / 2;
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
            OnPropertyChanged(nameof(ImagePositionLeft));
            OnPropertyChanged(nameof(ImagePositionTop));
            OnPropertyChanged(nameof(ImagePositionRight));
            OnPropertyChanged(nameof(ImagePositionBottom));
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
            OnPropertyChanged(nameof(ImagePositionLeft));
            OnPropertyChanged(nameof(ImagePositionTop));
            OnPropertyChanged(nameof(ImagePositionRight));
            OnPropertyChanged(nameof(ImagePositionBottom));
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
                ImagePositionLeft = 0;
                break;
            case ZoomableImagePosition.Top:
                ImagePositionTop = 0;
                break;
            case ZoomableImagePosition.LeftCenter:
                ImagePositionLeft = 0;
                ImageCenterY = CanvasControl.ActualHeight / 2;
                break;
            case ZoomableImagePosition.TopCenter:
                ImageCenterX = CanvasControl.ActualWidth / 2;
                ImagePositionTop = 0;
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
        var canvas = CanvasControl.To<CanvasControl>();
        var currentPoint = e.GetCurrentPoint(canvas);
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
