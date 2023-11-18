#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ZoomableImage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Point = Windows.Foundation.Point;
using Size = Windows.Foundation.Size;
using Microsoft.Graphics.Canvas.Effects;

namespace Pixeval.Controls;

/// <summary>
/// 这个控件放在Pixeval.Controls项目时出现
/// Cannot create instance of type 'Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl'
/// Win2D的玄学问题，放在这里暂时没问题，如果可以还是放回去
/// </summary>
[DependencyProperty<IEnumerable<IRandomAccessStream>>("Sources", DependencyPropertyDefaultValue.Default, nameof(OnSourcesChanged))]
[DependencyProperty<List<int>>("MsIntervals", DependencyPropertyDefaultValue.Default, nameof(OnMsIntervalsChanged))]
[DependencyProperty<bool>("IsPlaying", "true", nameof(OnIsPlayingChanged))]
[DependencyProperty<double>("ImagePositionX", "0d")]
[DependencyProperty<double>("ImagePositionY", "0d")]
[DependencyProperty<int>("ImageRotationDegree", "0", nameof(OnImageRotationAngleChanged))]
[DependencyProperty<bool>("ImageIsMirrored", "false")]
[DependencyProperty<float>("ImageScale", "1f", nameof(OnImageScaleChanged))]
[DependencyProperty<ZoomableImageMode>("Mode", DependencyPropertyDefaultValue.Default, nameof(OnModeChanged))]
[DependencyProperty<ZoomableImageMode>("InitMode", "ZoomableImageMode.Fit")]
[DependencyProperty<ZoomableImagePosition>("InitPosition", "ZoomableImagePosition.AbsoluteCenter")]
[ObservableObject]
public sealed partial class ZoomableImage : UserControl
{
    private double _originalImageWidth;

    private double _originalImageHeight;

    private double OriginalImageWidth
    {
        get => _originalImageWidth;
        set => CanvasWidth = _originalImageWidth = value;
    }

    private double OriginalImageHeight
    {
        get => _originalImageWidth;
        set => CanvasHeight = _originalImageHeight = value;
    }

    [ObservableProperty]
    private double _canvasWidth;

    [ObservableProperty]
    private double _canvasHeight;

    public ZoomableImage()
    {
        InitializeComponent();

        _ = Task.Run(Func, _token.Token);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
        return;

        async Task Func()
        {
            while (true)
            {
                if (_frames.Count is 0)
                {
                    await Task.Delay(200, _token.Token);
                }
                else
                {
                    var totalDelay = 0;
                    var startTime = DateTime.Now;
                    for (var i = 0; i < _frames.Count; ++i)
                    {
                        _currentFrame = _frames[i];
                        _ = ManualResetEvent.WaitOne();
                        CanvasControl.Invalidate();
                        _ = ManualResetEvent.WaitOne();
                        var delay = 20;
                        var index = i;
                        if (ClonedMsIntervals is { } t && t.Count > index)
                            delay = ClonedMsIntervals[index];
                        totalDelay += delay;
                        do
                        {
                            _ = ManualResetEvent.WaitOne();
                            await Task.Delay(10, _token.Token);
                        } while ((DateTime.Now - startTime).TotalMilliseconds < totalDelay);
                    }
                }
                if (_token.IsCancellationRequested)
                    return;
            }
        }
    }

    #region FrameRelated

