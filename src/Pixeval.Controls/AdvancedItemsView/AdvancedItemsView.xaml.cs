using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Pixeval.Collections;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// <see cref="ItemsView.ItemsSource"/>属性推荐使用<see cref="AdvancedObservableCollection{T}"/>类型，
/// 同时在<see cref="ItemContainer.Child"/>的控件使用<see cref="IViewModelControl"/>接口
/// </summary>
/// <remarks><see cref="ItemsView.ItemTemplate"/>中必须使用<see cref="ItemContainer"/>作为根元素</remarks>
[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default, nameof(OnLayoutTypeChanged))]
[DependencyProperty<double>("MinItemHeight", "0d", nameof(OnItemHeightChanged))]
[DependencyProperty<double>("MinItemWidth", "0d", nameof(OnItemWidthChanged))]
[DependencyProperty<double>("MinRowSpacing", "5d", nameof(OnMinRowSpacingChanged))]
[DependencyProperty<double>("MinColumnSpacing", "5d", nameof(OnMinColumnSpacingChanged))]
[DependencyProperty<double>("LoadingOffset", "100d")]
[DependencyProperty<int>("SelectedIndex", "-1", nameof(OnSelectedIndexChanged))]
[DependencyProperty<bool>("CanLoadMore", "true")]
[DependencyProperty<int>("LoadCount", "20")]
public sealed partial class AdvancedItemsView : ItemsView
{
    public event Func<AdvancedItemsView, EventArgs, Task> LoadMoreRequested;
    /// <summary>
    /// 调用此事件时可能需要防抖
    /// </summary>
    public event Action<AdvancedItemsView, ItemContainer>? ElementPrepared;
    public event Action<AdvancedItemsView, ItemContainer>? ElementClearing;

    /// <summary>
    /// 当加载本控件之前，若<see cref="ItemsView.ItemsSource"/>便已有内容，
    /// 则第一次加载就在视图里的控件可能不会触发<see cref="ItemsRepeater.ElementPrepared"/>，
    /// 故可以在<see cref="FrameworkElement.Loaded"/>时手动调用本方法来触发<see cref="ElementPrepared"/>
    /// </summary>
    /// <remarks><see cref="FrameworkElement.Loaded"/>触发在<see cref="ItemsRepeater.ElementPrepared"/>之后，
    /// 故不是第一次加载控件调用本方法后，不会多次触发<see cref="ElementPrepared"/>事件</remarks>
    /// <param name="element"></param>
    public void TryLoadedFirst(ItemContainer element)
    {
        _ = _loadedElements.Add(element.GetHashCode());
        ElementPrepared?.Invoke(this, element);
    }

    private readonly HashSet<int> _viewModels = [];

    /// <summary>
    /// 加载并在加载完毕后检查是否填满视区
    /// </summary>
    /// <remarks>
    /// 需要<see cref="ItemsView.ItemsSource"/>为<see cref="INotifyCollectionChanged"/>并且
    /// <see cref="ItemContainer.Child"/>是<see cref="IViewModelControl"/>的控件
    /// </remarks>
    /// <returns></returns>
    public async Task LoadAndFillAsync()
    {
        if (ItemsSource is not INotifyCollectionChanged ncc)
        {
            await TryRaiseLoadMoreRequestedAsync();
            return;
        }

        _viewModels.Clear();
        ncc.CollectionChanged += NccOnCollectionChanged;
        await TryRaiseLoadMoreRequestedAsync();
        ncc.CollectionChanged -= NccOnCollectionChanged;

        return;
        void NccOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is not NotifyCollectionChangedAction.Add || e.NewItems is null)
                return;

