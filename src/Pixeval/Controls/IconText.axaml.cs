// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using FluentIcons.Common;

namespace Pixeval.Controls;

public class IconText : TemplatedControl
{
    public static readonly StyledProperty<Symbol> SymbolProperty =
        AvaloniaProperty.Register<IconText, Symbol>(nameof(Symbol));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconText, string?>(nameof(Text));

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<IconText, double>(nameof(IconSize), 20);

    public static readonly StyledProperty<ControlTheme> TextThemeProperty =
        AvaloniaProperty.Register<IconText, ControlTheme>(nameof(TextTheme));

    public Symbol Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public ControlTheme TextTheme
    {
        get => GetValue(TextThemeProperty);
        set => SetValue(TextThemeProperty, value);
    }
}
