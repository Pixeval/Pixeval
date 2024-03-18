#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SortOptionComboBox.xaml.cs
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
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util;
using WinUI3Utilities.Attributes;
using System.Runtime.CompilerServices;

namespace Pixeval.Controls;

[DependencyProperty<WorkSortOption>("SelectedItem", propertyChanged: nameof(OnSelectedItemChanged))]
public sealed partial class SortOptionComboBox
{
    public event SelectionChangedEventHandler? SelectionChangedWhenLoaded;

    public SortOptionComboBox()
    {
        InitializeComponent();
        ComboBox.ItemsSource = LocalizedResourceAttributeHelper.GetLocalizedResourceContents<WorkSortOption>();
        SelectedItem = App.AppViewModel.AppSettings.DefaultSortOption;
    }

    public SortDescription? GetSortDescription()
    {
        return MakoHelper.GetSortDescriptionForIllustration(SelectedItem);
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (!Equals(e.NewValue, e.OldValue))
            SelectedItemChanged(sender, e.NewValue);
    }

    private void SortOptionComboBox_OnSelectionChangedWhenPrepared(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBox is not { SelectedItem: StringRepresentableItem { Item: WorkSortOption option } } || option == SelectedItem)
            return;
        SelectedItem = option;
        SelectionChangedWhenLoaded?.Invoke(this, e);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void SelectedItemChanged(DependencyObject d, object newValue)
    {
        if (d is not SortOptionComboBox { ComboBox: { } box } || newValue is not Enum e)
            return;

        box.SelectedItem = new StringRepresentableItem(e, null);
    }
}
