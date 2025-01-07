// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using FluentIcons.Common;
using FluentIcons.WinUI;
using Pixeval.Utilities;

namespace Pixeval.Controls.Windowing;

public record NavigationViewTag(Type NavigateTo, object? Parameter, int? Index = null)
{
    public object? Parameter { get; set; } = Parameter;

    public string? Content { get; init; }

    public Symbol? Symbol { get; init; }

    public SymbolIcon? SymbolIcon => Symbol?.Let(t => new SymbolIcon { Symbol = t });
}

public sealed record NavigationViewTag<TPage>(int? Index = null) : NavigationViewTag(typeof(TPage), null, Index);

public sealed record NavigationViewTag<TPage, TParam>(TParam Parameter, int? Index = null) : NavigationViewTag(typeof(TPage), Parameter, Index)
{
    public new TParam Parameter
    {
        get => (TParam)base.Parameter!;
        set => base.Parameter = value;
    }
}
