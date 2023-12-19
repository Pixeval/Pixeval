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
using Windows.Foundation;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Microsoft.Graphics.Canvas.Effects;

namespace Pixeval.Controls;

/// <summary>
/// 主体：<see cref="ZoomableImageMain"/>，由此控制渲染速度<br/>
/// 渲染：<see cref="CanvasControlOnDraw"/>，图片渲染逻辑<br/>
/// 对外API：<see cref="Zoom"/>、<see cref="SetPosition"/>
/// </summary>
[DependencyProperty<IEnumerable<IRandomAccessStream>>("Sources", DependencyPropertyDefaultValue.Default, nameof(OnSourcesChanged))]
[DependencyProperty<List<int>>("MsIntervals", DependencyPropertyDefaultValue.Default, nameof(OnMsIntervalsChanged))]
[DependencyProperty<bool>("IsPlaying", "true", nameof(OnIsPlayingChanged))]
[DependencyProperty<int>("ImageRotationDegree", "0", nameof(OnImageRotationDegreeChanged))]
[DependencyProperty<bool>("ImageIsMirrored", "false")]
[DependencyProperty<float>("ImageScale", "1f", nameof(OnImageScaleChanged))]
[DependencyProperty<ZoomableImageMode>("Mode", DependencyPropertyDefaultValue.Default, nameof(OnModeChanged))]
[DependencyProperty<ZoomableImageMode>("InitMode", "ZoomableImageMode.Fit")]
[DependencyProperty<ZoomableImagePosition>("InitPosition", "ZoomableImagePosition.AbsoluteCenter")]
[ObservableObject]
public sealed partial class ZoomableImage : UserControl
{
    public bool IsDisposed { get; private set; }

    public static ZoomableImage? EnsureNotDisposed(object? o)
    {
        return o is ZoomableImage { IsDisposed: false } image ? image : null;
    }

    public ZoomableImage()
    {
        InitializeComponent();

        _ = Task.Run(ZoomableImageMain, _token.Token);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
    }

    /// <summary> 
    /// 每个<see cref="ZoomableImage"/>实例只会有一个本函数运行
    /// </summary>
    private async Task ZoomableImageMain()
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
                    if (ClonedMsIntervals is { } t && t.Count > i)
                        delay = ClonedMsIntervals[i];
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

    #region SizePositionControl

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

    #endregion

    #region FrameRelated

    private static void OnMsIntervalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        zoomableImage.ClonedMsIntervals = new List<int>(zoomableImage.MsIntervals);
    }

    private static void OnSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
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
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        _ = zoomableImage.IsPlaying ? zoomableImage.ManualResetEvent.Set() : zoomableImage.ManualResetEvent.Reset();
    }

    private async void CanvasControlOnDraw(CanvasControl sender, CanvasDrawEventArgs e)
    {
        if (!IsPlaying || _timerRunning)
        {
            if (_currentFrame is null)
                return;
            e.DrawingSession.Clear(Colors.Transparent);

            var transform = Matrix3x2.Identity;
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
                float.DegreesToRadians(ImageRotationDegree));

            transform *= ImageRotationDegree switch
            {
                90 => Matrix3x2.CreateTranslation((float)OriginalImageHeight, 0),
                180 => Matrix3x2.CreateTranslation((float)OriginalImageWidth, (float)OriginalImageHeight),
                270 => Matrix3x2.CreateTranslation(0, (float)OriginalImageWidth),
                _ => Matrix3x2.Identity
            };

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

    private ManualResetEvent ManualResetEvent { get; } = new ManualResetEvent(true);

    #endregion

    #region ScaleRelated

    /// <summary>
    /// 缩放
    /// </summary>
    /// <remarks>MouseWheelDelta = 120 when mouse wheel scrolls up</remarks>
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
        var point = e.GetCurrentPoint(Canvas);
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
        CanvasRectangleGeometry.Rect = new Rect(new Point(), Canvas.ActualSize.ToSize());
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

    #endregion

    #region PositionRelated

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
                ImageCenterY = Canvas.ActualHeight / 2;
                break;
            case ZoomableImagePosition.TopCenter:
                ImageCenterX = Canvas.ActualWidth / 2;
                ImagePositionY = 0;
                break;
            case ZoomableImagePosition.AbsoluteCenter:
                ImageCenterX = Canvas.ActualWidth / 2;
                ImageCenterY = Canvas.ActualHeight / 2;
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
            ImageCenterX += currentPoint.Position.X - _lastPoint.X;
            ImageCenterY += currentPoint.Position.Y - _lastPoint.Y;
        }

        _lastPoint = currentPoint.Position;
    }

    private Point _lastPoint;

    #endregion

    #region RotationRelated

    private static void OnImageRotationDegreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;

        if (zoomableImage.ImageRotationDegree % 90 is not 0)
        {
            throw new ArgumentException($"{nameof(ImageRotationDegree)} must be a multiple of 90");
        }

        switch (zoomableImage.ImageRotationDegree)
        {
            case >= 360:
                zoomableImage.ImageRotationDegree %= 360;
                return;
            case <= -360:
                zoomableImage.ImageRotationDegree = zoomableImage.ImageRotationDegree % 360 + 360;
                return;
            case < 0:
                zoomableImage.ImageRotationDegree += 360;
                return;
        }

        // 更新图片大小
        zoomableImage.OnPropertyChanged(nameof(ImageWidth));
        zoomableImage.OnPropertyChanged(nameof(ImageHeight));

        // 更新图片位置
        zoomableImage.OnPropertyChanged(nameof(ImagePositionX));
        zoomableImage.OnPropertyChanged(nameof(ImagePositionY));
    }

    #endregion

    private void CanvasControlOnUnloaded(object sender, RoutedEventArgs e)
    {
        IsDisposed = true;
        foreach (var frame in _frames)
            frame.Dispose();
        _frames.Clear();
        _token.Cancel();
        _token.Dispose();
        ManualResetEvent.Dispose();
    }
}
