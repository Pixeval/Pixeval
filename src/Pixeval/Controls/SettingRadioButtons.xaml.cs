// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using CommunityToolkit.WinUI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<object>("ItemsSource", DependencyPropertyDefaultValue.Default, nameof(OnItemsSourceChanged))]
[DependencyProperty<object>("SelectedItem", propertyChanged: nameof(OnSelectedItemChanged))]
[DependencyProperty<object>("Header")]
public sealed partial class SettingRadioButtons : UserControl
{
    private RadioButtons Buttons => Content.To<RadioButtons>();

    public SettingRadioButtons()
    {
        InitializeComponent();
        SelectedItemChanged(this, SelectedItem);
    }

    private void RadioButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var select = sender.To<RadioButton>().GetTag<StringRepresentableItem>();
        if (!Equals(SelectedItem, select.Item))
        {
            SelectedItem = select.Item;
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs([], [select]));
        }
    }

    private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var buttons = sender.To<SettingRadioButtons>();

        buttons.Buttons.ItemsSource = buttons.ItemsSource;
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        SelectedItemChanged(sender, e.NewValue);
    }

    public event TypedEventHandler<SettingRadioButtons, SelectionChangedEventArgs>? SelectionChanged;

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void SelectedItemChanged(DependencyObject d, object newValue)
    {
        if (d is not SettingRadioButtons { Buttons: { } buttons, ItemsSource: not null })
            return;
        var correspondingItem = buttons.ItemsSource.To<IEnumerable<StringRepresentableItem>>().First(r => Equals(r.Item, newValue));
        // set RadioButtons.SelectedItem won't work
        foreach (var button in buttons.FindDescendants().OfType<RadioButton>())
            if (button.GetTag<StringRepresentableItem>().Equals(correspondingItem))
                button.IsChecked = true;
    }
}
