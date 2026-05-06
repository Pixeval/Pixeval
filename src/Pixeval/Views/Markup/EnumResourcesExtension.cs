// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Pixeval.Controls;

namespace Pixeval.Views.Markup;

public class EnumResourcesExtension<T> : MarkupExtension where T : struct, Enum
{
    /// <inheritdoc />
    public override IReadOnlyList<SymbolComboBoxItem> ProvideValue(IServiceProvider serviceProvider) => SymbolComboBoxItem.GetValues<T>();
}

public class EnumKeyedResourcesExtension(string key) : MarkupExtension
{
    [ConstructorArgument("key")]
    public string Key { get; set; } = key;

    /// <inheritdoc />
    public override IReadOnlyList<SymbolComboBoxItem> ProvideValue(IServiceProvider serviceProvider) => SymbolComboBoxItem.GetValues(Key);
}
