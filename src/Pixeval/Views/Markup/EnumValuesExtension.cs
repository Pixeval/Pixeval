// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Markup.Xaml;

namespace Pixeval.Views.Markup;

public class EnumValuesExtension(Type type) : MarkupExtension
{
    public Type EnumType { get; } = type;

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues(EnumType);
}