            foreach (var newItem in e.NewItems)
                _ = _viewModels.Add(newItem.GetHashCode());
        }
    }

    /// <summary>
    /// 判断滚动视图是否滚到底部，如果是则触发<see cref="LoadMoreRequested"/>事件
    /// </summary>
    /// <remarks>
    /// 需要<see cref="ItemsView.ItemsSource"/>为<see cref="INotifyCollectionChanged"/>
    /// </remarks>
    /// <returns></returns>
    public async Task TryRaiseLoadMoreRequestedAsync()
    {
        if (!CanLoadMore || IsLoadingMore)
            return;

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
                await LoadMoreRequested(this, EventArgs.Empty);
                IsLoadingMore = false;
                break;
            }
        }
    }

    public AdvancedItemsView()
    {
        InitializeComponent();
        LoadMoreRequested += async (sender, args) =>
        {
            if (sender.To<AdvancedItemsView>() is { ItemsSource: ISupportIncrementalLoading sil } aiv)
                _ = await sil.LoadMoreItemsAsync((uint)aiv.LoadCount);
        };
    }

    #region PropertyChanged

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

    private static void OnMinRowSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minRowSpacing = advancedItemsView.MinRowSpacing;
        switch (advancedItemsView.Layout)
        {
            case LinedFlowLayout linedFlowLayout:
                linedFlowLayout.LineSpacing = minRowSpacing;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinRowSpacing = minRowSpacing;
                break;
            case StackLayout stackLayout when advancedItemsView.LayoutType is ItemsViewLayoutType.VerticalStack:
                stackLayout.Spacing = minRowSpacing;
                break;
        }
    }

    private static void OnMinColumnSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minColumnSpacing = advancedItemsView.MinColumnSpacing;
        switch (advancedItemsView.Layout)
        {
            case LinedFlowLayout linedFlowLayout:
                linedFlowLayout.MinItemSpacing = minColumnSpacing;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinColumnSpacing = minColumnSpacing;
                break;
            case StackLayout stackLayout when advancedItemsView.LayoutType is ItemsViewLayoutType.HorizontalStack:
                stackLayout.Spacing = minColumnSpacing;
                break;
        }
    }

    private static void OnLayoutTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minItemHeight = advancedItemsView.MinItemHeight;
        var minItemWidth = advancedItemsView.MinItemWidth;
        var minRowSpacing = advancedItemsView.MinRowSpacing;
        var minColumnSpacing = advancedItemsView.MinColumnSpacing;
        advancedItemsView.Layout = advancedItemsView.LayoutType switch
        {
            ItemsViewLayoutType.LinedFlow => new LinedFlowLayout
            {
                ItemsStretch = LinedFlowLayoutItemsStretch.Fill,
                LineHeight = minItemHeight,
                LineSpacing = minRowSpacing,
                MinItemSpacing = minColumnSpacing,
            },
            ItemsViewLayoutType.Grid => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinColumnSpacing = minColumnSpacing,
                MinRowSpacing = minRowSpacing
            },
            ItemsViewLayoutType.VerticalUniformStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinColumnSpacing = minColumnSpacing,
                MinRowSpacing = minRowSpacing,
                Orientation = Orientation.Horizontal
            },
            ItemsViewLayoutType.HorizontalUniformStack => new UniformGridLayout
            {
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                MaximumRowsOrColumns = 1,
                MinItemHeight = minItemHeight,
                MinItemWidth = minItemWidth,
                MinColumnSpacing = minColumnSpacing,
                MinRowSpacing = minRowSpacing,
                Orientation = Orientation.Vertical
            },
            ItemsViewLayoutType.VerticalStack => new StackLayout
            {
                Spacing = minRowSpacing,
                Orientation = Orientation.Vertical
            },
            ItemsViewLayoutType.HorizontalStack => new StackLayout
            {
                Spacing = minColumnSpacing,
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

    #endregion

    #region EventHandlers

    private void AdvancedItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectionMode is not ItemsViewSelectionMode.Single)
            return;

        SelectedIndex = sender.To<AdvancedItemsView>().GetSelectedIndex();
    }

    private async void ScrollView_ViewChanged(ScrollView sender, object args) => await TryRaiseLoadMoreRequestedAsync();

    private void AdvancedItemsView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ScrollView.ViewChanged += ScrollView_ViewChanged;
        _itemsRepeater = ScrollView.Content.To<ItemsRepeater>();
        _itemsRepeater.ElementPrepared += async (s, arg) =>
        {
            // _loadedElements.Count is 0 说明不存在TryLoadedFirst注释里的情况
            // 否则需要检测该控件是否加载，加载后才能触发ElementPrepared，以此防止ElementPrepared调用两次
            var itemContainer = arg.Element.To<ItemContainer>();
            if (_loadedElements.Count is 0 || _loadedElements.Contains(arg.Element.GetHashCode()))
                ElementPrepared?.Invoke(this, itemContainer);
            // LoadAndFill用的逻辑
            if (_viewModels.Count is not 0 && itemContainer.Child is IViewModelControl viewModelControl)
            {
                _ = _viewModels.Remove(viewModelControl.ViewModel.GetHashCode());
                if (_viewModels.Count is 0)
                    await LoadAndFillAsync();
            }
        };
        _itemsRepeater.ElementClearing += (_, arg) => ElementClearing?.Invoke(this, arg.Element.To<ItemContainer>());
        // Loaded之后会触发SizeChanged
    }

    private async void AdvancedItemsView_OnSizeChanged(object sender, SizeChangedEventArgs e) => await LoadAndFillAsync();

    #endregion

    #region HelperMembers

    private ItemsRepeater _itemsRepeater = null!;

    private readonly HashSet<int> _loadedElements = [];

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

    /// <summary>
    /// For debounce
    /// </summary>
    private bool IsLoadingMore { get; set; }

    #endregion
}
