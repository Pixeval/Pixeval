using Avalonia;
using Avalonia.Controls;

namespace Pixeval.Controls;

public class CommandBarElementContainer : ContentControl, ICommandBarElement
{
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<CommandBarElementContainer, bool>(nameof(IsCompact));

    public static readonly StyledProperty<bool> IsInOverflowProperty =
        AvaloniaProperty.Register<CommandBarElementContainer, bool>(nameof(IsInOverflow));

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public bool IsInOverflow
    {
        get => GetValue(IsInOverflowProperty);
        set => SetValue(IsInOverflowProperty, value);
    }
}
