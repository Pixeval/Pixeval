using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// <see cref="IsLoadingMore"/>将覆盖<see cref="HasNoItem"/>的效果
/// </summary>
[DependencyProperty<bool>("HasNoItem", propertyChanged: nameof(OnHasNoItemChanged))]
[DependencyProperty<bool>("IsLoadingMore", propertyChanged: nameof(OnHasNoItemChanged))]
[DependencyProperty<object>("Content")]
[DependencyProperty<string>("TeachingTipTitle")]
public sealed partial class IllustrateView
{
    public IllustrateView() => InitializeComponent();

    private static void OnHasNoItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (IllustrateView)d;
        control.HasNoItemStackPanel.Visibility = control is { HasNoItem: true, IsLoadingMore: false } ? Visibility.Visible : Visibility.Collapsed;
    }
}
