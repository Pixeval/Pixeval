using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

public static class FloatingPane
{
    private const string ProximityClass = "proximity";
    private const string CloseProximityClass = "close-proximity";
    private const string TemporaryVisibleClass = "temporary-visible";

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsEnabled",
            typeof(FloatingPane),
            defaultValue: false);

    public static readonly AttachedProperty<Visual?> TargetProperty =
        AvaloniaProperty.RegisterAttached<Control, Visual?>(
            "Target",
            typeof(FloatingPane));

    public static readonly AttachedProperty<Thickness> ProximityRangeProperty =
        AvaloniaProperty.RegisterAttached<Control, Thickness>(
            "ProximityRange",
            typeof(FloatingPane),
            defaultValue: new Thickness(100));

    public static readonly AttachedProperty<Thickness> CloseProximityRangeProperty =
        AvaloniaProperty.RegisterAttached<Control, Thickness>(
            "CloseProximityRange",
            typeof(FloatingPane),
            defaultValue: new Thickness(30));

    public static readonly AttachedProperty<TimeSpan> CloseProximityExitDelayProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeSpan>(
            "CloseProximityExitDelay",
            typeof(FloatingPane),
            defaultValue: TimeSpan.FromSeconds(0.3));

    public static readonly AttachedProperty<TimeSpan> TemporaryVisibleDurationProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeSpan>(
            "TemporaryVisibleDuration",
            typeof(FloatingPane),
            defaultValue: TimeSpan.FromSeconds(1));

    static readonly AttachedProperty<FloatingPaneState?> StateProperty =
        AvaloniaProperty.RegisterAttached<Control, FloatingPaneState?>(
            "State",
            typeof(FloatingPane));

    static FloatingPane()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetIsEnabled(Control element) => element.GetValue(IsEnabledProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsEnabled(Control element, bool value) => element.SetValue(IsEnabledProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Visual? GetTarget(Control element) => element.GetValue(TargetProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetTarget(Control element, Visual? value) => element.SetValue(TargetProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Thickness GetProximityRange(Control element) => element.GetValue(ProximityRangeProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetProximityRange(Control element, Thickness value) => element.SetValue(ProximityRangeProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Thickness GetCloseProximityRange(Control element) => element.GetValue(CloseProximityRangeProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetCloseProximityRange(Control element, Thickness value) => element.SetValue(CloseProximityRangeProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TimeSpan GetCloseProximityExitDelay(Control element) => element.GetValue(CloseProximityExitDelayProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetCloseProximityExitDelay(Control element, TimeSpan value) => element.SetValue(CloseProximityExitDelayProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TimeSpan GetTemporaryVisibleDuration(Control element) => element.GetValue(TemporaryVisibleDurationProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetTemporaryVisibleDuration(Control element, TimeSpan value) =>
        element.SetValue(TemporaryVisibleDurationProperty, value);

    public static void ShowTemporarily(Control element, TimeSpan? duration = null)
    {
        var state = GetState(element);
        state.TemporaryVisibleDelayTask?.Dispose();

        element.Classes.Set(TemporaryVisibleClass, true);
        state.TemporaryVisibleDelayTask = DispatcherTimer.RunOnce(
            () => element.Classes.Set(TemporaryVisibleClass, false),
            duration ?? GetTemporaryVisibleDuration(element));
    }

    private static void AttachToHost(Control element)
    {
        if (!GetIsEnabled(element))
            return;

        var host = element.GetVisualAncestors().OfType<Panel>().FirstOrDefault(FloatingPaneHost.GetIsEnabled);
        FloatingPaneHost.RegisterPane(host, element);
    }

    private static void DetachFromHost(Control element)
    {
        FloatingPaneHost.UnregisterPane(element);
    }

    internal static void ResetState(Control element)
    {
        element.Classes.Set(ProximityClass, false);
        element.Classes.Set(CloseProximityClass, false);
    }

    internal static void UpdateState(Control element, bool isInProximity, bool isInCloseProximity)
    {
        SetProximity(element, isInProximity);
        SetCloseProximity(element, isInCloseProximity);
    }

    private static void OnIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>() != e.GetOldValue<bool>())
        {
            if (e.GetNewValue<bool>())
            {
                element.AttachedToVisualTree += OnAttachedToVisualTree;
                element.DetachedFromVisualTree += OnDetachedFromVisualTree;
                AttachToHost(element);
            }
            else
            {
                element.AttachedToVisualTree -= OnAttachedToVisualTree;
                element.DetachedFromVisualTree -= OnDetachedFromVisualTree;
                Cleanup(element);
            }
        }
    }

    private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control element)
            AttachToHost(element);
    }

    private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control element)
            DetachFromHost(element);
    }

    private static void Cleanup(Control element)
    {
        var state = element.GetValue(StateProperty);
        state?.CloseProximityDelayTask?.Dispose();
        state?.TemporaryVisibleDelayTask?.Dispose();

        ResetState(element);
        element.Classes.Set(TemporaryVisibleClass, false);

        DetachFromHost(element);
        element.ClearValue(StateProperty);
    }

    private static void SetProximity(Control element, bool isInProximity)
    {
        element.Classes.Set(ProximityClass, isInProximity);
    }

    private static void SetCloseProximity(Control element, bool isInCloseProximity)
    {
        var state = GetState(element);

        if (!isInCloseProximity && element.Classes.Contains(CloseProximityClass))
        {
            state.CloseProximityDelayTask?.Dispose();
            state.CloseProximityDelayTask = DispatcherTimer.RunOnce(
                () => element.Classes.Set(CloseProximityClass, false),
                GetCloseProximityExitDelay(element));
            return;
        }

        element.Classes.Set(CloseProximityClass, isInCloseProximity);
    }

    private static FloatingPaneState GetState(Control element)
    {
        if (element.GetValue(StateProperty) is { } state)
            return state;

        state = new FloatingPaneState();
        element.SetValue(StateProperty, state);
        return state;
    }
}

internal sealed class FloatingPaneState
{
    public IDisposable? CloseProximityDelayTask { get; set; }

    public IDisposable? TemporaryVisibleDelayTask { get; set; }
}
