using System;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    /// <summary>
    /// <see cref="IsPlaying"/>是对外的属性，当无图片、<see cref="IsPlaying"/>又为<see langword="true"/>时，此字段为<see langword="false"/>可以阻止<see cref="CanvasControlOnDraw"/>的渲染
    /// </summary>
    private bool _timerRunning;
    private CanvasBitmap? _currentFrame;
    private readonly List<CanvasBitmap> _frames = [];
    private readonly CancellationTokenSource _token = new();

    private int[]? ClonedMsIntervals { get; set; }

    /// <summary>
    /// 用于控制是否渲染（播放）
    /// </summary>
    private ManualResetEvent ManualResetEvent { get; } = new(true);

    private static void OnMsIntervalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;
        zoomableImage.ClonedMsIntervals = [.. zoomableImage.MsIntervals];
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

            _ = Compute(out var scale, out _, out _, out var x, out var y);
            // 最后调整scale，防止影响前面
            transform *= Matrix3x2.CreateScale(scale);

            var image = new Transform2DEffect
            {
                Source = _currentFrame,
                TransformMatrix = transform,
                InterpolationMode = CanvasImageInterpolation.MultiSampleLinear
            };

            e.DrawingSession.DrawImage(image, new Vector2((float)x, (float)y));
        }
        else
        {
            // 进入此分支时ManualResetEvent一定为false
            _frames.Clear();
            try
            {
                if (Sources is null)
                    return;
                foreach (var source in Sources)
                    if (source is { CanRead: true })
                    {
                        var randomAccessStream = source.AsRandomAccessStream();
                        randomAccessStream.Seek(0);
                        _frames.Add(await CanvasBitmap.LoadAsync(sender, randomAccessStream));
                    }
            }
            catch (Exception)
            {
                _frames.Clear();
            }

            if (_frames.Count is 0)
                return;
            OriginalImageWidth = _frames[0].Size.Width;
            OriginalImageHeight = _frames[0].Size.Height;
            _isInitMode = true;
            Mode = InitMode; // 触发OnModeChanged
            _timerRunning = true;
            // 防止此处ManualResetEvent已经Dispose了，绝大多数情况下不会发生
            if (!IsDisposed)
                _ = ManualResetEvent.Set();
        }
    }
}
