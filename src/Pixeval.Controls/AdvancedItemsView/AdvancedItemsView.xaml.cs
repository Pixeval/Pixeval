using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default, nameof(OnLayoutTypeChanged))]
[DependencyProperty<double>("ItemHeight", "100d", nameof(OnItemHeightChanged))]
[DependencyProperty<double>("LoadingHeight", "100d")]
public sealed partial class AdvancedItemsView : ItemsView
{
    public event EventHandler? LoadMoreRequested;

    private static void OnItemHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var itemHeight = advancedItemsView.ItemHeight;
        switch (advancedItemsView.Layout)
        {
            case LinedFlowLayout linedFlowLayout:
                linedFlowLayout.LineHeight = itemHeight;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinItemHeight = itemHeight;
                break;
        }
    }

    private static void OnLayoutTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var itemHeight = advancedItemsView.ItemHeight;
        advancedItemsView.Layout = advancedItemsView.LayoutType switch
        {
            ItemsViewLayoutType.LinedFlow => new LinedFlowLayout
            {
                ItemsStretch = LinedFlowLayoutItemsStretch.Fill,
                LineHeight = itemHeight,
                LineSpacing = 5,
                MinItemSpacing = 5,
            },
            ItemsViewLayoutType.Grid => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MinColumnSpacing = 5,
                MinItemHeight = itemHeight,
                MinItemWidth = 180,
                MinRowSpacing = 5
            },
            ItemsViewLayoutType.VerticalStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinColumnSpacing = 5,
                MinItemHeight = itemHeight,
                MinItemWidth = 180,
                MinRowSpacing = 5,
                Orientation = Orientation.Horizontal
            },
            ItemsViewLayoutType.HorizontalStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinColumnSpacing = 5,
                MinItemHeight = itemHeight,
                MinItemWidth = 180,
                MinRowSpacing = 5,
                Orientation = Orientation.Vertical
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void TryRaiseLoadMoreRequested()
    {
        if (ScrollView.ScrollableHeight - LoadingHeight < ScrollView.VerticalOffset)
            LoadMoreRequested?.Invoke(this, EventArgs.Empty);
    }

    public AdvancedItemsView()
    {
        InitializeComponent();
    }

    private void ScrollViewViewChanged(ScrollView sender, object args) => TryRaiseLoadMoreRequested();

    private void AdvancedItemsView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ScrollView.ViewChanged += ScrollViewViewChanged;
        TryRaiseLoadMoreRequested();
    }
}
