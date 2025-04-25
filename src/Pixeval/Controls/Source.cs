using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Controls;

public static class Source
{
    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.RegisterAttached(
        "ImageSource", typeof(string), typeof(Source), new(null));

    public static async void SetImageSource(DependencyObject element, string value)
    {
        element.SetValue(ImageSourceProperty, value);
        switch (element)
        {
            case Image image:
                image.Source = await CacheHelper.GetSourceFromCacheAsync(value);
                break;
            case PersonPicture personPicture:
                personPicture.ProfilePicture = await CacheHelper.GetSourceFromCacheAsync(value);
                break;
        }
    }

    public static string GetImageSource(DependencyObject element)
    {
        return (string) element.GetValue(ImageSourceProperty);
    }
}
