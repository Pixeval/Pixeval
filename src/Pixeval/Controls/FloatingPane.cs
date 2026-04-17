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
    private const string NotProximityClass = "not-proximity";
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
            defaultValue: new Thickness(250));

    public static readonly AttachedProperty<Thickness> CloseProximityRangeProperty =
        AvaloniaProperty.RegisterAttached<Control, Thickness>(
            "CloseProximityRange",
            typeof(FloatingPane),
            defaultValue: new Thickness(60));

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

    static readonly AttachedProperty<FloatingPaneDelayTask?> DelayTasksProperty =
        AvaloniaProperty.RegisterAttached<Control, FloatingPaneDelayTask?>(
            "DelayTasks",
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
    public static void SetCloseProximityExitDelay(Control element, TimeSpan value) =>
        element.SetValue(CloseProximityExitDelayProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TimeSpan GetTemporaryVisibleDuration(Control element) => element.GetValue(TemporaryVisibleDurationProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetTemporaryVisibleDuration(Control element, TimeSpan value) =>
        element.SetValue(TemporaryVisibleDurationProperty, value);

    public static void ShowTemporarily(Control element, TimeSpan? duration = null)
    {
        var state = GetDelayTasks(element);
        state.TemporaryVisibleDelayTask?.Dispose();

        element.Classes.Set(TemporaryVisibleClass, true);
        state.TemporaryVisibleDelayTask = DispatcherTimer.RunOnce(
            () =>
            {
                element.Classes.Set(TemporaryVisibleClass, false);
                state.TemporaryVisibleDelayTask = null;
            },
            duration ?? GetTemporaryVisibleDuration(element));
    }

    internal static void RefreshHostRegistration(Control element)
    {
        var host = element.GetVisualAncestors().OfType<Panel>().FirstOrDefault(FloatingPaneHost.GetIsEnabled);
        FloatingPaneHost.RegisterPane(host, element);
    }

    private static void AttachToHost(Control element)
    {
        if (!GetIsEnabled(element))
            return;

        RefreshHostRegistration(element);
    }

    private static void DetachFromHost(Control element)
    {
        FloatingPaneHost.UnregisterPane(element);
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

    internal static void ResetState(Control element)
    {
        ClearProximityState(element, isPointerAway: false);
    }

    internal static void SetPointerAway(Control element)
    {
        ClearProximityState(element, isPointerAway: true);
    }
    
    internal static void UpdateState(Control element, bool isInProximity, bool isInCloseProximity)
    {
        SetProximity(element, isInProximity);
        SetCloseProximity(element, isInCloseProximity);
    }
    
    private static FloatingPaneDelayTask GetDelayTasks(Control element)
    {
        if (element.GetValue(DelayTasksProperty) is { } state)
            return state;

        state = new FloatingPaneDelayTask();
        element.SetValue(DelayTasksProperty, state);
        return state;
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
                
                var state = element.GetValue(DelayTasksProperty);
                state?.CloseProximityDelayTask?.Dispose();
                state?.CloseProximityDelayTask = null;
                ClearTemporaryVisible(element);

                ResetState(element);

                DetachFromHost(element);
                element.ClearValue(DelayTasksProperty);
            }
        }
    }
    
    internal static void ClearTemporaryVisible(Control element)
    {
        var state = element.GetValue(DelayTasksProperty);
        state?.TemporaryVisibleDelayTask?.Dispose();
        if (state is not null)
            state.TemporaryVisibleDelayTask = null;

        element.Classes.Set(TemporaryVisibleClass, false);
    }

    private static void SetProximity(Control element, bool isInProximity)
    {
        element.Classes.Set(ProximityClass, isInProximity);
        element.Classes.Set(NotProximityClass, !isInProximity);
    }

    private static void SetCloseProximity(Control element, bool isInCloseProximity)
    {
        var state = GetDelayTasks(element);

        if (isInCloseProximity)
        {
            state.CloseProximityDelayTask?.Dispose();
            state.CloseProximityDelayTask = null;
            element.Classes.Set(CloseProximityClass, true);
            return;
        }

        if (!element.Classes.Contains(CloseProximityClass) || state.CloseProximityDelayTask is not null)
            return;

        state.CloseProximityDelayTask = DispatcherTimer.RunOnce(
            () =>
            {
                element.Classes.Set(CloseProximityClass, false);
                state.CloseProximityDelayTask = null;
            },
            GetCloseProximityExitDelay(element));
    }

    private static void ClearProximityState(Control element, bool isPointerAway)
    {
        var state = element.GetValue(DelayTasksProperty);
        state?.CloseProximityDelayTask?.Dispose();
        state?.CloseProximityDelayTask = null;

        element.Classes.Set(ProximityClass, false);
        element.Classes.Set(CloseProximityClass, false);
        element.Classes.Set(NotProximityClass, isPointerAway);
    }
    
    sealed class FloatingPaneDelayTask
    {
        public IDisposable? CloseProximityDelayTask { get; set; }

        public IDisposable? TemporaryVisibleDelayTask { get; set; }
    }
}
