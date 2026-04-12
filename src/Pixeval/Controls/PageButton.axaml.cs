// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentIcons.Common;

namespace Pixeval.Controls;

public class PageButton : TemplatedControl
{
    public static readonly StyledProperty<bool> IsPrevProperty =
        AvaloniaProperty.Register<PageButton, bool>(nameof(IsPrev), true);

    public bool IsPrev
    {
        get => GetValue(IsPrevProperty);
        set => SetValue(IsPrevProperty, value);
    }

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

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<PageButton, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find<Button>("PART_Button") is { } button)
            button.Click += (_, args) => RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
}
