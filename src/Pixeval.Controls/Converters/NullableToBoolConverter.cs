using System;
using Microsoft.UI.Xaml.Data;
using WinUI3Utilities;

namespace Pixeval.Controls.Converters;

public class NullableToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language) => value is not null;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => ThrowHelper.NotSupported<object>();
}
