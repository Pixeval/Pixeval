using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.JustifiedLayout;

public record JustifiedListViewRowItemWrapper(object Item, int LayoutWidth, int LayoutHeight)
{
    public object Item { get; set; } = Item;

    public int LayoutWidth { get; set; } = LayoutWidth;

    public int LayoutHeight { get; set; } = LayoutHeight;
}

[DependencyProperty<double>("Spacing", DefaultValue = "10")]
[DependencyProperty<ICollection<JustifiedListViewRowItemWrapper>>("ItemsSource", "OnItemsSourcePropertyChanged")]
[DependencyProperty<DataTemplate>("ItemTemplate")]
public sealed partial class JustifiedListViewRow
{
    private bool _refreshing;

    public JustifiedListViewRow()
    {
        InitializeComponent();
    }

    private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is JustifiedListViewRow row && e.NewValue is ICollection<JustifiedListViewRowItemWrapper> { Count: > 0 } collection && !row._refreshing)
        {
            row._refreshing = true;
            row.Container.Children.Clear();
            row.Container.ColumnDefinitions.Clear();

            row.Container.Height = collection.First().LayoutHeight + row.Spacing;

            var columns = collection.Select(wrapper => new ContentControl
            {
                ContentTemplate = row.ItemTemplate,
                Content = wrapper.LayoutHeight,
                VerticalAlignment = VerticalAlignment.Center
            });
            foreach (var (index, (column, (_, layoutWidth, _))) in columns.Zip(collection).Indexed())
            {
                row.Container.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(index == 0 ? layoutWidth : layoutWidth + row.Spacing)
                });

                if (index != 0)
                {
                    column.HorizontalAlignment = HorizontalAlignment.Right;
                    column.VerticalAlignment = VerticalAlignment.Center;
                }

                Grid.SetColumn(column, index);
                row.Container.Children.Add(column);
            }

            row._refreshing = false;
        }
    }
}