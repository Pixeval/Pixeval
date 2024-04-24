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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Options;
using Windows.Foundation;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<IllustrationItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class IllustrationItem
{
    public event TypedEventHandler<IllustrationItem, IllustrationItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<IllustrationItem, IllustrationItemViewModel>? RequestAddToBookmark;

    public event Func<(ThumbnailDirection ThumbnailDirection, double DesiredHeight)> RequiredParam = null!;

    public event Func<TeachingTip> RequestTeachingTip = null!;

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IllustrationItem illustrationItem)
        {
            illustrationItem.ViewModelChanged?.Invoke(illustrationItem, illustrationItem.ViewModel);
        }
    }

    public IllustrationItem() => InitializeComponent();

    private double DesiredHeight => RequiredParam().DesiredHeight;

    private TeachingTip QrCodeTeachingTip => RequestTeachingTip();

    /// <summary>
    /// 这些方法本来用属性就可以实现，但在ViewModel更新的时候更新，使用了{x:Bind GetXXX(ViewModel)}的写法。
    /// 这样可以不需要写OnPropertyChanged就实现更新
    /// </summary>
    private double GetDesiredWidth(IllustrationItemViewModel viewModel)
    {
        var illustration = viewModel.Entry;
        var thumbnailDirection = RequiredParam().ThumbnailDirection;
        return thumbnailDirection switch
        {
            ThumbnailDirection.Landscape => WorkView.LandscapeHeight * illustration.Width / illustration.Height,
            ThumbnailDirection.Portrait => WorkView.PortraitHeight * illustration.Width / illustration.Height,
            _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
        };
    }

    private void AddToBookmark_OnTapped(object sender, RoutedEventArgs e)
    {
        RequestAddToBookmark?.Invoke(this, ViewModel);
    }
}
