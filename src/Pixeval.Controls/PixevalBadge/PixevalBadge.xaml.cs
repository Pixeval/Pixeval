using System.Collections.Generic;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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

    private static readonly Dictionary<BadgeMode, (string Text, Color Background)> _propertySet =
        new()
        {
            [BadgeMode.Premium] = ("Premium", Colors.Orange),
            [BadgeMode.Following] = ("关注中", Colors.Crimson),
            [BadgeMode.Gif] = ("GIF", Colors.Green),
            [BadgeMode.R18] = ("R18", Colors.Crimson),
            [BadgeMode.R18G] = ("R18G", Colors.Crimson),
        };

    public PixevalBadge()
    {
        InitializeComponent();
    }

    public static void OnUseSmallPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // d.To<PixevalBadge>().GoToState(e.NewValue.To<bool>());
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
        VisualStateManager.GoToState(this, useSmall ? SmallState : NormalState, true);
    }
}
