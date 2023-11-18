#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationThumbnail.xaml.cs
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
using Windows.System;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.IllustrationView;

/// <summary>
/// TODO: IsSelected 在CardControl和CheckBox上是否选择的状态不一致
/// </summary>
[DependencyProperty<IllustrationViewModel>("ViewModel")]
public sealed partial class IllustrationThumbnail : CardControl
{
    /// <summary>
    /// 当本控件被点击时触发，取代Tapped事件
    /// </summary>
    public event EventHandler<TappedRoutedEventArgs>? TappedOverride;

    /// <summary>
    /// 请求显示二维码
    /// </summary>
    public event EventHandler<SoftwareBitmapSource>? ShowQrCodeRequested;

    /// <summary>
    /// 请求获取承载本控件的<see cref="IllustrationView"/>
    /// </summary>
    public event Func<IllustrationView> ThisRequired = null!;

    public IllustrationThumbnail() => InitializeComponent();

    // 这些方法本来用属性就可以实现，但在ViewModel更新的时候更新，使用了{x:Bind GetXXX(ViewModel)}的写法
    // 这样可以不需要写OnPropertyChange就实现更新
    #region XAML用的Get方法

    private double GetDesiredWidth(IllustrationViewModel viewModel)
    {
        var illustration = viewModel.Illustrate;
        var thumbnailUrlOption = ThisRequired.Invoke().IllustrationViewOption.ToThumbnailUrlOption();
        var thumbnailDirection = ThisRequired.Invoke().ThumbnailDirection;
        return thumbnailUrlOption is ThumbnailUrlOption.SquareMedium
            ? thumbnailDirection switch
            {
                ThumbnailDirection.Landscape => IllustrationView.PortraitHeight,
                ThumbnailDirection.Portrait => IllustrationView.LandscapeHeight,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
            }
            : thumbnailDirection switch
            {
                ThumbnailDirection.Landscape => IllustrationView.LandscapeHeight * illustration.Width / illustration.Height,
                ThumbnailDirection.Portrait => IllustrationView.PortraitHeight * illustration.Width / illustration.Height,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
            };
    }

#pragma warning disable IDE0060 // 删除未使用的参数
    private double GetDesiredHeight(IllustrationViewModel viewModel) => ThisRequired.Invoke().DesiredHeight;
#pragma warning restore IDE0060

    #endregion

    private ThumbnailUrlOption ThumbnailUrlOption => ThisRequired.Invoke().IllustrationViewOption.ToThumbnailUrlOption();

    /// <summary>
    /// For Bookmark Button
    /// </summary>
    private void IllustrationThumbnailContainerItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private async void ToggleBookmarkButtonOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        if (ViewModel.IsBookmarked)
            await ViewModel.RemoveBookmarkAsync();
        else
            await ViewModel.PostPublicBookmarkAsync();
    }

    private void ThumbnailOnTapped(object sender, TappedRoutedEventArgs e) => TappedOverride?.Invoke(this, e);

    private async void BookmarkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.SwitchBookmarkStateAsync();
    }

    private async void SaveContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
    }

    private async void SaveAsContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.SaveAsAsync();
    }

    private async void OpenInBrowserContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(ViewModel.Id));
    }

    private void AddToBookmarkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CopyWebLinkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustrationWebUri(ViewModel.Id).ToString());
    }

    private void CopyAppLinkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(ViewModel.Id).ToString());
    }

    private async void ShowQrCodeContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        var webQrCodeSource = await UiHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustrationWebUri(ViewModel.Id).ToString());
        ShowQrCodeRequested?.Invoke(this, webQrCodeSource);
    }

    private async void ShowPixEzQrCodeContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        var pixEzQrCodeSource = await UiHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustrationPixEzUri(ViewModel.Id).ToString());
        ShowQrCodeRequested?.Invoke(this, pixEzQrCodeSource);
    }
}
