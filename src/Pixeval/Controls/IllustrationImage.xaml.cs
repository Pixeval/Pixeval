#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationImage.xaml.cs
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
using System.Collections.Immutable;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.IllustrationView;
using Pixeval.Options;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<IllustrationItemViewModel>("ViewModel")]
[DependencyProperty<ThumbnailUrlOption>("ThumbnailOption")]
public sealed partial class IllustrationImage : UserControl, IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public IllustrationImage() => InitializeComponent();

    /// <summary>
    /// 这个方法用来刷新获取缩略图属性
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    private SoftwareBitmapSource? GetThumbnailSource(ImmutableDictionary<ThumbnailUrlOption, SoftwareBitmapSource> dictionary, ThumbnailUrlOption option)
        => CollectionExtensions.GetValueOrDefault(dictionary, option);
}
