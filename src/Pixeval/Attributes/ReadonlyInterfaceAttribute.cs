// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ReadonlyInterfaceAttribute(string ns) : Attribute
{
    public string Namespace { get; } = ns;
}
