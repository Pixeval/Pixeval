using System;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using PInvoke;
using Pixeval.CoreApi.Util;

namespace Pixeval.Util.UI
{
    public static partial class UIHelper
    {
        public static async Task<SoftwareBitmapSource> GetSoftwareBitmapSourceAsync(this IRandomAccessStream randomAccessStream, bool disposeImageStream)
        {
            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
            var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);
            if (disposeImageStream)
            {
                randomAccessStream.Dispose();
            }
            return source;
        }

        public static async Task<BitmapImage> GetBitmapImageSourceAsync(this IRandomAccessStream randomAccessStream)
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(randomAccessStream);
            return bitmapImage;
        }

        public static async Task<IRandomAccessStream> GetUnderlyingStreamAsync(this Image image, bool isGif = false)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(image);
            var buffer = await renderTargetBitmap.GetPixelsAsync();
            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(isGif ? BitmapEncoder.GifEncoderId : BitmapEncoder.PngEncoderId, stream);
            var dpi = User32.GetDpiForWindow(App.GetMainWindowHandle());
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight, 
                (uint) renderTargetBitmap.PixelWidth,
                (uint) renderTargetBitmap.PixelHeight,
                dpi,
                dpi,
                buffer.ToArray());
            await encoder.FlushAsync();
            return stream;
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

        public static void ShowTextToastNotification(string title, string content, string? logoUri = null, Action<ToastContentBuilder>? contentBuilder = null)
        {
            var builder = new ToastContentBuilder()
                .AddText(title, AdaptiveTextStyle.Header)
                .AddText(content, AdaptiveTextStyle.Caption);
            contentBuilder?.Invoke(builder);
            if (logoUri is not null)
            {
                builder.AddAppLogoOverride(logoUri, ToastGenericAppLogoCrop.Default);
            }

            builder.Show();
        }

        private static readonly PropertyInfo AppLogoOverrideUriProperty = typeof(ToastContentBuilder).GetProperty("AppLogoOverrideUri", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static ToastContentBuilder AddAppLogoOverride(
            this ToastContentBuilder builder, 
            string uri, 
            ToastGenericAppLogoCrop? hintCrop = default,
            string? alternateText = default,
            bool? addImageQuery = default)
        {

            var appLogoOverrideUri = new ToastGenericAppLogo()
            {
                Source = uri
            };

            if (hintCrop is { } crop)
            {
                appLogoOverrideUri.HintCrop = crop;
            }

            if (alternateText is { } alt)
            {
                appLogoOverrideUri.AlternateText = alt;
            }

            if (addImageQuery is { } query)
            {
                appLogoOverrideUri.AddImageQuery = query;
            }

            AppLogoOverrideUriProperty.SetValue(builder, appLogoOverrideUri);

            return builder;
        }

        public static ToastContentBuilder AddInlineImage(
            this ToastContentBuilder builder,
            string uri,
            string? alternateText = default,
            bool? addImageQuery = default,
            AdaptiveImageCrop? hintCrop = default,
            bool? hintRemoveMargin = default)
        {
            var inlineImage = new AdaptiveImage
            {
                Source = uri
            };

            if (hintCrop != null)
            {
                inlineImage.HintCrop = hintCrop.Value;
            }

            if (alternateText != default)
            {
                inlineImage.AlternateText = alternateText;
            }

            if (addImageQuery != default)
            {
                inlineImage.AddImageQuery = addImageQuery;
            }

            return builder.AddVisualChild(inlineImage);
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

        public static void SetClipboardContent(Action<DataPackage> contentAction)
        {
            Clipboard.SetContent(new DataPackage().Apply(contentAction));
        }

        public static async Task SetClipboardContentAsync(Func<DataPackage, Task> contentAction)
        {
            var package = new DataPackage();
            await contentAction(package);
            Clipboard.SetContent(package);
        }

        public static void NavigateByNavigationViewTag(this Frame frame, NavigationView sender, NavigationTransitionInfo? transitionInfo = null)
        {
            if (sender.SelectedItem is NavigationViewItem {Tag: NavigationViewTag tag})
            {
                frame.Navigate(tag.NavigateTo, tag.Parameter, transitionInfo ?? new EntranceNavigationTransitionInfo());
            }
        }
    }
}