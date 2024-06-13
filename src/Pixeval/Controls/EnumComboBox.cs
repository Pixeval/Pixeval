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
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Pixeval.CoreApi.Global.Enum;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<object>("SelectedEnum", DependencyPropertyDefaultValue.Default, nameof(OnSelectedEnumChanged), IsNullable = true)]
public sealed partial class EnumComboBox : ComboBox
{
    public new event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    public EnumComboBox()
    {
        Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;
        base.SelectionChanged += ComboBox_SelectionChanged;
    }

    public bool RaiseEventAfterLoaded { get; set; } = true;

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var newEnum = SelectedItem is StringRepresentableItem { Item: { } option } ? option : null;
        if (Equals(newEnum, SelectedEnum))
            return;
        SelectedEnum = newEnum;
        if (RaiseEventAfterLoaded && !IsLoaded)
            return;
        SelectionChanged?.Invoke(this, new([], []));
    }

    public bool ItemSelected => SelectedItem is not null;

    public T GetSelectedItem<T>() where T : Enum => SelectedEnum is T t ? t : ThrowHelper.InvalidCast<T>();

    private static void OnSelectedEnumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var comboBox = sender.To<EnumComboBox>();
        comboBox.SelectedItem = comboBox.ItemsSource?.To<IEnumerable<StringRepresentableItem>>().FirstOrDefault(r => Equals(r.Item, comboBox.SelectedEnum));
    }
}

[MarkupExtensionReturnType(ReturnType = typeof(IReadOnlyList<StringRepresentableItem>))]
public sealed partial class EnumValuesExtension : MarkupExtension
{
    public EnumValuesEnum Type { get; set; }

    protected override object ProvideValue()
    {
        return Type switch
        {
            EnumValuesEnum.WorkType => WorkTypeExtension.GetItems(),
            EnumValuesEnum.SimpleWorkType => SimpleWorkTypeExtension.GetItems(),
            EnumValuesEnum.WorkSortOption => WorkSortOptionExtension.GetItems(),
            EnumValuesEnum.PrivacyPolicy => PrivacyPolicyExtension.GetItems(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum EnumValuesEnum
{
    WorkType,
    SimpleWorkType,
    WorkSortOption,
    PrivacyPolicy
}
