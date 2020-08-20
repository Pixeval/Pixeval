using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Pixeval.Wpf.ViewModel;

namespace Pixeval.Wpf.Objects.ValueConverters
{
    public class MultiCultureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return AppContext.AvailableCultures.FirstOrDefault(cul => cul.Name == s) ?? I18NOption.MainlandChinese;
            }

            return I18NOption.MainlandChinese;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is I18NOption option)
            {
                return option.Name;
            }

            return I18NOption.MainlandChinese.Name;
        }
    }
}
