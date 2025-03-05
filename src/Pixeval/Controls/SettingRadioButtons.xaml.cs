// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class SettingRadioButtons : UserControl
{
    [GeneratedDependencyProperty]
    public partial object? ItemsSource { get; set; }

    [GeneratedDependencyProperty]
    public partial object SelectedItem { get; set; }

    [GeneratedDependencyProperty]
    public partial object? Header { get; set; }

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

    partial void OnItemsSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        Buttons.ItemsSource = ItemsSource;
    }

    partial void OnSelectedItemPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        SelectedItemChanged(this, e.NewValue);
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
