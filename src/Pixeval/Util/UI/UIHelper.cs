#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/UIHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using CommunityToolkit.WinUI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.AppManagement;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Controls.MarkupExtensions.FontSymbolIcon;
using Pixeval.Util.Threading;
using Pixeval.Utilities;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using WinUI3Utilities;
using Brush = Microsoft.UI.Xaml.Media.Brush;
using Color = Windows.UI.Color;
using FontFamily = Microsoft.UI.Xaml.Media.FontFamily;
using Image = SixLabors.ImageSharp.Image;
using Point = Windows.Foundation.Point;
using Pixeval.Controls.Windowing;
using Size = Windows.Foundation.Size;

namespace Pixeval.Util.UI;

public static partial class UiHelper
{
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

        return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
    }

    [Pure]
    public static SizeInt32 ToSizeInt32(this Size size) => new((int)size.Width, (int)size.Height);

    [Pure]
    public static Size ToSize(this SizeInt32 size) => new(size.Width, size.Height);

    [Pure]
    public static SolidColorBrush WithAlpha(this SolidColorBrush brush, byte alpha) => new(brush.Color.WithAlpha(alpha));

    [Pure]
    public static Color WithAlpha(this Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    public static async Task<double> GetImageAspectRatioAsync(Stream stream, bool disposeOfStream = true)
    {
        using var image = await Image.LoadAsync(stream);
        var result = image.Width / (double)image.Height;
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

    public static Visibility Inverse(this Visibility visibility)
    {
        return visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    public static Visibility ToVisibility(this bool value)
    {
        return value ? Visibility.Visible : Visibility.Collapsed;
    }

    public static FontIcon GetFontIcon(this FontIconSymbols symbol, double? fontSize = null)
    {
        var systemThemeFontFamily = new FontFamily(AppInfo.AppIconFontFamilyName);
        var icon = new FontIcon
        {
            Glyph = symbol.GetGlyph().ToString(),
            FontFamily = systemThemeFontFamily
        };
        if (fontSize is not null)
        {
            icon.FontSize = fontSize.Value;
        }

        return icon;
    }

    public static FontIconSource GetFontIconSource(this FontIconSymbols symbol, double? fontSize = null, Brush? foregroundBrush = null)
    {
        var systemThemeFontFamily = new FontFamily(AppInfo.AppIconFontFamilyName);
        var icon = new FontIconSource
        {
            Glyph = symbol.GetGlyph().ToString(),
            FontFamily = systemThemeFontFamily
        };
        if (fontSize is not null)
        {
            icon.FontSize = fontSize.Value;
        }

        if (foregroundBrush is not null)
        {
            icon.Foreground = foregroundBrush;
        }

        return icon;
    }

    public static T? GetComboBoxSelectedItemTag<T>(this ComboBox box, T? defaultValue = default)
    {
        return box is { SelectedItem: ComboBoxItem { Tag: T t } } ? t : defaultValue;
    }

    public static bool IsFullyOrPartiallyVisible(this FrameworkElement child, FrameworkElement scrollViewer)
    {
        var childTransform = child.TransformToVisual(scrollViewer);
        var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
        var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
        return ownerRectangle.IntersectsWith(childRectangle);
    }

    public static IEnumerable<T> FindChildren<T>(this FrameworkElement startNode) where T : DependencyObject
    {
        return startNode.FindChildren().OfType<T>();
    }

    public static void ClearContent(this RichEditBox box)
    {
        box.Document.SetText(TextSetOptions.None, "");
    }

    public static IAsyncOperation<StorageFolder?> OpenFolderPickerAsync(this Window window)
    {
        var folderPicker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            FileTypeFilter = { "*" }
        };
        return folderPicker.InitializeWithWindow(window).PickSingleFolderAsync();
    }

    public static IAsyncOperation<StorageFile?> OpenFileSavePickerAsync(this Window window, string suggestedFileName, string fileTypeId)
    {
        var savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            FileTypeChoices =
            {
                [fileTypeId] = new List<string> { fileTypeId }
            },
            SuggestedFileName = suggestedFileName
        };
        return savePicker.InitializeWithWindow(window).PickSaveFileAsync();
    }

    public static IAsyncOperation<StorageFile?> OpenFileOpenPickerAsync(this Window window)
    {
        var openPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            ViewMode = PickerViewMode.Thumbnail,
            FileTypeFilter = { "*" }
        };
        return openPicker.InitializeWithWindow(window).PickSingleFileAsync();
    }

    public static async Task<T> AwaitPageTransitionAsync<T>(this Frame root) where T : Page
    {
        await ThreadingHelper.SpinWaitAsync(() => root.Content is not T { IsLoaded: true });
        return (T)root.Content;
    }

    public static Color ParseHexColor(string hex)
    {
        var trimmed = !hex.StartsWith('#') ? $"#{hex}" : hex;
        var color = ColorTranslator.FromHtml(trimmed);
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
