// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace Pixeval.Controls;

[PseudoClasses(pcDocked, pcLocked)]
public class FloatingPaneView : ContentControl
{
    const string pcDocked = ":docked";
    const string pcLocked = ":locked";

    const string partFloatingPaneBorder = "PART_FloatingPaneBorder";

    private Border? _floatingPaneBorder;

    public static readonly StyledProperty<object?> PaneProperty =
        AvaloniaProperty.Register<FloatingPaneView, object?>(nameof(Pane));

    public static readonly StyledProperty<IDataTemplate?> FloatingPaneTemplateProperty =
        AvaloniaProperty.Register<FloatingPaneView, IDataTemplate?>(nameof(FloatingPaneTemplate));

    public static readonly StyledProperty<bool> IsDockedProperty =
        AvaloniaProperty.Register<FloatingPaneView, bool>(nameof(IsDocked));

    public static readonly StyledProperty<bool> IsLockedProperty =
        AvaloniaProperty.Register<FloatingPaneView, bool>(nameof(IsLocked));

    public static readonly StyledProperty<double> DockProgressProperty =
        AvaloniaProperty.Register<FloatingPaneView, double>(nameof(DockProgress));

    public static readonly StyledProperty<double> DockedPaneWidthProperty =
        AvaloniaProperty.Register<FloatingPaneView, double>(
            nameof(DockedPaneWidth),
            defaultValue: 340);

    public static readonly StyledProperty<double> FloatingPaneWidthProperty =
        AvaloniaProperty.Register<FloatingPaneView, double>(
            nameof(FloatingPaneWidth),
            defaultValue: 300);

    public static readonly StyledProperty<double> FloatingPaneMarginProperty =
        AvaloniaProperty.Register<FloatingPaneView, double>(
            nameof(FloatingPaneMargin),
            defaultValue: 20);

    public static readonly StyledProperty<VerticalAlignment> PaneVerticalAlignmentProperty =
        AvaloniaProperty.Register<FloatingPaneView, VerticalAlignment>(
            nameof(PaneVerticalAlignment),
            defaultValue: VerticalAlignment.Bottom);

    public object? Pane
    {
        get => GetValue(PaneProperty);
        set => SetValue(PaneProperty, value);
    }

    public IDataTemplate? FloatingPaneTemplate
    {
        get => GetValue(FloatingPaneTemplateProperty);
        set => SetValue(FloatingPaneTemplateProperty, value);
    }

    public bool IsDocked
    {
        get => GetValue(IsDockedProperty);
        set => SetValue(IsDockedProperty, value);
    }

    public bool IsLocked
    {
        get => GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    public double DockProgress
    {
        get => GetValue(DockProgressProperty);
        set => SetValue(DockProgressProperty, value);
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

    public VerticalAlignment PaneVerticalAlignment
    {
        get => GetValue(PaneVerticalAlignmentProperty);
        set => SetValue(PaneVerticalAlignmentProperty, value);
    }
    
    public void ShowPaneTemporarily(TimeSpan? duration = null)
    {
        if (IsDocked || _floatingPaneBorder is null)
            return;

        FloatingPane.ShowTemporarily(_floatingPaneBorder, duration);
    }

    public void ClearTemporaryPaneVisibility()
    {
        if (_floatingPaneBorder is null)
            return;

        FloatingPane.ClearTemporaryVisible(_floatingPaneBorder);
    }

    static FloatingPaneView()
    {
        IsDockedProperty.Changed.AddClassHandler<FloatingPaneView>((x, e) =>
        {
            var isDocked = e.GetNewValue<bool>();

            x.UpdatePseudoClasses();
            x.SetValue(DockProgressProperty, isDocked ? 1 : 0);
            if (x._floatingPaneBorder is { } border)
                x.UpdateFloatingPaneVisibilityState(border, isDocked);
        });
        IsLockedProperty.Changed.AddClassHandler<FloatingPaneView>((x, e) => x.UpdatePseudoClasses());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _floatingPaneBorder = e.NameScope.Find<Border>(partFloatingPaneBorder);
        UpdatePseudoClasses();
        SetValue(DockProgressProperty, IsDocked ? 1 : 0);
        if (_floatingPaneBorder is { } border)
            UpdateFloatingPaneVisibilityState(border, IsDocked);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(pcDocked, IsDocked);
        PseudoClasses.Set(pcLocked, IsLocked && !IsDocked);
    }

    private void UpdateFloatingPaneVisibilityState(Border border, bool isDocked)
    {
        if (!isDocked)
        {
            FloatingPane.ShowTemporarily(border);
            return;
        }

        FloatingPane.ResetState(border);
        FloatingPane.ClearTemporaryVisible(border);
    }
}
