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
    private bool _timerRunning;
    private CanvasBitmap? _currentFrame;
    private readonly List<CanvasBitmap> _frames = [];
    private readonly CancellationTokenSource _token = new();

    private int[]? ClonedMsIntervals { get; set; }

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
                if (source.CanRead)
                {
                    var randomAccessStream = source.AsRandomAccessStream();
                    randomAccessStream.Seek(0);
                    _frames.Add(await CanvasBitmap.LoadAsync(sender, randomAccessStream));
                }
            OriginalImageWidth = _frames[0].Size.Width;
            OriginalImageHeight = _frames[0].Size.Height;
            Mode = InitMode; // 触发OnModeChanged
            _timerRunning = true;
            _ = ManualResetEvent.Set();
        }
    }
}
