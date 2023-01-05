using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.JustifiedLayout;

public record JustifiedListViewRowItemWrapper(IllustrationView.IllustrationViewModel Item, double LayoutWidth, double LayoutHeight)
{
    public IllustrationView.IllustrationViewModel Item { get; set; } = Item;

    public double LayoutWidth { get; set; } = LayoutWidth;

    public double LayoutHeight { get; set; } = LayoutHeight;

    public Thickness ItemBorderThickness(bool isSelected)
    {
        return new Thickness(isSelected ? 2 : 0);
    }
}

[DependencyProperty<double>("Spacing", DefaultValue = "10d")]
[DependencyProperty<ICollection<JustifiedListViewRowItemWrapper>>("ItemsSource", "OnItemsSourcePropertyChanged")]
[DependencyProperty<DataTemplate>("ItemTemplate")]
public sealed partial class JustifiedListViewRow
{
    private bool _refreshing;

    public JustifiedListViewRow()
    {
        InitializeComponent();
    }

    public UIElement? ContainerFromItem(IllustrationView.IllustrationViewModel item)
    {
        return Container.FindChildren().OfType<ContentControl>().FirstOrDefault(cc => cc.Content is JustifiedListViewRowItemWrapper wrapper && wrapper.Item.Equals(item));
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
                Content = wrapper,
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