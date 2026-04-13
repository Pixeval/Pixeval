// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Pixeval.Views.Converters;

namespace Pixeval.Views.Markup;

public class EitherBindingExtension<T>(BindingBase binding, T x, T y) : MarkupExtension
{
    public BindingBase Binding { get; set; } = binding;

    /// <inheritdoc />
    public override MultiBinding ProvideValue(IServiceProvider serviceProvider) =>
        new()
        {
            Bindings =
            {
                Binding,
                new CompiledBinding
                {
                    Source = x,
                    Mode = BindingMode.OneTime
                },
                new CompiledBinding
                {
                    Source = y,
                    Mode = BindingMode.OneTime
                }
            },
            Converter = PixevalConverters.EitherConverter
        };
}
