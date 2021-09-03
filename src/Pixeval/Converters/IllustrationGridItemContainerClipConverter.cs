using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Data;

namespace Pixeval.Converters
{
    public class IllustrationGridItemContainerClipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double width)
            {
                return new Rect(0, 0, width, 250);
            }

            return default(Rect);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}