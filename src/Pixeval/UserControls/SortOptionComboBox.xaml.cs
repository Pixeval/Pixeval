#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SortOptionComboBox.xaml.cs
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

using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Options;
using Pixeval.Util;

namespace Pixeval.UserControls;

public sealed partial class SortOptionComboBox
{
    public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(SortOptionComboBox),
        PropertyMetadata.Create(DependencyProperty.UnsetValue));

    private SelectionChangedEventHandler? _selectionChangedWhenLoadedInternal;

    public SortOptionComboBox()
    {
        InitializeComponent();
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set
        {
            SetValue(SelectedItemProperty, value);
            ComboBox.SelectedItem = value;
        }
    }

    public IllustrationSortOption SelectedOption => ((IllustrationSortOptionWrapper) SelectedItem).Value;

    public event SelectionChangedEventHandler SelectionChangedWhenLoaded
    {
        add => _selectionChangedWhenLoadedInternal += value;
        remove => _selectionChangedWhenLoadedInternal -= value;
    }

    public SortDescription? GetSortDescription()
    {
        return MakoHelper.GetSortDescriptionForIllustration(SelectedOption);
    }

    private void SortOptionComboBox_OnSelectionChangedWhenPrepared(object sender, SelectionChangedEventArgs e)
    {
        SelectedItem = ComboBox.SelectedItem;
        _selectionChangedWhenLoadedInternal?.Invoke(sender, e);
    }
}