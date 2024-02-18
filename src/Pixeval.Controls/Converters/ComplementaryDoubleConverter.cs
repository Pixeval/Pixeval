using System;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class ComplementaryDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => 1 - value.To<double>();

    public object ConvertBack(object value, Type targetType, object parameter, string language) => 1 - value.To<double>();
}
