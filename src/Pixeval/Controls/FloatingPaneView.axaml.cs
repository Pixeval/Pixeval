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

    public static readonly StyledProperty<VerticalAlignment> PaneVerticalAlignmentProperty =
        AvaloniaProperty.Register<FloatingPaneView, VerticalAlignment>(
            nameof(PaneVerticalAlignment));

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
            x.UpdatePseudoClasses();
            if (x._floatingPaneBorder is { } border)
            {
                if (!e.GetNewValue<bool>())
                {
                    x.ShowPaneTemporarily();
                }
                else
                {
                    FloatingPane.ResetState(border);
                    x.ClearTemporaryPaneVisibility();
                }
            }
        });
        IsLockedProperty.Changed.AddClassHandler<FloatingPaneView>((x, e) => x.UpdatePseudoClasses());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _floatingPaneBorder = e.NameScope.Find<Border>(partFloatingPaneBorder);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(pcDocked, IsDocked);
        PseudoClasses.Set(pcLocked, IsLocked && !IsDocked);
    }
}
