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

    public static readonly StyledProperty<double> DockedPaneWidthProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(DockedPaneWidth),
            defaultValue: 340);

    public static readonly StyledProperty<double> FloatingPaneWidthProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(FloatingPaneWidth),
            defaultValue: 300);

    public static readonly StyledProperty<double> FloatingPaneMarginProperty =
        AvaloniaProperty.Register<FloatingDockHost, double>(
            nameof(FloatingPaneMargin),
            defaultValue: 20);

    public static readonly StyledProperty<VerticalAlignment> FloatingPaneVerticalAlignmentProperty =
        AvaloniaProperty.Register<FloatingDockHost, VerticalAlignment>(
            nameof(FloatingPaneVerticalAlignment),
            defaultValue: VerticalAlignment.Bottom);

    static FloatingDockHost()
    {
        AffectsMeasure<FloatingDockHost>(
            DockProgressProperty,
            DockedPaneWidthProperty,
            FloatingPaneWidthProperty,
            FloatingPaneMarginProperty,
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

    public double DockedPaneWidth
    {
        get => GetValue(DockedPaneWidthProperty);
        set => SetValue(DockedPaneWidthProperty, value);
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

    public VerticalAlignment FloatingPaneVerticalAlignment
    {
        get => GetValue(FloatingPaneVerticalAlignmentProperty);
        set => SetValue(FloatingPaneVerticalAlignmentProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var progress = CoerceProgress(DockProgress);
        var dockedPaneWidth = CoerceLength(DockedPaneWidth);
        var paneWidth = double.Lerp(CoerceLength(FloatingPaneWidth), dockedPaneWidth, progress);

        var pane = GetPane();
        pane?.Measure(new Size(paneWidth, availableSize.Height));

        var content = GetContent();
        var reservedWidth = dockedPaneWidth * progress;
        content?.Measure(new Size(Math.Max(0, availableSize.Width - reservedWidth), availableSize.Height));

        var desiredWidth = double.IsInfinity(availableSize.Width)
            ? Math.Max((content?.DesiredSize.Width ?? 0) + reservedWidth, pane?.DesiredSize.Width ?? 0)
            : availableSize.Width;

        // var desiredHeight = double.IsInfinity(availableSize.Height)
        //     ? Math.Max(content?.DesiredSize.Height ?? 0, pane?.DesiredSize.Height ?? 0)
        //     : availableSize.Height;

        var desiredHeight = Math.Max(content?.DesiredSize.Height ?? 0, pane?.DesiredSize.Height ?? 0);

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var progress = CoerceProgress(DockProgress);
        var dockedPaneWidth = Math.Min(CoerceLength(DockedPaneWidth), finalSize.Width);

        if (GetContent() is { } content)
        {
            var contentX = dockedPaneWidth * progress;
            content.Arrange(new Rect(
                contentX,
                0,
                // progress < 1-ProgressEpsilon? finalSize.Width : finalSize.Width - dockedPaneWidth, 
                // finalSize.Width,
                Math.Max(0, finalSize.Width - contentX),
                finalSize.Height));
        }

        if (GetPane() is { } pane)
        {
            var floatingRect = GetTransitionFloatingPaneRect(pane, finalSize, progress);
            var dockedRect = new Rect(0, 0, dockedPaneWidth, finalSize.Height);
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
            var margin = Math.Max(0, FloatingPaneMargin);
            var width = Math.Min(
                CoerceLength(FloatingPaneWidth),
                Math.Max(0, finalSize.Width - margin * 2));
            var height = Math.Min(
                pane.DesiredSize.Height > 0 ? pane.DesiredSize.Height : pane.Bounds.Height,
                Math.Max(0, finalSize.Height - margin * 2));

            var y = FloatingPaneVerticalAlignment switch
            {
                VerticalAlignment.Top => margin,
                VerticalAlignment.Center => (finalSize.Height - height) / 2,
                _ => finalSize.Height - height - margin,
            };

            return new Rect(margin, Math.Max(0, y), width, height);
        }
        
        static bool IsTransitioning(double progress)
        {
            return progress > ProgressEpsilon && progress < 1 - ProgressEpsilon;
        }
        
        static Rect GetCurrentPaneRectOrFallback(Control pane, Rect fallback)
        {
            return pane.Bounds.Width > 0 && pane.Bounds.Height > 0
                ? pane.Bounds
                : fallback;
        }
    }

    private static double CoerceProgress(double value) => double.IsNaN(value) ? 0 : Math.Clamp(value, 0, 1);

    private static double CoerceLength(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
            return 0;

        return value;
    }
}
