// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Pixeval.Views.Converters;

public class StringFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 和BindingBase区别在于这里返回null
        if (value is null)
            return null;
        if (parameter is not string format)
            return value?.ToString();
        return string.Format(culture, format, value);
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    public static StringFormatConverter Instance { get; } = new();
}
