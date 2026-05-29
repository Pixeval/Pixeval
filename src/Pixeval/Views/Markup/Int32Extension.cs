// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Metadata;

namespace Pixeval.Views.Markup;

public class Int32Extension(int value)
{
    [ConstructorArgument("value")]
    public int Value { get; set; } = value;

    public int ProvideValue(IServiceProvider serviceProvider) => Value;
}
