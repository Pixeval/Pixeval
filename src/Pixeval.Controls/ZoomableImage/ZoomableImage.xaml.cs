// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// 主体：<see cref="ZoomableImageMain"/>，由此控制渲染速度<br/>
/// 渲染：<see cref="CanvasControlOnDraw"/>，图片渲染逻辑<br/>
/// 对外API：<see cref="Zoom(float)"/>、<see cref="SetPosition"/>
/// </summary>
public sealed partial class ZoomableImage : UserControl, IStructuralDisposalCompleter, INotifyPropertyChanged
{
    public bool IsDisposed { get; private set; }

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

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    public void CompleteDisposal()
    {
        IsDisposed = true;
        CanvasControl.Draw -= CanvasControlOnDraw;
        _token.TryCancelDispose();
        foreach (var frame in _frames)
            frame.Dispose();
        _frames.Clear();
    }

    private void ZoomableImage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ((IStructuralDisposalCompleter) this).Hook();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
