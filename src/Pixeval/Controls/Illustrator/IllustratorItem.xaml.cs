#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorItem.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Attributes;
using Pixeval.Controls.Windowing;

namespace Pixeval.Controls;

[DependencyProperty<IllustratorItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class IllustratorItem
{
    public event Action<IllustratorItem, IllustratorItemViewModel>? ViewModelChanged;

    public event Func<TeachingTip> RequestTeachingTip = null!;

    public ulong HWnd => WindowFactory.GetWindowForElement(this).HWnd;

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d as IllustratorItem is { } item)
        {
            item.ViewModelChanged?.Invoke(item, item.ViewModel);
        }
    }

    private const float RotatedRotation = 10f;
    private const float CommonRotation = 0f;

    private static readonly Vector3 _zoomedScale = new(1.2f, 1.2f, 1.2f);
    private static readonly Vector3 _commonScale = new(1, 1, 1);
    private static readonly Vector3 _elevatedTranslation = new(0, 0, 60);
    private static readonly Vector3 _commonTranslation = new(0, 0, 30);

    private TeachingTip QrCodeTeachingTip => RequestTeachingTip();

    public IllustratorItem() => InitializeComponent();

    private void AvatarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void RestoreAvatarButton(object? sender, object e)
    {
        if (!AvatarButton.Flyout.IsOpen)
        {
            AvatarButton.Scale = _commonScale;
            AvatarButton.Translation = _commonTranslation;
            AvatarButton.Rotation = CommonRotation;
        }
    }
}
