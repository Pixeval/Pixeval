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
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// 主体：<see cref="ZoomableImageMain"/>，由此控制渲染速度<br/>
/// 渲染：<see cref="CanvasControlOnDraw"/>，图片渲染逻辑<br/>
/// 对外API：<see cref="Zoom(float)"/>、<see cref="SetPosition"/>
/// </summary>
[DependencyProperty<IReadOnlyList<Stream>>("Sources", DependencyPropertyDefaultValue.Default, nameof(OnSourcesChanged), IsNullable = true)]
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

        _ = Task.Run(ZoomableImageMain, _token.Token);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
    }

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
                    var delay = 20;
                    _ = ManualResetEvent.WaitOne();
                    if (ClonedMsIntervals is { } t && t.Length > i)
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

    private void CanvasControlOnUnloaded(object sender, RoutedEventArgs e)
    {
        IsDisposed = true;
        _token.Cancel();
        foreach (var frame in _frames)
            frame.Dispose();
        _frames.Clear();
        _token.Dispose();
        ManualResetEvent.Dispose();
    }
}
