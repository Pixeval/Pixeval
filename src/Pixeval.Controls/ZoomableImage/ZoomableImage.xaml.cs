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

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    public void CompleteDisposal()
    {
        IsDisposed = true;
        CanvasControl.Draw -= CanvasControlOnDraw;
        CanvasControl.RemoveFromVisualTree();
        CanvasControl = null;
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
