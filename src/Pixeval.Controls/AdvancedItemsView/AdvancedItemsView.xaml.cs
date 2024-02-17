using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Pixeval.Collections;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// <see cref="ItemsView.ItemsSource"/>属性推荐使用<see cref="AdvancedObservableCollection{T}"/>类型
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

    /// <summary>
    /// 判断滚动视图是否滚到底部，如果是则触发<see cref="LoadMoreRequested"/>事件，
    /// 这个事件只会使源加载最多一次
    /// </summary>
    /// <remarks>
    /// 需要<see cref="ItemsView.ItemsSource"/>为<see cref="INotifyCollectionChanged"/>
    /// </remarks>
    /// <returns></returns>
    public async Task TryRaiseLoadMoreRequestedAsync()
    {
        if (!CanLoadMore || IsLoadingMore)
            return;

        // 只有页面没充满时会触发LoadMoreRequested
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
        SelectionChanged += AdvancedItemsViewOnSelectionChanged;
        LoadMoreRequested += async (sender, args) =>
        {
            if (sender.To<AdvancedItemsView>() is { ItemsSource: ISupportIncrementalLoading sil } aiv)
                _ = await sil.LoadMoreItemsAsync((uint)aiv.LoadCount);
        };
        _ = RegisterPropertyChangedCallback(ScrollViewProperty, ScrollViewOnPropertyChanged);
        _ = RegisterPropertyChangedCallback(ItemsSourceProperty, ItemsSourceOnPropertyChanged);
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

    /// <summary>
    /// 本方法之后会触发<see cref="AdvancedItemsViewOnSizeChanged"/>
    /// </summary>
    private void ScrollViewOnPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        ScrollView.ViewChanged += ScrollView_ViewChanged;
        _itemsRepeater = ScrollView.Content.To<ItemsRepeater>();
        _itemsRepeater.SizeChanged += AdvancedItemsViewOnSizeChanged;
        _itemsRepeater.ElementPrepared += (_, arg) =>
        {
            // _loadedElements.Count is 0 说明不存在TryLoadedFirst注释里的情况
            // 否则需要检测该控件是否加载，加载后才能触发ElementPrepared，以此防止ElementPrepared调用两次
            var itemContainer = arg.Element.To<ItemContainer>();
            if (_loadedElements.Count is 0 || _loadedElements.Contains(arg.Element.GetHashCode()))
                ElementPrepared?.Invoke(this, itemContainer);
        };
        _itemsRepeater.ElementClearing += (_, arg) => ElementClearing?.Invoke(this, arg.Element.To<ItemContainer>());
    }

    private void AdvancedItemsViewOnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectionMode is not ItemsViewSelectionMode.Single)
            return;

        SelectedIndex = sender.To<AdvancedItemsView>().GetSelectedIndex();
    }

    private async void ScrollView_ViewChanged(ScrollView sender, object args) => await TryRaiseLoadMoreRequestedAsync();

    /// <summary>
    /// 当数据源变化或者<see cref="NotifyCollectionChangedAction.Reset"/>、<see cref="NotifyCollectionChangedAction.Remove"/>时，
    /// 这个方法可以重新加载数据。
    /// 这个方法旨在解决数据源变化，而<see cref="ItemsRepeater"/>的<see cref="FrameworkElement.ActualHeight"/>没有变化时，重新加载数据的问题
    /// </summary>
    private async void ItemsSourceOnPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (sender.To<AdvancedItemsView>() is { ItemsSource: ISupportIncrementalLoading sil })
        {
            if (sil is INotifyCollectionChanged ncc)
            {
                _sourceWeakEventListener?.Detach();

                _sourceWeakEventListener =
                    new WeakEventListener<AdvancedItemsView, object?, NotifyCollectionChangedEventArgs>(this)
                    {
                        // ReSharper disable once AsyncVoidLambda
                        OnEventAction = async (source, changed, arg) => await TryRaiseLoadMoreRequestedAsync(),
                        OnDetachAction = listener => ncc.CollectionChanged -= listener.OnEvent
                    };
                ncc.CollectionChanged += _sourceWeakEventListener.OnEvent;
            }

            // 在第一次加载时，ScrollView还未初始化，可以交给AdvancedItemsView_OnSizeChanged来触发
            if (ScrollView != null!)
                await TryRaiseLoadMoreRequestedAsync();
        }
    }

    /// <summary>
    /// 这个方法是连续加载的关键所在。
    /// 当新数据加载完毕后，会使<see cref="ItemsRepeater"/>的<see cref="FrameworkElement.ActualHeight"/>变大，
    /// 或数据换源后（或第一次设置数据源时），<see cref="FrameworkElement.ActualHeight"/>变为0，这个方法都可以重新加载数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AdvancedItemsViewOnSizeChanged(object sender, SizeChangedEventArgs e) => await TryRaiseLoadMoreRequestedAsync();

    #endregion

    #region HelperMembers

    private ItemsRepeater _itemsRepeater = null!;

    private readonly HashSet<int> _loadedElements = [];

    private WeakEventListener<AdvancedItemsView, object?, NotifyCollectionChangedEventArgs> _sourceWeakEventListener = null!;

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
