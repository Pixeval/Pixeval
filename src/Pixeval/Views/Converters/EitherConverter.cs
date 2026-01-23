// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Pixeval.Views.Converters;

public class EitherConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [var switchValue, var first, var fallback])
            throw new ArgumentException($"Exactly 3 values are expected, but {values.Count} were provided.", nameof(values));

        if (switchValue is not bool b)
            return fallback;

        return b ? first : fallback;
    }

    internal static readonly EitherConverter Instance = new();
}
