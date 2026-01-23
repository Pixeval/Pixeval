// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using FluentIcons.Common;

// ReSharper disable once CheckNamespace
namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LocalizedResourceAttribute(Symbol symbol, string key) : Attribute
{
    public LocalizedResourceAttribute(string key)
        : this(default, key)
    {
    }
    
    public Symbol Symbol { get; } = symbol;
    
    public string Key { get; } = key;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AttachedLocalizedResourceAttribute(Symbol symbol, string fieldName, string key) : Attribute
{
    public AttachedLocalizedResourceAttribute(string fieldName, string key)
        : this(default, fieldName, key)
    {
    }
    
    public Symbol Symbol { get; } = symbol;
    
    public string FieldName { get; } = fieldName;

    public string Key { get; } = key;
}
