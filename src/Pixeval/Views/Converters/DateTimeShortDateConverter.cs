// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Pixeval.Views.Converters;

public class DateTimeShortDateConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTime v => v.ToString(culture.DateTimeFormat.ShortDatePattern),
            DateTimeOffset v => v.ToString(culture.DateTimeFormat.ShortDatePattern),
            _ => throw new ArgumentException(
                $"{nameof(value)} should be a {nameof(DateTime)} or {nameof(DateTimeOffset)}", nameof(value))
        };
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    internal static readonly DateTimeShortDateConverter Instance = new();
}
