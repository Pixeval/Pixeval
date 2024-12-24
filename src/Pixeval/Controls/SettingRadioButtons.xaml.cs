#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/SettingRadioButtons.xaml.cs
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

    public SettingRadioButtons() => InitializeComponent();

    private void RadioButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var select = sender.To<RadioButton>().GetTag<StringRepresentableItem>();
        if (!Equals(SelectedItem, select.Item))
        {
            SelectedItem = select.Item;
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs([], [select]));
        }
    }

    private void SettingRadioButtons_OnLoaded(object sender, RoutedEventArgs e)
    {
        SelectedItemChanged(this, SelectedItem);
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
