using System;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class DisplayIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => (value.To<int>() + 1).ToString();

    public object ConvertBack(object value, Type targetType, object parameter, string language) => int.Parse(value.To<string>()) - 1;
}
