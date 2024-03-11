using System;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<WorkType>("SelectedItem", propertyChanged: nameof(OnSelectedItemChanged))]
public sealed partial class WorkTypeComboBox : UserControl
{
    public event SelectionChangedEventHandler? SelectionChangedWhenLoaded;

    public WorkTypeComboBox()
    {
        InitializeComponent();
        ComboBox.ItemsSource = LocalizedResourceAttributeHelper.GetLocalizedResourceContents<WorkType>();
        SelectedItem = WorkType.Illust;
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (!Equals(e.NewValue, e.OldValue))
            SelectedItemChanged(sender, e.NewValue);
    }

    private void WorkTypeComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not NotifyOnLoadedComboBox { SelectedItem: StringRepresentableItem { Item: WorkType option } } || option == SelectedItem)
            return;
        SelectedItem = option;
        SelectionChangedWhenLoaded?.Invoke(this, e);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void SelectedItemChanged(DependencyObject d, object newValue)
    {
        if (d is not WorkTypeComboBox { ComboBox: { } box } || newValue is not Enum e)
            return;

        box.SelectedItem = new StringRepresentableItem(e, null);
    }
}
