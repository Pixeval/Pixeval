using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Pixeval.Objects.ValueConverters
{
    public class StringSplitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : string.Join(' ', value as IEnumerable<string>);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString().IsNullOrEmpty())
            {
                return new List<string>();
            }
            return value.ToString().Split(" ");
        }
    }
}