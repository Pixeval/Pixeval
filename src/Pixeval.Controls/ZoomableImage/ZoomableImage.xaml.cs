// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// 主体：<see cref="ZoomableImageMain"/>，由此控制渲染速度<br/>
/// 渲染：<see cref="CanvasControlOnDraw"/>，图片渲染逻辑<br/>
/// 对外API：<see cref="Zoom(float)"/>、<see cref="SetPosition"/>
/// </summary>
[DependencyProperty<object>("Source", DependencyPropertyDefaultValue.Default, nameof(OnSourceChanged), IsNullable = true)]
[DependencyProperty<IReadOnlyList<int>>("MsIntervals", DependencyPropertyDefaultValue.Default, nameof(OnMsIntervalsChanged))]
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

        _ = Task.Run(ZoomableImageMain);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
    }

    /// <summary>
    /// 不知道为什么不能直接用<see cref="IsPlaying"/>，可能渲染调用速度太快了
    /// </summary>
    private bool _isPlayingInternal = true;

    /// <summary> 
    /// 每个<see cref="ZoomableImage"/>实例只会有一个本函数运行
    /// </summary>
    private async Task ZoomableImageMain()
    {
        while (!IsDisposed)
        {
            // 刚开始时图片可能为空，等待图片加载
            if (_frames.Count is 0)
            {
                await Task.Delay(20);
                // 尝试触发加载资源
                CanvasControl.Invalidate();
            }
            else
            {
                var totalDelay = 0;
                var startTime = DateTime.Now;
                for (var i = 0; i < _frames.Count;)
                {
                    var start = DateTime.Now;
                    if (_token.IsCancellationRequested || IsDisposed)
                        return;
                    _currentFrame = _frames[i];
                    CanvasControl.Invalidate();
                    if (_isPlayingInternal)
                    {
                        if ((DateTime.Now - startTime).TotalMilliseconds > totalDelay)
                        {
                            ++i;
                            var delay = 20;
                            if (ClonedMsIntervals is { } t && t.Length > i)
                                delay = ClonedMsIntervals[i];
                            totalDelay += delay;
                            if (delay < 5)
                                continue;
                        }
                        await Task.Delay(10);
                    }
                    else
                    {
                        await Task.Delay(10);
                        var end = DateTime.Now;
                        startTime += end - start;
                    }
                }
            }
            if (_token.IsCancellationRequested)
                return;
        }
    }

    private void CanvasControlOnUnloaded(object sender, RoutedEventArgs e)
    {
        IsDisposed = true;
        CanvasControl.Draw -= CanvasControlOnDraw;
        CanvasControl.Unloaded -= CanvasControlOnUnloaded;
        _token.TryCancelDispose();
        foreach (var frame in _frames)
            frame.Dispose();
        _frames.Clear();
    }
}