    private static void OnMsIntervalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        zoomableImage.ClonedMsIntervals = new(zoomableImage.MsIntervals);
    }

    private static void OnSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        zoomableImage.IsPlaying = true;
        zoomableImage._timerRunning = false;
        _ = zoomableImage.ManualResetEvent.Reset();
        // 使CanvasControl具有大小，否则不会触发CanvasControlOnDraw
        zoomableImage.OriginalImageWidth = zoomableImage.OriginalImageHeight = 10;
        zoomableImage.CanvasControl.Invalidate();
        // 进入CanvasControlOnDraw的else分支，其中会令Mode = InitMode，从而触发OnModeChanged
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        _ = zoomableImage.IsPlaying ? zoomableImage.ManualResetEvent.Set() : zoomableImage.ManualResetEvent.Reset();
    }

    private async void CanvasControlOnDraw(CanvasControl sender, CanvasDrawEventArgs e)
    {
        if (!IsPlaying || _timerRunning)
        {
            if (_currentFrame is null)
                return;
            e.DrawingSession.Clear(Colors.Transparent);

            var transform = new Matrix3x2
            {
                M11 = 1,
                M22 = 1
            };
            if (ImageIsMirrored)
            {
                // 沿x轴翻转
                transform *= new Matrix3x2
                {
                    M11 = -1,
                    M22 = 1
                };
                // 平移回原来位置
                transform *= Matrix3x2.CreateTranslation((float)OriginalImageWidth, 0);
            }
            transform *= Matrix3x2.CreateRotation(
                float.DegreesToRadians(ImageRotationDegree),
                new((float)(OriginalImageWidth / 2), (float)(OriginalImageHeight / 2)));

            var image = new Transform2DEffect
            {
                Source = _currentFrame,
                TransformMatrix = transform
            };

            e.DrawingSession.DrawImage(image);
        }
        else
        {
            _frames.Clear();
            if (Sources is null)
                return;
            foreach (var source in Sources)
                _frames.Add(await CanvasBitmap.LoadAsync(sender, source));
            OriginalImageWidth = _frames[0].Size.Width;
            OriginalImageHeight = _frames[0].Size.Height;
            if (ImageRotationDegree % 180 is not 0)
            {
                CanvasWidth = OriginalImageHeight;
                CanvasHeight = OriginalImageWidth;
            }
            Mode = InitMode; // 触发OnModeChanged
            _timerRunning = true;
            _ = ManualResetEvent.Set();
        }
    }

    private bool _timerRunning;
    private CanvasBitmap? _currentFrame;
    private readonly List<CanvasBitmap> _frames = [];
    private readonly CancellationTokenSource _token = new();
    private List<int>? ClonedMsIntervals { get; set; }
    private ManualResetEvent ManualResetEvent { get; } = new(true);

    #endregion

    #region ScaleRelated

    /// <summary>
    /// MouseWheelDelta = 120 when mouse wheel scrolls up
    /// </summary>
    /// <param name="delta"></param>
    public void Zoom(float delta)
    {
        ImageScale = MathF.Exp(MathF.Log(ImageScale) + delta / 5000f);
    }

    /// <summary>
    /// Get the scale factor of the original image when it is contained inside an <see cref="Microsoft.UI.Xaml.Controls.Image"/> control, and the <see cref="Microsoft.UI.Xaml.Controls.Image.Stretch"/>
    /// property is set to <see cref="Stretch.UniformToFill"/> or <see cref="Stretch.Uniform"/>
    /// </summary>
    /// <remarks>当图片按原比例显示，并占满画布时，图片的缩放比例</remarks>>
    private double ScaledFactor
    {
        get
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
    }

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
        CanvasRectangleGeometry.Rect = new(new(), Canvas.ActualSize.ToSize());
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
                    zoomableImage.SetPosition(zoomableImage.InitPosition, new(imageWidth, imageHeight));
                }
                break;
            case ZoomableImageMode.Fit:
                var scale = zoomableImage.ScaledFactor;
                if (Math.Abs(scale - imageScale) > 0.01 || imageScale is float.NaN)
                {
                    zoomableImage.ImageScale = (float)scale;
                    var imageWidth = zoomableImage.OriginalImageWidth * zoomableImage.ScaledFactor;
                    var imageHeight = zoomableImage.OriginalImageHeight * zoomableImage.ScaledFactor;
                    zoomableImage.SetPosition(zoomableImage.InitPosition, new(imageWidth, imageHeight));
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

    #endregion

    #region PositionRelated

    public void SetPosition(ZoomableImagePosition position, Size size)
    {
        if (size.Width is 0)
            size.Width = CanvasControl.ActualWidth;
        if (size.Height is 0)
            size.Height = CanvasControl.ActualHeight;
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
                ImagePositionY = (Canvas.ActualHeight - size.Width) / 2;
                break;
            case ZoomableImagePosition.TopCenter:
                ImagePositionX = (Canvas.ActualWidth - size.Height) / 2;
                ImagePositionY = 0;
                break;
            case ZoomableImagePosition.AbsoluteCenter:
                ImagePositionX = (Canvas.ActualWidth - size.Width) / 2;
                ImagePositionY = (Canvas.ActualHeight - size.Height) / 2;
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

    #region RotationRelated

    private static void OnImageRotationAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var zoomableImage = d.To<ZoomableImage>();
        switch (zoomableImage.ImageRotationDegree)
        {
            case >= 360:
                zoomableImage.ImageRotationDegree %= 360;
                return;
            case <= -360:
                zoomableImage.ImageRotationDegree = zoomableImage.ImageRotationDegree % 360 + 360;
                return;
        }

        if (zoomableImage.ImageRotationDegree % 90 is not 0)
        {
            throw new ArgumentException("ImageRotationDegree must be a multiple of 90");
        }

        if (zoomableImage.ImageRotationDegree % 180 is not 0)
        {
            zoomableImage.CanvasWidth = zoomableImage.OriginalImageHeight;
            zoomableImage.CanvasHeight = zoomableImage.OriginalImageWidth;
        }
    }

    #endregion

    private void CanvasControlOnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs e)
    {
        // 由于需要随时重新加载新图片，故创建资源的逻辑放在CanvasControlOnDraw的else分支中
    }

    private void CanvasControlOnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var frame in _frames)
            frame.Dispose();
        _frames.Clear();
        _token.Cancel();
        _token.Dispose();
        ManualResetEvent.Dispose();
    }
}
