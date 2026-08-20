// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Pixeval.Controls;

public class FloatingDockHost : Panel
{
    private const double ProgressEpsilon = 0.001;

    private Rect? _activeTransitionFloatingRect;
    private Rect? _stableFloatingRect;

    public static readonly StyledProperty<double> DockProgressProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(DockProgress));

    public static readonly StyledProperty<bool> IsDockedProperty =
        AvaloniaProperty.Register<FloatingDockHost, bool>(
            nameof(IsDocked));

    public static readonly StyledProperty<Dock> DockPositionProperty =
        AvaloniaProperty.Register<FloatingDockHost, Dock>(
            nameof(DockPosition),
            defaultValue: Dock.Left);

    public static readonly StyledProperty<double> DockedPaneSizeProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(DockedPaneSize),
            defaultValue: 340);

    public static readonly StyledProperty<double> FloatingPaneWidthProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(FloatingPaneWidth),
            defaultValue: 300);

    public static readonly StyledProperty<double> FloatingPaneMarginProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(FloatingPaneMargin),
            defaultValue: 20);

    public static readonly StyledProperty<HorizontalAlignment> FloatingPaneHorizontalAlignmentProperty =
        AvaloniaProperty.Register<FloatingDockHost, HorizontalAlignment>(
            nameof(FloatingPaneHorizontalAlignment),
            defaultValue: HorizontalAlignment.Left);

    public static readonly StyledProperty<VerticalAlignment> FloatingPaneVerticalAlignmentProperty =
        AvaloniaProperty.Register<FloatingDockHost, VerticalAlignment>(
            nameof(FloatingPaneVerticalAlignment),
            defaultValue: VerticalAlignment.Bottom);

    static FloatingDockHost()
    {
        AffectsMeasure<FloatingDockHost>(
            DockProgressProperty,
            DockPositionProperty,
            DockedPaneSizeProperty,
            FloatingPaneWidthProperty,
            FloatingPaneMarginProperty,
            FloatingPaneHorizontalAlignmentProperty,
            FloatingPaneVerticalAlignmentProperty);
    }

    public double DockProgress
    {
        get => GetValue(DockProgressProperty);
        set => SetValue(DockProgressProperty, value);
    }

    public bool IsDocked
    {
        get => GetValue(IsDockedProperty);
        set => SetValue(IsDockedProperty, value);
    }

    public Dock DockPosition
    {
        get => GetValue(DockPositionProperty);
        set => SetValue(DockPositionProperty, value);
    }

    public double DockedPaneSize
    {
        get => GetValue(DockedPaneSizeProperty);
        set => SetValue(DockedPaneSizeProperty, value);
    }

    public double FloatingPaneWidth
    {
        get => GetValue(FloatingPaneWidthProperty);
        set => SetValue(FloatingPaneWidthProperty, value);
    }

    public double FloatingPaneMargin
    {
        get => GetValue(FloatingPaneMarginProperty);
        set => SetValue(FloatingPaneMarginProperty, value);
    }

    public HorizontalAlignment FloatingPaneHorizontalAlignment
    {
        get => GetValue(FloatingPaneHorizontalAlignmentProperty);
        set => SetValue(FloatingPaneHorizontalAlignmentProperty, value);
    }

    public VerticalAlignment FloatingPaneVerticalAlignment
    {
        get => GetValue(FloatingPaneVerticalAlignmentProperty);
        set => SetValue(FloatingPaneVerticalAlignmentProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var progress = CoerceProgress(DockProgress);
        var dockedPaneSize = CoerceLength(DockedPaneSize);
        var reservedSize = dockedPaneSize * progress;

        var pane = GetPane();
        pane?.Measure(GetPaneMeasureSize(availableSize, dockedPaneSize, progress));

        var content = GetContent();
        content?.Measure(GetContentMeasureSize(availableSize, reservedSize));

        var desiredWidth = double.IsInfinity(availableSize.Width)
            ? IsHorizontalDock
                ? double.Max((content?.DesiredSize.Width ?? 0) + reservedSize, pane?.DesiredSize.Width ?? 0)
                : double.Max(content?.DesiredSize.Width ?? 0, pane?.DesiredSize.Width ?? 0)
            : availableSize.Width;

        var desiredHeight = double.IsInfinity(availableSize.Height)
            ? IsHorizontalDock
                ? double.Max(content?.DesiredSize.Height ?? 0, pane?.DesiredSize.Height ?? 0)
                : double.Max((content?.DesiredSize.Height ?? 0) + reservedSize, pane?.DesiredSize.Height ?? 0)
            : availableSize.Height;

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var progress = CoerceProgress(DockProgress);
        var dockedPaneSize = double.Min(
            CoerceLength(DockedPaneSize),
            IsHorizontalDock ? finalSize.Width : finalSize.Height);

        if (GetContent() is { } content)
        {
            content.Arrange(GetContentRect(finalSize, dockedPaneSize * progress));
        }

        if (GetPane() is { } pane)
        {
            var floatingRect = GetTransitionFloatingPaneRect(pane, finalSize, progress);
            var dockedRect = GetDockedPaneRect(finalSize, dockedPaneSize);
            var paneRect = new Rect(
                double.Lerp(floatingRect.X, dockedRect.X, progress),
                double.Lerp(floatingRect.Y, dockedRect.Y, progress),
                double.Lerp(floatingRect.Width, dockedRect.Width, progress),
                double.Lerp(floatingRect.Height, dockedRect.Height, progress));

            Debug.WriteLine($"[FloatingDockHost] Arrange Floating ({floatingRect}) Dock ({dockedRect}) Pane ({paneRect})");
            pane.Arrange(paneRect);

            if (!IsDocked && progress <= ProgressEpsilon)
                _stableFloatingRect = paneRect;
        }

        Debug.WriteLine($"[FloatingDockHost] Arrange finalsize ({finalSize}), progress ({progress})");
        return finalSize;
    }

    private Control? GetContent() => Children.Count > 0 ? Children[0] : null;

    private Control? GetPane() => Children.Count > 1 ? Children[1] : null;

    private bool IsHorizontalDock => DockPosition is Dock.Left or Dock.Right;

    private Size GetPaneMeasureSize(Size availableSize, double dockedPaneSize, double progress) =>
        IsHorizontalDock
            ? new Size(double.Lerp(CoerceLength(FloatingPaneWidth), dockedPaneSize, progress), availableSize.Height)
            : new Size(
                double.Lerp(CoerceLength(FloatingPaneWidth), availableSize.Width, progress),
                availableSize.Height);

    private Size GetContentMeasureSize(Size availableSize, double reservedSize) => DockPosition switch
    {
        Dock.Left or Dock.Right => new Size(double.Max(0, availableSize.Width - reservedSize), availableSize.Height),
        Dock.Top or Dock.Bottom => new Size(availableSize.Width, double.Max(0, availableSize.Height - reservedSize)),
        _ => throw new ArgumentOutOfRangeException(nameof(DockPosition), DockPosition, null),
    };

    private Rect GetContentRect(Size finalSize, double reservedSize) => DockPosition switch
    {
        Dock.Left => new Rect(reservedSize, 0, double.Max(0, finalSize.Width - reservedSize), finalSize.Height),
        Dock.Right => new Rect(0, 0, double.Max(0, finalSize.Width - reservedSize), finalSize.Height),
        Dock.Top => new Rect(0, reservedSize, finalSize.Width, double.Max(0, finalSize.Height - reservedSize)),
        Dock.Bottom => new Rect(0, 0, finalSize.Width, double.Max(0, finalSize.Height - reservedSize)),
        _ => throw new ArgumentOutOfRangeException(nameof(DockPosition), DockPosition, null),
    };

    private Rect GetDockedPaneRect(Size finalSize, double dockedPaneSize) => DockPosition switch
    {
        Dock.Left => new Rect(0, 0, dockedPaneSize, finalSize.Height),
        Dock.Right => new Rect(finalSize.Width - dockedPaneSize, 0, dockedPaneSize, finalSize.Height),
        Dock.Top => new Rect(0, 0, finalSize.Width, dockedPaneSize),
        Dock.Bottom => new Rect(0, finalSize.Height - dockedPaneSize, finalSize.Width, dockedPaneSize),
        _ => throw new ArgumentOutOfRangeException(nameof(DockPosition), DockPosition, null),
    };

    private Rect GetTransitionFloatingPaneRect(Control pane, Size finalSize, double progress)
    {
        if (!IsTransitioning(progress))
        {
            _activeTransitionFloatingRect = null;
            return IsDocked
                ? _stableFloatingRect ?? GetCurrentPaneRectOrFallback(pane, CalculateFloatingPaneRect(pane, finalSize))
                : CalculateFloatingPaneRect(pane, finalSize);
        }

        if (_activeTransitionFloatingRect is { } rect)
            return rect;

        rect = IsDocked
            ? _stableFloatingRect ?? GetCurrentPaneRectOrFallback(pane, CalculateFloatingPaneRect(pane, finalSize))
            : CalculateFloatingPaneRect(pane, finalSize);

        _activeTransitionFloatingRect = rect;
        return rect;

        Rect CalculateFloatingPaneRect(Control pane, Size finalSize)
        {
            var margin = double.Max(0, FloatingPaneMargin);
            var width = double.Min(
                CoerceLength(FloatingPaneWidth),
                double.Max(0, finalSize.Width - margin * 2));
            var height = double.Min(
                pane.DesiredSize.Height > 0 ? pane.DesiredSize.Height : pane.Bounds.Height,
                double.Max(0, finalSize.Height - margin * 2));

            var x = FloatingPaneHorizontalAlignment switch
            {
                HorizontalAlignment.Center => (finalSize.Width - width) / 2,
                HorizontalAlignment.Right => finalSize.Width - width - margin,
                _ => margin,
            };

            var y = FloatingPaneVerticalAlignment switch
            {
                VerticalAlignment.Top => margin,
                VerticalAlignment.Center => (finalSize.Height - height) / 2,
                _ => finalSize.Height - height - margin,
            };

            return new Rect(double.Max(0, x), double.Max(0, y), width, height);
        }

        static bool IsTransitioning(double progress)
        {
            return progress is > ProgressEpsilon and < 1 - ProgressEpsilon;
        }

        static Rect GetCurrentPaneRectOrFallback(Control pane, Rect fallback)
        {
            return pane.Bounds is { Width: > 0, Height: > 0 }
                ? pane.Bounds
                : fallback;
        }
    }

    private static double CoerceProgress(double value) => double.IsNaN(value) ? 0 : double.Clamp(value, 0, 1);

    private static double CoerceLength(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
            return 0;

        return value;
    }
}
