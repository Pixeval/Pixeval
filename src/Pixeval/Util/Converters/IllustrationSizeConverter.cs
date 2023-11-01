using System;
using Microsoft.UI.Xaml.Data;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using WinUI3Utilities;

namespace Pixeval.Util.Converters;

public class IllustrationWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var box = parameter.To<Box>();
        var illustration = value.To<Illustration>();
        var thumbnailUrlOption = box.Value.To<ThumbnailUrlOption>();
        var thumbnailDirection = box.Tag.To<ThumbnailDirection>();
        return thumbnailUrlOption is ThumbnailUrlOption.SquareMedium
            ? thumbnailDirection switch
            {
                ThumbnailDirection.Landscape => IllustrationHeightConverter.PortraitHeight,
                ThumbnailDirection.Portrait => IllustrationHeightConverter.LandscapeHeight,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
            }
            : thumbnailDirection switch
            {
                ThumbnailDirection.Landscape => IllustrationHeightConverter.LandscapeHeight * illustration.Width / illustration.Height,
                ThumbnailDirection.Portrait => IllustrationHeightConverter.PortraitHeight * illustration.Width / illustration.Height,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
            };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}


public class IllustrationHeightConverter : IValueConverter
{
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;

    public object Convert(object value, Type targetType, object? parameter, string language)
    {
        var thumbnailDirection = (parameter?.To<Box>().Tag ?? value).To<ThumbnailDirection>();
        return thumbnailDirection switch
        {
            ThumbnailDirection.Landscape => LandscapeHeight,
            ThumbnailDirection.Portrait => PortraitHeight,
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
