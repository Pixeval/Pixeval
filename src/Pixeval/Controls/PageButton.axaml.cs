// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls;
using FluentIcons.Common;

namespace Pixeval.Controls;

public class PageButton : Button
{
    public static readonly StyledProperty<Symbol> SymbolProperty =
        AvaloniaProperty.Register<PageButton, Symbol>(nameof(Symbol), Symbol.ChevronLeft);

    public Symbol Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public static readonly StyledProperty<string?> ToolTipTextProperty =
        AvaloniaProperty.Register<PageButton, string?>(nameof(ToolTipText));

    public string? ToolTipText
    {
        get => GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }
}
