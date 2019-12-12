using System;
using System.Globalization;
using System.Windows.Data;

namespace Pixeval.Objects.ValueConverters
{
    public class UserIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"用户ID: {value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}