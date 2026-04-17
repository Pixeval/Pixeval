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
    private bool? _isAutoHideEnabled;

    public static readonly StyledProperty<object?> PaneProperty =
        AvaloniaProperty.Register<FloatingPaneView, object?>(nameof(Pane));

    public static readonly StyledProperty<IDataTemplate?> FloatingPaneTemplateProperty =
        AvaloniaProperty.Register<FloatingPaneView, IDataTemplate?>(nameof(FloatingPaneTemplate));

    public static readonly StyledProperty<bool> IsDockedProperty =
        AvaloniaProperty.Register<FloatingPaneView, bool>(nameof(IsDocked));

    public static readonly StyledProperty<bool> IsLockedProperty =
        AvaloniaProperty.Register<FloatingPaneView, bool>(nameof(IsLocked));

    public static readonly StyledProperty<VerticalAlignment> PaneVerticalAlignmentProperty = AvaloniaProperty.Register<FloatingPaneView, VerticalAlignment>(
        nameof(PaneVerticalAlignment));

    static FloatingPaneView()
    {
        IsDockedProperty.Changed.AddClassHandler<FloatingPaneView>((x, e) =>
        {
            x.UpdatePseudoClasses();
            if (x._floatingPaneBorder is { } border)
            {
                if (!e.GetNewValue<bool>())
                {
                    FloatingPane.ShowTemporarily(border);
                }
                else
                {
                    FloatingPane.ResetState(border);
                    FloatingPane.ClearTemporaryVisible(border);
                }
            }
        });
        IsLockedProperty.Changed.AddClassHandler<FloatingPaneView>((x, e) => x.UpdatePseudoClasses());
    }

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _floatingPaneBorder = e.NameScope.Find<Border>(partFloatingPaneBorder);
        _isAutoHideEnabled = null;

        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(pcDocked, IsDocked);
        PseudoClasses.Set(pcLocked, IsLocked && !IsDocked);
    }
}
