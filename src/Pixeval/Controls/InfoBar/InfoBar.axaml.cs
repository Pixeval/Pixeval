// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls.Primitives;
using FluentIcons.Common;

namespace Pixeval.Controls;

public class InfoBar : TemplatedControl
{
    public static readonly StyledProperty<InfoBarMode> ModeProperty =
        AvaloniaProperty.Register<InfoBar, InfoBarMode>(nameof(Mode));

    public static readonly StyledProperty<Symbol> SymbolProperty =
        AvaloniaProperty.Register<InfoBar, Symbol>(nameof(Symbol));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InfoBar, string?>(nameof(Text));

    public InfoBarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

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
}
