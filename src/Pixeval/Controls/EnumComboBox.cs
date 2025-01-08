// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
        var token = RegisterPropertyChangedCallback(ItemsSourceProperty, (sender, _) =>
        {
            if (sender is EnumComboBox { ItemsSource: IEnumerable<StringRepresentableItem> enumerable } box)
                box.SelectedItem = enumerable.FirstOrDefault();
        });
        Unloaded += (sender, _) => sender.To<DependencyObject>().UnregisterPropertyChangedCallback(ItemsSourceProperty, token);
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
            EnumValuesEnum.DownloadListOption => DownloadListOptionExtension.GetItems(),
            _ => ThrowHelper.ArgumentOutOfRange<EnumValuesEnum, object>(Type)
        };
    }
}

public enum EnumValuesEnum
{
    WorkType,
    SimpleWorkType,
    WorkSortOption,
    PrivacyPolicy,
    DownloadListOption
}
