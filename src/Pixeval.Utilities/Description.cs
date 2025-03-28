// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Pixeval.Utilities;

public static class DescriptionHelper
{
    public static string GetDescription<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(this TEnum @enum) where TEnum : Enum
    {
        return typeof(TEnum).GetField(@enum.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? ThrowUtils.InvalidOperation<string>("Attribute not found");
    }
}
