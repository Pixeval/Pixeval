// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using FluentIcons.Common;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;
using Pixeval.Util.Threading;
using Pixeval.Utilities;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;
using WinUI3Utilities;
using Brush = Microsoft.UI.Xaml.Media.Brush;
using Color = Windows.UI.Color;
using Image = SixLabors.ImageSharp.Image;
using Point = Windows.Foundation.Point;
using Size = Windows.Foundation.Size;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.WinUI.SymbolIcon;
using SymbolIconSource = FluentIcons.WinUI.SymbolIconSource;

namespace Pixeval.Util.UI;

public static partial class UiHelper
{
    /// <summary>
    /// Detects the perceived brightness of a color, returns <c>true</c> if the color is perceived as light
    /// and returns <c>false</c> if the color is perceived as dark.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static bool PerceivedBright(Color color)
    {
        return 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B >= 128;
    }

    /// <summary>
    /// With higher <paramref name="magnitude"/> you will get brighter color and vice-versa.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="magnitude"></param>
    /// <returns></returns>
    public static Color Brighten(this Color color, int magnitude)
    {
        const int maxByte = 255;

        magnitude = magnitude.CoerceIn((-255, 255));
        int red = color.R;
        int green = color.G;
        int blue = color.B;

        switch (magnitude)
        {
            case < 0:
            {
                (red, green, blue) = (red + magnitude, green + magnitude, blue + magnitude);
                break;
            }
            case > 0:
            {
                var limit = maxByte - magnitude;
                (red, green, blue) = ((red + magnitude).CoerceIn((0, limit)), (green + magnitude).CoerceIn((0, limit)), (blue + magnitude).CoerceIn((0, limit)));
                break;
            }
        }

        return Color.FromArgb(color.A, (byte) red, (byte) green, (byte) blue);
    }

    [Pure]
    public static SizeInt32 ToSizeInt32(this Size size) => new((int) size.Width, (int) size.Height);

    [Pure]
    public static Size ToSize(this SizeInt32 size) => new(size.Width, size.Height);

    [Pure]
    public static SolidColorBrush WithAlpha(this SolidColorBrush brush, byte alpha) => new(brush.Color.WithAlpha(alpha));

