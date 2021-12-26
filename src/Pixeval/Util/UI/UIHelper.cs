#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/UIHelper.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Misc;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using QRCoder;
using WinRT.Interop;

namespace Pixeval.Util.UI;

public static partial class UIHelper
{
    private static readonly PropertyInfo AppLogoOverrideUriProperty = typeof(ToastContentBuilder).GetProperty("AppLogoOverrideUri", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static T GetDataContext<T>(this FrameworkElement element)
    {
        return (T) element.DataContext;
    }

    public static T GetDataContext<T>(this object element)
    {
        return ((FrameworkElement) element).GetDataContext<T>(); // direct cast will throw exception if the type check fails, and that's exactly what we want
    }

    public static ImageSource GetImageSourceFromUriRelativeToAssetsImageFolder(string relativeToAssetsImageFolder)
    {
        return new BitmapImage(new Uri($"ms-appx:///Assets/Images/{relativeToAssetsImageFolder}"));
    }

    public static void ShowTextToastNotification(string title, string content, string? logoUri = null, Action<ToastContentBuilder>? contentBuilder = null)
    {
        var builder = new ToastContentBuilder()
            .SetBackgroundActivation()
            .AddText(title, AdaptiveTextStyle.Header)
            .AddText(content, AdaptiveTextStyle.Caption);
        contentBuilder?.Invoke(builder);
        if (logoUri is not null)
        {
            builder.AddAppLogoOverride(logoUri, ToastGenericAppLogoCrop.Default);
        }

        builder.Show();
    }

    public static ToastContentBuilder AddAppLogoOverride(
        this ToastContentBuilder builder,
        string uri,
        ToastGenericAppLogoCrop? hintCrop = default,
        string? alternateText = default,
        bool? addImageQuery = default)
    {
        var appLogoOverrideUri = new ToastGenericAppLogo
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

    public static DoubleAnimation CreateDoubleAnimation(this DependencyObject depObj,
        string property,
        Duration duration = default,
        EasingFunctionBase? easingFunction = null,
        double by = default,
        double from = default,
        double to = default)
    {
        var animation = new DoubleAnimation
        {
            Duration = duration,
            EasingFunction = easingFunction,
            By = by,
            From = from,
            To = to
        };
        Storyboard.SetTarget(animation, depObj);
        Storyboard.SetTargetProperty(animation, property);
        return animation;
    }

    public static Storyboard GetStoryboard(this Timeline timeline)
    {
        return CreateStoryboard(timeline);
    }

    public static void BeginStoryboard(this Timeline timeline)
    {
        CreateStoryboard(timeline).Begin();
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
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
        {
            frame.Navigate(tag.NavigateTo, tag.Parameter, transitionInfo ?? new SuppressNavigationTransitionInfo());
        }
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
        var icon = new FontIcon
        {
            Glyph = symbol.GetMetadataOnEnumMember()
        };
        if (fontSize is not null)
        {
            icon.FontSize = fontSize.Value;
        }

        return icon;
    }

    public static FontIconSource GetFontIconSource(this FontIconSymbols symbol, double? fontSize = null)
    {
        var icon = new FontIconSource
        {
            Glyph = symbol.GetMetadataOnEnumMember()
        };
        if (fontSize is not null)
        {
            icon.FontSize = fontSize.Value;
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
        box.Document.SetText(TextSetOptions.None, string.Empty);
    }

    public static void Deactivate(this ObservableRecipient recipient)
    {
        recipient.IsActive = false;
    }

    public static void Invisible(this UIElement element)
    {
        element.Visibility = Visibility.Collapsed;
    }

    public static void Visible(this UIElement element)
    {
        element.Visibility = Visibility.Visible;
    }

    public static Size ToWinRtSize(this SizeInt32 size)
    {
        return new Size(size.Width, size.Height);
    }

    public static async Task<SoftwareBitmapSource> GenerateQrCodeForUrlAsync(string url)
    {
        var qrCodeGen = new QRCodeGenerator();
        var urlPayload = new PayloadGenerator.Url(url);
        var qrCodeData = qrCodeGen.CreateQrCode(urlPayload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new BitmapByteQRCode(qrCodeData);
        var bytes = qrCode.GetGraphic(20);
        return await (await IOHelper.GetRandomAccessStreamFromByteArrayAsync(bytes)).GetSoftwareBitmapSourceAsync(true);
    }

    public static async Task<SoftwareBitmapSource> GenerateQrCodeAsync(string content)
    {
        var qrCodeGen = new QRCodeGenerator();
        var qrCodeData = qrCodeGen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new BitmapByteQRCode(qrCodeData);
        var bytes = qrCode.GetGraphic(20);
        return await (await IOHelper.GetRandomAccessStreamFromByteArrayAsync(bytes)).GetSoftwareBitmapSourceAsync(true);
    }

    public static IAsyncOperation<StorageFolder?> OpenFolderPickerAsync(PickerLocationId suggestedStartLocation)
    {
        var folderPicker = new FolderPicker
        {
            SuggestedStartLocation = suggestedStartLocation,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(folderPicker, App.AppViewModel.GetMainWindowHandle());
        return folderPicker.PickSingleFolderAsync();
    }

    public static IAsyncOperation<StorageFile?> OpenFileSavePickerAsync(string suggestedFileName, string fileTypeName, string fileTypeId)
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
        InitializeWithWindow.Initialize(savePicker, App.AppViewModel.GetMainWindowHandle());
        return savePicker.PickSaveFileAsync();
    }

    public static IAsyncOperation<StorageFile?> OpenFileOpenPickerAsync()
    {
        var openPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            ViewMode = PickerViewMode.Thumbnail,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(openPicker, App.AppViewModel.GetMainWindowHandle());
        return openPicker.PickSingleFileAsync();
    }
}