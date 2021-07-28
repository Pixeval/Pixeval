using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PInvoke;
using Pixeval.Interop;
using WinRT;

namespace Pixeval.Util
{
    // ReSharper disable once InconsistentNaming
    public static class UIHelper
    {
        public static void SetWindowSize(this Window window, int width, int height)
        {
            var windowNative = window.As<IWindowNative>(); // see https://github.com/microsoft/WinUI-3-Demos/blob/master/src/Build2020Demo/DemoBuildCs/DemoBuildCs/DemoBuildCs/App.xaml.cs
            var dpi = User32.GetDpiForWindow(windowNative.WindowHandle);
            var scalingFactor = (float) dpi / 96;
            var scaledWidth = (int) (width * scalingFactor);
            var scaledHeight = (int) (height * scalingFactor);

            User32.SetWindowPos(windowNative.WindowHandle, User32.SpecialWindowHandles.HWND_TOP,
                0, 0, scaledWidth, scaledHeight,
                User32.SetWindowPosFlags.SWP_NOMOVE);
        }

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
    }
}