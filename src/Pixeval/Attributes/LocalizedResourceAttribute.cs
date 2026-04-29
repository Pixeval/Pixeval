// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using FluentIcons.Common;
using Pixeval.I18N;

// ReSharper disable once CheckNamespace
namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LocalizedResourceAttribute(Symbol symbol = default) : Attribute
{
    public LocalizedResourceAttribute(Symbol symbol, string key)
        : this(symbol)
    {
        Resource = I18NManager.GetResource(key);
    }
    
    public LocalizedResourceAttribute(string key)
        : this()
    {
        Resource = I18NManager.GetResource(key);
    }
    
    public Symbol Symbol { get; } = symbol;

    public string Resource { get; init; } = "";
}
