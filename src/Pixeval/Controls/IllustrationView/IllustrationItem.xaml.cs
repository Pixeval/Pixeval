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
using System.Collections.Generic;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.IllustrationView;

[DependencyProperty<IllustrationItemViewModel>("ViewModel")]
public sealed partial class IllustrationItem : IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public IllustrationItem() => InitializeComponent();

    private ThumbnailUrlOption ThumbnailUrlOption => ThisRequired.Invoke().LayoutType.ToThumbnailUrlOption();

    private double DesiredHeight => ThisRequired.Invoke().DesiredHeight;

    /// <summary>
    /// 请求显示二维码
    /// </summary>
    public event Func<TeachingTip>? TeachingTipRequested;

    /// <summary>
    /// 请求获取承载本控件的<see cref="IllustrationView"/>
    /// </summary>
    public event Func<IllustrationView> ThisRequired = null!;

    private async void ToggleBookmarkButtonOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        await ViewModel.ToggleBookmarkStateAsync();
    }

    private async void BookmarkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ToggleBookmarkStateAsync();
    }

    private async void SaveContextItemOnTapped(object sender, RoutedEventArgs routedEventArgs)
    {
        await ViewModel.SaveAsync();
    }

    private async void OpenInBrowserContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(ViewModel.Id));
    }

    private void AddToBookmarkContextItemOnTapped(object sender, TappedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private TeachingTip? GetQrCodeTeachingTip() => TeachingTipRequested?.Invoke();

    // 这些方法本来用属性就可以实现，但在ViewModel更新的时候更新，使用了{x:Bind GetXXX(ViewModel)}的写法
    // 这样可以不需要写OnPropertyChange就实现更新

    #region XAML用的Get方法

    private double GetDesiredWidth(IllustrationItemViewModel viewModel)
    {
        var illustration = viewModel.Illustrate;
        var thumbnailDirection = ThisRequired.Invoke().ThumbnailDirection;
        return ThumbnailUrlOption is ThumbnailUrlOption.SquareMedium
            ? thumbnailDirection switch
            {
                ThumbnailDirection.Landscape => IllustrationView.PortraitHeight,
                ThumbnailDirection.Portrait => IllustrationView.LandscapeHeight,
                _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
            }
            : thumbnailDirection switch
            {
                ThumbnailDirection.Landscape => IllustrationView.LandscapeHeight * illustration.Width / illustration.Height,
                ThumbnailDirection.Portrait => IllustrationView.PortraitHeight * illustration.Width / illustration.Height,
                _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
            };
    }

    private Visibility IsImageLoaded(IDictionary<ThumbnailUrlOption, SoftwareBitmapSource> dictionary) => dictionary.ContainsKey(ThumbnailUrlOption) ? Visibility.Collapsed : Visibility.Visible;

    #endregion
}
