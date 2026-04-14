// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Markup.Xaml;

namespace Pixeval.Views.Markup;

public class EnumValuesExtension<T> : MarkupExtension where T : struct, Enum
{
    /// <inheritdoc />
    public override T[] ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues<T>();
}
