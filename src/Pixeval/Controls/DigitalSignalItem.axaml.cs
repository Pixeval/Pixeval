using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Pixeval.Controls;

public class DigitalSignalItem : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<DigitalSignalItem, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<DigitalSignalItem, IBrush?>(nameof(Fill));

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
}
