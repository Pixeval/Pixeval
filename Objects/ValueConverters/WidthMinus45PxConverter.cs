using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pixeval.Objects.ValueConverters
{
    public class WidthMinus45PxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            return (double) value - 45;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}