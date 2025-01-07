// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class LocalizationMetadataAttribute(Type resourceType) : Attribute
{
    public Type ResourceType { get; } = resourceType;

    public bool IsPartial { get; init; }
}

[AttributeUsage(AttributeTargets.Class)]
public class AttachedLocalizationMetadataAttribute<T>(Type resourceType) : Attribute
{
    public Type ResourceType { get; } = resourceType;
}
