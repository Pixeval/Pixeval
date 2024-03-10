using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Options;
using WinUI3Utilities.Attributes;
using Pixeval.Util;
using System.Runtime.CompilerServices;

namespace Pixeval.Controls;

[DependencyProperty<SimpleWorkType>("SelectedItem", propertyChanged: nameof(OnSelectedItemChanged))]
public sealed partial class SimpleWorkTypeComboBox : UserControl
{
    public event SelectionChangedEventHandler? SelectionChangedWhenLoaded;

    public SimpleWorkTypeComboBox()
    {
        InitializeComponent();
        ComboBox.ItemsSource = LocalizedResourceAttributeHelper.GetLocalizedResourceContents<SimpleWorkType>();
        SelectedItem = SimpleWorkType.IllustAndManga;
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (!Equals(e.NewValue, e.OldValue))
            SelectedItemChanged(sender, e.NewValue);
    }

    private void WorkTypeComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not NotifyOnLoadedComboBox { SelectedItem: StringRepresentableItem { Item: SimpleWorkType option } } || option == SelectedItem)
            return;
        SelectedItem = option;
        SelectionChangedWhenLoaded?.Invoke(this, e);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void SelectedItemChanged(DependencyObject d, object newValue)
    {
        if (d is not SimpleWorkTypeComboBox { ComboBox: { } box } || newValue is not Enum e)
            return;

        box.SelectedItem = new StringRepresentableItem(e, null);
    }
}
