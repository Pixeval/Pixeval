using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default, nameof(OnLayoutTypeChanged))]
[DependencyProperty<double>("MinItemHeight", "0d", nameof(OnItemHeightChanged))]
[DependencyProperty<double>("MinItemWidth", "0d", nameof(OnItemWidthChanged))]
[DependencyProperty<double>("LoadingOffset", "100d")]
[DependencyProperty<int>("SelectedIndex", "-1", nameof(OnSelectedIndexChanged))]
public sealed partial class AdvancedItemsView : ItemsView
{
    private ItemsRepeater _itemsRepeater = null!;

    public event Func<AdvancedItemsView, EventArgs, Task>? LoadMoreRequested;
    public event Action<AdvancedItemsView, ScrollView>? ViewChanged;
    // TODO: 调用此事件时可能需要防抖
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
                MinColumnSpacing = 5,
                Orientation = Orientation.Horizontal
            },
            ItemsViewLayoutType.HorizontalUniformStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinRowSpacing = 5,
                MinColumnSpacing = 5,
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
            _ => ThrowHelper.ArgumentOutOfRange<ItemsViewLayoutType, VirtualizingLayout>(advancedItemsView.LayoutType)
        };
    }

    private static void OnSelectedIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var selectedIndex = advancedItemsView.GetSelectedIndex();
        if (advancedItemsView.SelectedIndex != selectedIndex)
            advancedItemsView.Select(advancedItemsView.SelectedIndex);
    }

    private readonly HashSet<int> _loadedElements = [];

    /// <summary>
    /// 当加载本控件之前，<see cref="ItemsView.ItemsSource"/>便已有内容，
    /// 则<see cref="ItemsRepeater.ElementPrepared"/>可能不会触发，故
    /// 在<see cref="FrameworkElement.Loaded"/>时手动触发
    /// </summary>
    /// <param name="element"></param>
    public void TryLoadedFirst(ItemContainer element)
    {
        _ = _loadedElements.Add(element.GetHashCode());
        ElementPrepared?.Invoke(this, element);
    }

    public async void TryRaiseLoadMoreRequested()
    {
        if (LoadMoreRequested is { } handler && !IsLoadingMore)
            switch (ScrollView.ComputedVerticalScrollMode, ScrollView.ComputedHorizontalScrollMode)
            {
                case (ScrollingScrollMode.Disabled, ScrollingScrollMode.Disabled):
                case (ScrollingScrollMode.Enabled, ScrollingScrollMode.Disabled)
                    when ScrollView.ScrollableHeight - LoadingOffset < ScrollView.VerticalOffset:
                case (ScrollingScrollMode.Disabled, ScrollingScrollMode.Enabled)
                    when ScrollView.ScrollableWidth - LoadingOffset < ScrollView.HorizontalOffset:
                case (ScrollingScrollMode.Enabled, ScrollingScrollMode.Enabled)
                    when ScrollView.ScrollableHeight - LoadingOffset < ScrollView.VerticalOffset
                        || ScrollView.ScrollableWidth - LoadingOffset < ScrollView.HorizontalOffset:
                {
                    IsLoadingMore = true;
                    await handler(this, EventArgs.Empty);
                    IsLoadingMore = false;
                    break;
                }
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
        _itemsRepeater.ElementPrepared += (_, arg) =>
        {
            if (_loadedElements.Count is 0 || _loadedElements.Contains(arg.Element.GetHashCode()))
                ElementPrepared?.Invoke(this, arg.Element.To<ItemContainer>());
        };
        _itemsRepeater.ElementClearing += (_, arg) => ElementClearing?.Invoke(this, arg.Element.To<ItemContainer>());
        TryRaiseLoadMoreRequested();
    }

    private int GetSelectedIndex()
    {
        return SelectedItem switch
        {
            null => -1,
            _ => ItemsSource switch
            {
                Array array => Array.IndexOf(array, SelectedItem),
                IList list => list.IndexOf(SelectedItem),
                IEnumerable enumerable => enumerable.Cast<object>().ToList().IndexOf(SelectedItem),
                _ => ThrowHelper.ArgumentOutOfRange<object, int>(ItemsSource)
            }
        };
    }

    private void AdvancedItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectionMode is not ItemsViewSelectionMode.Single)
            return;

        SelectedIndex = sender.To<AdvancedItemsView>().GetSelectedIndex();
    }
}
