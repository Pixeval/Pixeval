using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.UserControls.Setting.UI.Model;
using Windows.Foundation;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.Setting.UI;

[DependencyProperty<IEnumerable<StringRepresentableItem>>("ItemsSource")]
[DependencyProperty<object>("SelectedItem", propertyChanged: nameof(OnSelectedItemChanged))]
public sealed partial class SettingRadioButtons : UserControl
{
    private RadioButtons Buttons => Content.To<RadioButtons>();

    public SettingRadioButtons() => InitializeComponent();

    private void RadioButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var select = sender.To<RadioButton>().GetDataContext<StringRepresentableItem>();
        if (!Equals(SelectedItem, select.Item))
        {
            SelectedItem = select.Item;
            SelectionChanged?.Invoke(this,
                new(new List<object>(), new List<object> { select }));
        }
    }

    private void SettingRadioButtons_OnLoaded(object sender, RoutedEventArgs e)
    {
        SelectedItemChanged(this, SelectedItem);
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        SelectedItemChanged(sender, e.NewValue);
    }

    public event TypedEventHandler<SettingRadioButtons, SelectionChangedEventArgs>? SelectionChanged;

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void SelectedItemChanged(DependencyObject d, object newValue)
    {
        if (d is not SettingRadioButtons { Buttons: { } buttons, ItemsSource: { } itemsSource })
            return;

        var correspondingItem = itemsSource.First(r => Equals(r.Item, newValue));
        // set RadioButtons.SelectedItem won't work
        buttons.FindDescendants().OfType<RadioButton>().First(b => b.GetDataContext<StringRepresentableItem>() == correspondingItem).IsChecked = true;
    }
}
