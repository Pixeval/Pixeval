using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Pixeval.Data.Model.ViewModel;

namespace Pixeval.Objects.ValueConverters
{
    public class IllustSubscriptConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (Illustration) value;
            if (val == null)
            {
                return Visibility.Hidden;
            }

            if (val.IsManga || val.IsUgoira)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}