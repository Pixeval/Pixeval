// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class PixevalBadge : UserControl
{
    [GeneratedDependencyProperty]
    public partial bool UseSmall { get; set; }

    [GeneratedDependencyProperty]
    public partial BadgeMode Mode { get; set; }

    internal const string SmallState = "Small";
    internal const string NormalState = "Normal";

    private static readonly Dictionary<BadgeMode, (string Text, Color Background)> _PropertySet = new()
    {
        [BadgeMode.None] = ("None", Colors.DarkGray),
        [BadgeMode.Premium] = ("Premium", Colors.Orange),
        [BadgeMode.Following] = (PixevalBadgeResources.Following, Colors.Crimson),
        [BadgeMode.Gif] = ("GIF", Colors.Green),
        [BadgeMode.R18] = ("R18", Colors.Crimson),
        [BadgeMode.R18G] = ("R18G", Colors.Crimson),
        [BadgeMode.Ai] = ("AI", Colors.Gray)
    };

    public PixevalBadge() => InitializeComponent();

    partial void OnUseSmallPropertyChanged(DependencyPropertyChangedEventArgs e) => GoToState(e.NewValue.To<bool>());

    private void GoToState(bool useSmall) => _ = VisualStateManager.GoToState(this, useSmall ? SmallState : NormalState, true);

#pragma warning disable CA1822
    private string GetText(BadgeMode mode) => _PropertySet[mode].Text;

    private SolidColorBrush GetBrush(BadgeMode mode) => new SolidColorBrush(_PropertySet[mode].Background);

    private Visibility GetVisibility(BadgeMode mode) =>
        mode is BadgeMode.None ? Visibility.Collapsed : Visibility.Visible;
#pragma warning restore CA1822
}
