#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorProfile.xaml.cs
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
using System.Numerics;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<IllustratorViewModel>("ViewModel")]
public sealed partial class IllustratorProfile
{
    public static readonly Vector3 ZoomedScale = new(1.2f, 1.2f, 1.2f);
    public static readonly Vector3 CommonScale = new(1, 1, 1);
    public static readonly Vector3 ElevatedTranslation = new(0, 0, 60);
    public static readonly Vector3 CommonTranslation = new(0, 0, 30);
    public const float RotatedRotation = 10f;
    public const float CommonRotation = 0f;

    public event Func<TeachingTip>? RequestTeachingTip;

    public IllustratorProfile()
    {
        InitializeComponent();
    }

    private void AvatarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private Thickness GetIllustrationAt(List<SoftwareBitmapSource> sources)
    {
        BannerContainer.Children.Clear();
        var i = 0;
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var source in sources)
        {
            var image = new Image
            {
                Source = source,
                Stretch = Stretch.UniformToFill,
            };
            UIElementExtensions.SetClipToBounds(image, true);
            Grid.SetColumn(image, i);
            BannerContainer.Children.Add(image);
            ++i;
        }

        return new Thickness();
    }

    private void RestoreAvatarButton(object? sender, object e)
    {
        if (!AvatarButton.Flyout.IsOpen)
        {
            AvatarButton.Scale = CommonScale;
            AvatarButton.Translation = CommonTranslation;
            AvatarButton.Rotation = CommonRotation;
            // TODO: BlurEffect BlurOutAnimation.Start(Banner);
        }
    }

    private void GenerateLinkCommandOnExecuteRequested(object sender, RoutedEventArgs routedEventArgs)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustratorAppUri(ViewModel.UserId).ToString());
        RequestTeachingTip?.Invoke().ShowAndHide(IllustratorProfileResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(object sender, RoutedEventArgs routedEventArgs)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustratorWebUri(ViewModel.UserId).ToString());
        RequestTeachingTip?.Invoke().ShowAndHide(IllustratorProfileResources.LinkCopiedToClipboard);
    }
}
