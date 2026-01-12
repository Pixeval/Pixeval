// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;

// ReSharper disable once CheckNamespace
namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LocalizedResourceAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AttachedLocalizedResourceAttribute(string fieldName, string key) : Attribute
{
    public string FieldName { get; } = fieldName;

    public string Key { get; } = key;
}
