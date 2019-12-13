using System;
using System.Globalization;
using System.Windows.Data;

namespace Pixeval.Objects.ValueConverters
{
    public class ImageCenterConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (double) value / 2;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}