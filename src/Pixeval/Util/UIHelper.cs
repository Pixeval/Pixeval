using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.Util
{
    public static partial class UIHelper
    {
        public static async Task<ImageSource> GetImageSourceAsync(this IRandomAccessStream randomAccessStream)
        {
            using (randomAccessStream)
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(randomAccessStream);
                return bitmapImage;
            }
        }

        public static T GetDataContext<T>(this FrameworkElement element)
        {
            return (T) element.DataContext;
        }

        public static T GetDataContext<T>(this object element)
        {
            return ((FrameworkElement) element).GetDataContext<T>(); // direct cast will throw exception if the type check failed, and that's exactly what we want
        }

        public static ImageSource GetImageSourceFromUriRelativeToAssetsImageFolder(string relativeToAssetsImageFolder)
        {
            return new BitmapImage(new Uri($"ms-appx:///Assets/Images/{relativeToAssetsImageFolder}"));
        }

        public static void ShowTextToastNotification(string title, string content, string? logoUrl = null)
        {
            var builder = new ToastContentBuilder()
                .AddText(title, AdaptiveTextStyle.Header)
                .AddText(content, AdaptiveTextStyle.Caption);
            if (logoUrl is not null)
            {
                builder.AddAppLogoOverride(new Uri(logoUrl), ToastGenericAppLogoCrop.Default);
            }

            builder.Show();
        }

        public static void ScrollToElement(this ScrollViewer scrollViewer, UIElement element)
        {
            var transform = element.TransformToVisual((UIElement) scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));
            scrollViewer.ChangeView(null, position.Y, null, false);
        }

        public static Storyboard CreateStoryboard(params Timeline[] animations)
        {
            var sb = new Storyboard();
            foreach (var animation in animations)
            {
                sb.Children.Add(animation);
            }

            return sb;
        }

        public static DoubleAnimation CreateDoubleAnimation(this DependencyObject depObj, string property, Func<DoubleAnimation> supplier)
        {
            var animation = supplier();
            Storyboard.SetTarget(animation, depObj);
            Storyboard.SetTargetProperty(animation, property);
            return animation;
        }
    }
}