// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Pixeval.Utilities;

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

    partial void OnMsIntervalsPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (IsDisposed)
            return;
        ClonedMsIntervals = [.. MsIntervals ?? []];
    }

    partial void OnSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (IsDisposed)
            return;
        IsPlaying = true;
        _timerRunning = false;
        // 使CanvasControl具有大小，否则不会触发CanvasControlOnDraw
        OriginalImageWidth = OriginalImageHeight = 10;
        _needInitSource = true;
    }

    private bool _needInitSource;

    public async void InitSource()
    {
        _frames.Clear();
        try
        {
            var sources = Source switch
            {
                Stream stream => await Streams.ReadZipAsync(stream, false),
                IEnumerable<Stream> list => list,
                _ => null
            };

            if (sources is null)
                return;

            foreach (var source in sources)
            {
                var randomAccessStream = source.AsRandomAccessStream();
                randomAccessStream.Seek(0);
                var frame = await CanvasBitmap.LoadAsync(CanvasControl, randomAccessStream);
                source.Position = 0;
                _frames.Add(frame);
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
    }

    partial void OnIsPlayingPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (IsDisposed)
            return;

        _isPlayingInternal = IsPlaying;
    }

    private void CanvasControlOnDraw(CanvasControl sender, CanvasDrawEventArgs e)
    {
        if (_needInitSource)
        {
            _needInitSource = false;
            InitSource();
            return;
        }

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
                transform *= Matrix3x2.CreateTranslation((float) OriginalImageWidth, 0);
            }

            transform *= Matrix3x2.CreateRotation(
                float.DegreesToRadians(ImageRotationDegree));

            transform *= ImageRotationDegree switch
            {
                90 => Matrix3x2.CreateTranslation((float) OriginalImageHeight, 0),
                180 => Matrix3x2.CreateTranslation((float) OriginalImageWidth, (float) OriginalImageHeight),
                270 => Matrix3x2.CreateTranslation(0, (float) OriginalImageWidth),
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

            e.DrawingSession.DrawImage(image, new Vector2((float) x, (float) y));
        }
    }
}
