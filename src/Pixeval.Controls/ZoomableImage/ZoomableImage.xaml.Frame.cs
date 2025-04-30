// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
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

    private int[]? MsIntervals { get; set; }

    private bool _needInitSource;

    public async void InitSource()
    {
        _frames.Clear();
        try
        {
            IEnumerable<Stream>? sources;
            switch (Source)
            {
                case Stream stream:
                    sources = [stream];
                    break;
                case (Stream stream, IEnumerable<int> delays):
                    sources = await Streams.ReadZipAsync(stream, false);
                    MsIntervals = delays.ToArray();
                    break;
                case (IEnumerable<Stream> list, IEnumerable<int> delays):
                    sources = list;
                    MsIntervals = delays.ToArray();
                    break;
                default:
                    sources = null;
                    break;
            }

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

    private DateTime _gifStartTime = DateTime.Now;
    private DateTime _pauseStart = DateTime.Now;
    private void CanvasControlOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs e)
    {
        if (_needInitSource)
        {
            _needInitSource = false;
            InitSource();
            return;
        }
        // 刚开始时图片可能为空，等待图片加载
        if (_frames.Count is 0)
        {
            // 尝试触发加载资源
            CanvasControl.Invalidate();
        }       
        else if (_frames.Count is 1)//就一张图，直接显示
        {
            _currentFrame = _frames[0];
            if (DateTime.Now - _lastPointerActivityTime > TimeSpan.FromSeconds(3))
            {
                CanvasControl.Paused = true;
            }
        }
        else if(IsPlaying)
        {
            //动图，计算当前帧
            var surplus = (DateTime.Now - _gifStartTime).TotalMilliseconds;
            var totalTime = 0;
            var index = 0;
            while (totalTime < surplus && index < _frames.Count)
            {
                totalTime += MsIntervals[index];
                index++;
            }
            if (index == _frames.Count)//播放完毕，重置
            {
                _gifStartTime = DateTime.Now;
                index = 0;                
            }
            _currentFrame = _frames[index];
        }

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

        e.DrawingSession.DrawImage(image, (float) x, (float) y);
    }
}
