#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/PixevalBadge.xaml.cs
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
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<bool>("UseSmall", "false", nameof(OnUseSmallPropertyChanged))]
[DependencyProperty<string>("Text")]
[DependencyProperty<Brush>("BadgeColor")]
[DependencyProperty<BadgeMode>("Mode", propertyChanged: nameof(OnBadgeModePropertyChanged))]
public sealed partial class PixevalBadge : UserControl
{
    internal const string SmallState = "Small";
    internal const string NormalState = "Normal";

    private static readonly Dictionary<BadgeMode, (string Text, Color Background)> _propertySet = new()
    {
        [BadgeMode.Premium] = ("Premium", Colors.Orange),
        [BadgeMode.Following] = (PixevalBadgeResources.Following, Colors.Crimson),
        [BadgeMode.Gif] = ("GIF", Colors.Green),
        [BadgeMode.R18] = ("R18", Colors.Crimson),
        [BadgeMode.R18G] = ("R18G", Colors.Crimson)
    };

    public PixevalBadge() => InitializeComponent();

    public static void OnUseSmallPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<PixevalBadge>().GoToState(e.NewValue.To<bool>());
    }

    public static void OnBadgeModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var properties = _propertySet[e.NewValue.To<BadgeMode>()];
        var badge = d.To<PixevalBadge>();
        badge.Text = properties.Text;
        badge.BadgeColor = new SolidColorBrush(properties.Background);
    }

    private void GoToState(bool useSmall)
    {
        _ = VisualStateManager.GoToState(this, useSmall ? SmallState : NormalState, true);
    }
}

static file class PixevalBadgeResources
{
    private static readonly ResourceLoader _resourceLoader = ResourceHelper.GetResourceLoader("PixevalBadge");

    public static string Following { get; } = _resourceLoader.GetString("Following");
}
