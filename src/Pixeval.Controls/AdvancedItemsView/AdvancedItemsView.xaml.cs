using System;
using System.Threading.Tasks;
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
    public event Func<AdvancedItemsView, EventArgs, Task>? LoadMoreRequested;

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

    public async void TryRaiseLoadMoreRequested()
    {
        if (LoadMoreRequested is { } handler && !IsLoadingMore)
            if (ScrollView.ScrollableHeight - LoadingHeight < ScrollView.VerticalOffset)
            {
                IsLoadingMore = true;
                await handler(this, EventArgs.Empty);
                IsLoadingMore = false;
            }
    }

    private bool IsLoadingMore { get; set; }

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
