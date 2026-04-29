// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using Pixeval.Controls;

namespace Pixeval.Views.Markup;

public class EnumResourceItemsExtension<T> : MarkupExtension where T : struct, Enum
{
    /// <inheritdoc />
    public override IReadOnlyList<SymbolComboBoxItem> ProvideValue(IServiceProvider serviceProvider) => SymbolComboBoxItem.GetValues<T>();
}
