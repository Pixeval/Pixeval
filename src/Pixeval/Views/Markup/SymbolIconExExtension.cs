// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using FluentIcons.Avalonia;
using FluentIcons.Common;

namespace Pixeval.Views.Markup;

public sealed class SymbolIconExExtension : SymbolIcon
{
    public SymbolIconExExtension()
    {
    }

    public SymbolIconExExtension(Symbol symbol)
    {
        Symbol = symbol;
    }

    public SymbolIcon ProvideValue(IServiceProvider serviceProvider) => this;
}
