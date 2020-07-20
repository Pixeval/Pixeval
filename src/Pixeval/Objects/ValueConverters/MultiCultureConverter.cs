using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Pixeval.Data.ViewModel;

namespace Pixeval.Objects.ValueConverters
{
    public class MultiCultureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return AppContext.AvailableCultures.FirstOrDefault(cul => cul.Name == s) ?? I18nOption.MainlandChinese;
            }

            return I18nOption.MainlandChinese;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is I18nOption option)
            {
                return option.Name;
            }

            return I18nOption.MainlandChinese.Name;
        }
    }
}
