using System;
using System.Collections;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Util.Converters;

public class Box
{
    public object Value { get; set; } = null!;

    public object Tag { get; set; } = null!;
}

public class DictionaryConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, string language) => GetItemOrNull(value, parameter);

    public static object? GetItemOrNull(object value, object? parameter)
    {
        if (parameter is null)
            return null;
        var dict = value.To<IDictionary>();
        var target = parameter switch
        {
            Box box => box.Value,
            // Enum e => e,
            _ => parameter
        };
        return dict.Contains(target) ? dict[target] : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

