// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;

// ReSharper disable once CheckNamespace
namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LocalizedResourceAttribute(Type resourceLoader, string key, object? formatKey = null) : Attribute
{
    public Type ResourceLoader { get; } = resourceLoader;

    public string Key { get; } = key;

    public object? FormatKey { get; } = formatKey;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AttachedLocalizedResourceAttribute(string fieldName, string key) : Attribute
{
    public string FieldName { get; } = fieldName;

    public string Key { get; } = key;
}
