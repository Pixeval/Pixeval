// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Pixeval.Controls;

public sealed class ProgressRing : Control
{
    private readonly DispatcherTimer _timer;
    private int _frame;

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
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80)
        };
        _timer.Tick += OnTimerTick;

        Width = 16;
        Height = 16;
        Focusable = false;

        Loaded += (_, _) => UpdateTimer();
        Unloaded += (_, _) => _timer.Stop();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!IsActive)
            return;

        var brush = Foreground ?? Brushes.Gray;
        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 0)
            return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var dotRadius = Math.Max(1, size * 0.08);
        var orbitRadius = Math.Max(0, (size / 2) - dotRadius);
        const int dots = 12;

        for (var i = 0; i < dots; ++i)
        {
            var distanceFromHead = (i - _frame + dots) % dots;
            var opacity = 1 - (distanceFromHead / (double)dots * 0.75);
            var angle = (Math.Tau * i / dots) - (Math.PI / 2);
            var point = new Point(
                center.X + (Math.Cos(angle) * orbitRadius),
                center.Y + (Math.Sin(angle) * orbitRadius));

            using (context.PushOpacity(opacity))
                context.DrawEllipse(brush, null, point, dotRadius, dotRadius);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsActiveProperty)
            UpdateTimer();

        if (e.Property == IsActiveProperty || e.Property == ForegroundProperty)
            InvalidateVisual();
    }

    private void UpdateTimer()
    {
        if (IsActive && IsLoaded)
            _timer.Start();
        else
            _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _frame = (_frame + 1) % 12;
        InvalidateVisual();
    }
}
