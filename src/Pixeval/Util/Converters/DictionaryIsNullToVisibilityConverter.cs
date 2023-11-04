using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Pixeval.Util.Converters;

public class DictionaryIsNullToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, string language)
        => DictionaryConverter.GetItemOrNull(value, parameter) is null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
