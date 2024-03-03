using System;
using Microsoft.UI.Xaml;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<PrivacyPolicy>("SelectedItem", propertyChanged: nameof(OnSelectedItemChanged))]
public sealed partial class PrivacyPolicyComboBox
{
    public event SelectionChangedEventHandler? SelectionChangedWhenLoaded;

    public PrivacyPolicyComboBox()
    {
        InitializeComponent();
        ComboBox.ItemsSource = LocalizedResourceAttributeHelper.GetLocalizedResourceContents<PrivacyPolicy>();
        SelectedItem = PrivacyPolicy.Public;
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (!Equals(e.NewValue, e.OldValue))
            SelectedItemChanged(sender, e.NewValue);
    }

    private void PrivacyPolicyComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not NotifyOnLoadedComboBox { SelectedItem: StringRepresentableItem { Item: PrivacyPolicy option } } || option == SelectedItem)
            return;
        SelectedItem = option;
        SelectionChangedWhenLoaded?.Invoke(this, e);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void SelectedItemChanged(DependencyObject d, object newValue)
    {
        if (d is not PrivacyPolicyComboBox { ComboBox: { } box } || newValue is not Enum e)
            return;

        box.SelectedItem = new StringRepresentableItem(e, null);
    }
}
