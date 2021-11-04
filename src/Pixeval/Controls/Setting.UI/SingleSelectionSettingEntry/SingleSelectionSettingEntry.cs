#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SingleSelectionSettingEntry.cs
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
using Windows.Foundation;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Setting.UI.Model;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.SingleSelectionSettingEntry
{
    [TemplatePart(Name = PartSelectorRadioButtons, Type = typeof(RadioButtons))]
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    public sealed class SingleSelectionSettingEntry : SettingEntryBase
    {
        private const string PartSelectorRadioButtons = "SelectorRadioButtons";

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            nameof(HeaderHeight),
            typeof(double),
            typeof(SingleSelectionSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable<IStringRepresentableItem>),
            typeof(SingleSelectionSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(SingleSelectionSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => SelectedItemChanged(o, args.NewValue)));

        private TypedEventHandler<SingleSelectionSettingEntry, SelectionChangedEventArgs>? _selectionChanged;

        private RadioButtons? _selectorRadioButtons;

        public SingleSelectionSettingEntry()
        {
            DefaultStyleKey = typeof(SingleSelectionSettingEntry);
        }

        public double HeaderHeight
        {
            get => (double) GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public IEnumerable<IStringRepresentableItem> ItemsSource
        {
            get => (IEnumerable<IStringRepresentableItem>) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public event TypedEventHandler<SingleSelectionSettingEntry, SelectionChangedEventArgs> SelectionChanged
        {
            add => _selectionChanged += value;
            remove => _selectionChanged -= value;
        }

        protected override void IconChanged(object? newValue)
        {
            if (_selectorRadioButtons is not null)
            {
                _selectorRadioButtons.Margin = newValue is IconElement
                    ? new Thickness(50, 0, 50, 0)
                    : new Thickness(10, 0, 10, 0);
            }

            base.IconChanged(newValue);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void SelectedItemChanged(DependencyObject d, object newValue)
        {
            if (d is SingleSelectionSettingEntry { _selectorRadioButtons: { } buttons, ItemsSource: { } itemsSource })
            {
                var correspondingItem = itemsSource.First(r => r.Item.Equals(newValue));
                // set RadioButtons.SelectedItem won't work
                buttons.FindDescendants().OfType<RadioButton>().First(b => b.DataContext.Equals(correspondingItem)).IsChecked = true;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SelectorRadioButtonsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectorRadioButtons is { SelectedItem: IStringRepresentableItem item })
            {
                SelectedItem = item.Item;
            }

            _selectionChanged?.Invoke(this, e);
        }

        protected override void Update()
        {
            SelectedItemChanged(this, SelectedItem);
        }

        protected override void OnApplyTemplate()
        {
            if (_selectorRadioButtons is not null)
            {
                _selectorRadioButtons.SelectionChanged -= SelectorRadioButtonsOnSelectionChanged;
            }

            if ((_selectorRadioButtons = GetTemplateChild(PartSelectorRadioButtons) as RadioButtons) is not null)
            {
                _selectorRadioButtons.SelectionChanged += SelectorRadioButtonsOnSelectionChanged;
            }

            base.OnApplyTemplate();
        }
    }
}