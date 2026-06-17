// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace Pixeval.Controls;

public sealed class ProgressRing : Control
{
    private const int Dots = 12;

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsActive));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush?>(nameof(Foreground), Brushes.Gray);

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public ProgressRing()
    {
        Width = 16;
        Height = 16;
        Focusable = false;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!IsActive)
            return;

        var brush = Foreground ?? Brushes.Gray;
        var size = double.Min(Bounds.Width, Bounds.Height);
        if (size <= 0)
            return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var dotRadius = double.Max(1, size * 0.08);
        var orbitRadius = double.Max(0, (size / 2) - dotRadius);

        for (var i = 0; i < Dots; ++i)
        {
            var opacity = 1 - (i / (double) Dots * 0.75);
            var angle = (double.Tau * i / Dots) - (double.Pi / 2);
            var point = new Point(
                center.X + (double.Cos(angle) * orbitRadius),
                center.Y + (double.Sin(angle) * orbitRadius));

            using (context.PushOpacity(opacity))
                context.DrawEllipse(brush, null, point, dotRadius, dotRadius);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsActiveProperty || e.Property == BoundsProperty || e.Property == IsVisibleProperty)
            UpdateAnimation();

        if (e.Property == IsActiveProperty || e.Property == ForegroundProperty)
            InvalidateVisual();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateAnimation();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        StopAnimation();
    }

    private void UpdateAnimation()
    {
        if (!IsLoaded || !IsActive || !IsVisible || Bounds is not { Width: > 0, Height: > 0 })
        {
            StopAnimation();
            return;
        }

        if (ElementComposition.GetElementVisual(this) is not { } visual)
            return;

        visual.CenterPoint = new Vector3((float) Bounds.Width / 2, (float) Bounds.Height / 2, 0);

        var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
        animation.InsertKeyFrame(0f, 0, new LinearEasing());
        animation.InsertKeyFrame(1f, float.Tau, new LinearEasing());
        animation.Duration = TimeSpan.FromMilliseconds(960);
        animation.IterationBehavior = AnimationIterationBehavior.Forever;

        visual.StartAnimation(nameof(CompositionVisual.RotationAngle), animation);
    }

    private void StopAnimation()
    {
        if (ElementComposition.GetElementVisual(this) is not { } visual)
            return;

        visual.StopAnimation(nameof(CompositionVisual.RotationAngle));
        visual.RotationAngle = 0;
    }
}