    [Pure]
    public static Color WithAlpha(this Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    public static async Task<double> GetImageAspectRatioAsync(Stream stream, bool disposeOfStream = true)
    {
        using var image = await Image.LoadAsync(stream);
        var result = image.Width / (double) image.Height;
        if (disposeOfStream)
        {
            await stream.DisposeAsync();
        }

        return result;
    }

    public static async Task<Color> GetDominantColorAsync(Stream stream, bool disposeOfStream = true)
    {
        using var image = await Image.LoadAsync<Rgb24>(stream);
        image.Mutate(x => x
            .Resize(new ResizeOptions { Sampler = KnownResamplers.NearestNeighbor, Size = new(100, 0) })
            .Quantize(new OctreeQuantizer(new QuantizerOptions { Dither = null, MaxColors = 1 })));
        var pixel = image[0, 0];
        if (disposeOfStream)
        {
            await stream.DisposeAsync();
        }
        return Color.FromArgb(0xFF, pixel.R, pixel.G, pixel.B);
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

    public static void ClipboardSetText(string text)
    {
        var content = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
        content.SetText(text);
        Clipboard.SetContent(content);
        try
        {
            Clipboard.Flush();
        }
        catch (COMException)
        {
        }
    }

    /// <summary>
    /// 调用此方法不要过快
    /// </summary>
    public static async Task ClipboardSetBitmapAsync(Stream stream)
    {
        using var randomAccessStream = new InMemoryRandomAccessStream();
        randomAccessStream.Size = (ulong)stream.Length;
        await stream.CopyToAsync(randomAccessStream.AsStreamForWrite());
        randomAccessStream.Seek(0);
        // 此处必须是原生的IRandomAccessStream，而非Stream.AsRandomAccessStream()，否则会导致不显示剪切板缩略图
        var reference = RandomAccessStreamReference.CreateFromStream(randomAccessStream);
        var content = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
        content.SetBitmap(reference);
        Clipboard.SetContent(content);
        try
        {
            Clipboard.Flush();
        }
        catch (COMException)
        {
            // 第一次会失败，再试一次
            var content2 = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            content2.SetBitmap(reference);
            Clipboard.SetContent(content2);
            try
            {
                Clipboard.Flush();
            }
            catch (COMException)
            {
            }
        }
    }

    public static void NavigateByNavigationViewTag(this Frame frame, NavigationView sender, NavigationTransitionInfo? transitionInfo = null)
    {
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
        {
            _ = frame.Navigate(tag.NavigateTo, tag.Parameter, transitionInfo);
        }
    }

    public static void NavigateTag(this Frame frame, NavigationViewTag tag, NavigationTransitionInfo? transitionInfo = null)
    {
        _ = frame.Navigate(tag.NavigateTo, tag.Parameter, transitionInfo);
    }

    public static SymbolIcon GetSymbolIcon(this Symbol symbol, bool useSmallFontSize = false)
    {
        var icon = new SymbolIcon
        {
            Symbol = symbol
        };

        if (useSmallFontSize)
            icon.FontSize = 16; // 20 is default

        return icon;
    }

    public static SymbolIconSource GetSymbolIconSource(this Symbol symbol, IconVariant variant = IconVariant.Regular, Brush? foregroundBrush = null, bool useSmallFontSize = false)
    {
        var icon = new SymbolIconSource
        {
            IconVariant = variant,
            Symbol = symbol
        };

        if (useSmallFontSize)
            icon.FontSize = 16; // 20 is default

        if (foregroundBrush is not null)
            icon.Foreground = foregroundBrush;

        return icon;
    }

    public static bool IsFullyOrPartiallyVisible(this FrameworkElement child, FrameworkElement parent)
    {
        var childTransform = child.TransformToVisual(parent);
        var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
        var ownerRectangle = new Rect(new Point(0, 0), parent.RenderSize);
        return ownerRectangle.IntersectsWith(childRectangle);
    }

    public static void ClearContent(this RichEditBox box) => box.Document.SetText(TextSetOptions.None, "");

    public static IAsyncOperation<StorageFolder?> OpenFolderPickerAsync(this FrameworkElement frameworkElement, PickerLocationId location = PickerLocationId.PicturesLibrary) => WindowFactory.GetWindowForElement(frameworkElement).PickSingleFolderAsync(location);

    public static IAsyncOperation<StorageFile?> OpenFileOpenPickerAsync(this FrameworkElement frameworkElement, PickerLocationId location = PickerLocationId.PicturesLibrary) => WindowFactory.GetWindowForElement(frameworkElement).PickSingleFileAsync(location);

    public static IAsyncOperation<IReadOnlyList<StorageFile>> OpenMultipleJsonsOpenPickerAsync(this FrameworkElement frameworkElement)
    {
        var fileOpenPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { ".json" }
        };
        fileOpenPicker.FileTypeFilter.Add(".json");
        InitializeWithWindow.Initialize(fileOpenPicker, (nint) WindowFactory.GetWindowForElement(frameworkElement).HWnd);
        return fileOpenPicker.PickMultipleFilesAsync();
    }

    public static IAsyncOperation<IReadOnlyList<StorageFile>> OpenMultipleDllsOpenPickerAsync(this FrameworkElement frameworkElement)
    {
        var fileOpenPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { ".dll", ".zip" }
        };
        InitializeWithWindow.Initialize(fileOpenPicker, (nint) WindowFactory.GetWindowForElement(frameworkElement).HWnd);
        return fileOpenPicker.PickMultipleFilesAsync();
    }

    public static async Task<T> AwaitPageTransitionAsync<T>(this Frame root) where T : Page
    {
        await root.DispatcherQueue.SpinWaitAsync(() => root.Content is not T { IsLoaded: true });
        return (T) root.Content;
    }

    public static async Task<Page> AwaitPageTransitionAsync(this Frame root, Type pageType)
    {
        await root.DispatcherQueue.SpinWaitAsync(() => root.Content is not Page { IsLoaded: true } || root.Content?.GetType() != pageType);
        return (Page) root.Content;
    }
}
