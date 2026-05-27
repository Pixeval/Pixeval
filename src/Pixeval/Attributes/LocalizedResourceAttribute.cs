// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using FluentIcons.Common;
using Pixeval.I18N;

// ReSharper disable once CheckNamespace
namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class LocalizedResourceAttribute(Symbol symbol = default) : Attribute
{
    public LocalizedResourceAttribute(Symbol symbol, string resource)
        : this(symbol)
    {
        Resource = I18NManager.GetResource(resource);
    }
    
    public LocalizedResourceAttribute(string resource)
        : this()
    {
        Resource = I18NManager.GetResource(resource);
    }
    
    public Symbol Symbol { get; } = symbol;

    public string Resource { get; init; } = "";

    public string? Key { get; init; }
}
