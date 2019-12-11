using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Pixeval.Objects.ValueConverters
{
    public class QueryR18ToggleButtonIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> val)
            {
                var enumerable = val as string[] ?? val.ToArray();
                if (enumerable.Contains("R-18") && enumerable.Contains("R-18G"))
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}