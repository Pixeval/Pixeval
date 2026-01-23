// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class LocalizationMetadataAttribute : Attribute
{
    public bool IsPartial { get; init; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AttachedLocalizationMetadataAttribute<T>(string distinctName = "") : Attribute
{
    public string DistinctName { get; } = distinctName;
}
