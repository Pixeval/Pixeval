using System;
using System.Globalization;
using System.Windows.Data;

namespace Pixeval.Objects.ValueConverters
{
    public class ThumbnailUrlToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string) value;
            return val == null ? null : PixivImage.FromUrl(val).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}