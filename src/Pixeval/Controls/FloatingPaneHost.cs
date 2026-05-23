// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

public static class FloatingPaneHost
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Panel, bool>(
            "IsEnabled",
            typeof(FloatingPaneHost),
            defaultValue: false);

    static readonly AttachedProperty<HashSet<Control>?> RegisteredPanesProperty =
        AvaloniaProperty.RegisterAttached<Panel, HashSet<Control>?>(
            "RegisteredPanes",
            typeof(FloatingPaneHost));

    static FloatingPaneHost()
    {
        IsEnabledProperty.Changed.AddClassHandler<Panel>(OnIsEnabledChanged);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetIsEnabled(Panel element) => element.GetValue(IsEnabledProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsEnabled(Panel element, bool value) => element.SetValue(IsEnabledProperty, value);

    internal static void RegisterPane(Panel? host, Control pane)
    {
        UnregisterPane(pane);

        if (host is null || !GetIsEnabled(host))
            return;

        if (host.GetValue(RegisteredPanesProperty) is not { } panes)
        {
            panes = [];
            host.SetValue(RegisteredPanesProperty, panes);
        }

        panes.Add(pane);
    }

    internal static void UnregisterPane(Control pane)
    {
        foreach (var host in pane.GetVisualAncestors().OfType<Panel>().Where(GetIsEnabled))
        {
            if (host.GetValue(RegisteredPanesProperty) is { } panes)
                panes.Remove(pane);
        }
    }

    private static void OnIsEnabledChanged(Panel host, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            host.PointerMoved += OnPointerMoved;
            host.PointerExited += OnPointerExited;
        }
        else
        {
            host.PointerMoved -= OnPointerMoved;
            host.PointerExited -= OnPointerExited;
            ResetPanes(host);
            host.ClearValue(RegisteredPanesProperty);
        }

        foreach (var pane in host.GetVisualDescendants().OfType<Control>().Where(FloatingPane.GetIsEnabled))
            FloatingPane.RefreshHostRegistration(pane);
    }

    private static void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is Panel host && host.GetValue(RegisteredPanesProperty) is { Count: > 0 } panes)
        {
            var pointerPosition = e.GetPosition(host);
            foreach (var pane in panes.ToArray())
            {
                if (!pane.IsEffectivelyVisible || !FloatingPane.GetIsEnabled(pane))
                    continue;

                var target = FloatingPane.GetTarget(pane) ?? pane;

                if (target.TranslatePoint(default, host) is { } targetOrigin)
                {
                    var targetBounds = new Rect(targetOrigin, target.Bounds.Size);

                    var isInProximity = targetBounds.Inflate(FloatingPane.GetProximityRange(pane)).Contains(pointerPosition);
                    var isInCloseProximity = targetBounds.Inflate(FloatingPane.GetCloseProximityRange(pane)).Contains(pointerPosition);
                    // Debug.WriteLine($"判定结果: proximity={isInProximity}, close={isInCloseProximity}; 鼠标位置: {pointerPosition}; target范围: {targetBounds}");
                    FloatingPane.UpdateState(pane, isInProximity, isInCloseProximity);
                }
                else
                {
                    FloatingPane.SetPointerAway(pane);
                    continue;
                }
            }
        }
    }

    private static void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Panel host)
            SetPanesPointerAway(host);
    }

    private static void ResetPanes(Panel host)
    {
        if (host.GetValue(RegisteredPanesProperty) is not { } panes)
            return;

        foreach (var pane in panes)
            FloatingPane.ResetState(pane);
    }

    private static void SetPanesPointerAway(Panel host)
    {
        if (host.GetValue(RegisteredPanesProperty) is not { } panes)
            return;

        foreach (var pane in panes)
            FloatingPane.SetPointerAway(pane);
    }
}
