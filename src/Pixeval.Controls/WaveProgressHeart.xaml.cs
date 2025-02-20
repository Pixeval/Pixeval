using System;
using System.Numerics;
using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Pixeval.Controls.Windowing;

namespace Pixeval.Controls;

public sealed partial class WaveProgressHeart : UserControl
{
    private const double TranslateTransformMinY = -5;

    private double _translateTransformYRange;

    /// <summary>
    /// 0-1 value of the wave.
    /// </summary>
    [GeneratedDependencyProperty]
    public partial double Value { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 0.5d)]
    public partial double DurationSeconds { get; set; }

    partial void OnValuePropertyChanged(DependencyPropertyChangedEventArgs e) => UpdateWave((double) e.NewValue);

    partial void OnDurationSecondsPropertyChanged(DependencyPropertyChangedEventArgs e) => UpdateDurationSeconds((double) e.NewValue);

    private void UpdateWave(double value)
    {
        var scale = 1 - value;
        var y = _translateTransformYRange * scale + TranslateTransformMinY;
        Presenter.Translation = new Vector3(0, (float) y, 0);
    }

    private void UpdateDurationSeconds(double value)
    {
        Presenter.TranslationTransition.Duration = TimeSpan.FromSeconds(value);
    }

    public WaveProgressHeart() => InitializeComponent();

    private void WaveProgressHeart_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_loaded)
            return;
        _loaded = true;
        _translateTransformYRange = BorderClip.Height - TranslateTransformMinY;
        UpdateDurationSeconds(DurationSeconds);
        UpdateWave(Value);
        VisualStateManager.GoToState(this, "Indeterminate", true);

        var compositor = WindowFactory.GetWindowForElement(this).Compositor;
        var visual = ElementCompositionPreview.GetElementVisual(BorderClip);
        using var pathBuilder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        pathBuilder.BeginFigure(7.92281f, 10.1545f);
        pathBuilder.AddCubicBezier(new(12.0358f, 6.85826f), new(17.9624f, 7.1521f), new(21.7291f, 10.839f));
        pathBuilder.AddLine(new(24.0007f, 13.0625f));
        pathBuilder.AddLine(new(26.2675f, 10.8422f));
        pathBuilder.AddCubicBezier(new(30.0338f, 7.15321f), new(35.9618f, 6.85812f), new(40.0759f, 10.1548f));
        pathBuilder.AddCubicBezier(new(44.9267f, 14.042f), new(45.3175f, 21.2843f), new(40.913f, 25.6707f));
        pathBuilder.AddLine(new(24.883f, 41.6353f));
        pathBuilder.AddCubicBezier(new(24.3953f, 42.121f), new(23.6067f, 42.1211f), new(23.1189f, 41.6354f));
        pathBuilder.AddLine(new(7.0866f, 25.6702f));
        pathBuilder.AddCubicBezier(new(2.68198f, 21.2841f), new(3.07229f, 14.0419f), new(7.92281f, 10.1545f));
        pathBuilder.EndFigure(CanvasFigureLoop.Closed);
        var canvasGeometry = CanvasGeometry.CreatePath(pathBuilder);
        var compositionPath = new CompositionPath(canvasGeometry);
        var compositionPathGeometry = compositor.CreatePathGeometry(compositionPath);
        var compositionGeometricClip = compositor.CreateGeometricClip(compositionPathGeometry);
        compositionGeometricClip.ViewBox = compositor.CreateViewBox();
        compositionGeometricClip.ViewBox.Offset = new Vector2(50, 0);
        visual.Clip = compositionGeometricClip;
    }

    private bool _loaded;
}
