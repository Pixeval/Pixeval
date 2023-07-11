using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Util.Converters;

public class BoolNegationToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.To<bool>() ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value.To<Visibility>() is Visibility.Collapsed;
    }
}
