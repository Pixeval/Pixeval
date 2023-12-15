using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default, nameof(OnLayoutTypeChanged))]
[DependencyProperty<double>("MinItemHeight", "0d", nameof(OnItemHeightChanged))]
[DependencyProperty<double>("MinItemWidth", "0d", nameof(OnItemWidthChanged))]
[DependencyProperty<double>("LoadingHeight", "100d")]
public sealed partial class AdvancedItemsView : ItemsView
{
    private ItemsRepeater _itemsRepeater = null!;

    public event Func<AdvancedItemsView, EventArgs, Task>? LoadMoreRequested;
    public event Action<AdvancedItemsView, ScrollView>? ViewChanged;
    public event Action<AdvancedItemsView, ItemContainer>? ElementPrepared;
    public event Action<AdvancedItemsView, ItemContainer>? ElementClearing;

    private static void OnItemHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minItemHeight = advancedItemsView.MinItemHeight;
        switch (advancedItemsView.Layout)
        {
            case LinedFlowLayout linedFlowLayout:
                linedFlowLayout.LineHeight = minItemHeight;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinItemHeight = minItemHeight;
                break;
        }
    }

    private static void OnItemWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minItemWidth = advancedItemsView.MinItemWidth;
        if (advancedItemsView.Layout is UniformGridLayout uniformGridLayout)
        {
            uniformGridLayout.MinItemWidth = minItemWidth;
        }
    }

    private static void OnLayoutTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minItemHeight = advancedItemsView.MinItemHeight;
        var minItemWidth = advancedItemsView.MinItemWidth;
        advancedItemsView.Layout = advancedItemsView.LayoutType switch
        {
            ItemsViewLayoutType.LinedFlow => new LinedFlowLayout
            {
                ItemsStretch = LinedFlowLayoutItemsStretch.Fill,
                LineHeight = minItemHeight,
                LineSpacing = 5,
                MinItemSpacing = 5,
            },
            ItemsViewLayoutType.Grid => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MinColumnSpacing = 5,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinRowSpacing = 5
            },
            ItemsViewLayoutType.VerticalUniformStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinRowSpacing = 5,
                Orientation = Orientation.Horizontal
            },
            ItemsViewLayoutType.HorizontalUniformStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinRowSpacing = 5,
                Orientation = Orientation.Vertical
            },
            ItemsViewLayoutType.VerticalStack => new StackLayout
            {
                Spacing = 5,
                Orientation = Orientation.Vertical
            },
            ItemsViewLayoutType.HorizontalStack => new StackLayout
            {
                Spacing = 5,
                Orientation = Orientation.Horizontal
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

        ViewChanged?.Invoke(this, ScrollView);
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
        _itemsRepeater = ScrollView.Content.To<ItemsRepeater>();
        _itemsRepeater.ElementPrepared += (_, arg) => ElementPrepared?.Invoke(this, arg.Element.To<ItemContainer>());
        _itemsRepeater.ElementClearing += (_, arg) => ElementClearing?.Invoke(this, arg.Element.To<ItemContainer>());
        TryRaiseLoadMoreRequested();
    }
}
