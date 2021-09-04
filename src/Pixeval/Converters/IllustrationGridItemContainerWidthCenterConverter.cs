using System;
using Microsoft.UI.Xaml.Data;

namespace Pixeval.Converters
{
    public class IllustrationGridItemContainerWidthCenterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double width)
            {
                return width / 2;
            }

            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}