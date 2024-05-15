using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Pixeval.Collections;
using Pixeval.Utilities;
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
[DependencyProperty<bool>("IsLoadingMore", "false", nameof(OnIsLoadingMoreChanged))]
[DependencyProperty<int>("LoadCount", "20")]
[DependencyProperty<bool>("DisableLoadingText", "false")]
public sealed partial class AdvancedItemsView : ItemsView
{
    public event Func<AdvancedItemsView, EventArgs, Task<bool>> LoadMoreRequested;

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
        if (ScrollView is null)
            return;

        var loadMore = true;
        // 加载直到有新元素加载进来
        while (loadMore)
        {
            if (!CanLoadMore || IsLoadingMore)
                return;

            // 只有页面没充满时会触发LoadMoreRequested
            if ((ScrollView.ScrollableHeight is 0 && ScrollView.ScrollableWidth is 0) ||
                (ScrollView.ScrollableHeight > 0 &&
                 ScrollView.ScrollableHeight - LoadingOffset < ScrollView.VerticalOffset) ||
                (ScrollView.ScrollableWidth > 0 &&
                 ScrollView.ScrollableWidth - LoadingOffset < ScrollView.HorizontalOffset))
            {
                IsLoadingMore = true;
                var before = GetItemsCount();
                if (await LoadMoreRequested(this, EventArgs.Empty))
                {
                    var after = GetItemsCount();
                    // 这里可以设为一行的元素数，这样在加载过少数量的时候，也可以持续加载
                    // 一般一次会加载20个元素，而一行元素数一般少于10，所以这里设为10
                    if (before + 10 < after)
                        loadMore = false;
                }
                else
                    loadMore = false;

                IsLoadingMore = false;
            }
            else
            {
                // 如果填满了页面，也无需继续加载
                loadMore = false;
            }
        }
    }

    public AdvancedItemsView()
    {
        SelectionChanged += AdvancedItemsViewOnSelectionChanged;
        LoadMoreRequested += async (sender, args) =>
        {
            if (sender.To<AdvancedItemsView>() is { ItemsSource: ISupportIncrementalLoading sil } aiv)
            {
                _ = await sil.LoadMoreItemsAsync((uint)aiv.LoadCount);
                return sil.HasMoreItems;
            }

            return false;
        };
        _scrollViewOnPropertyChangedToken = RegisterPropertyChangedCallback(ScrollViewProperty, ScrollViewOnPropertyChanged);
        var itemsSourceOnPropertyChangedToken = RegisterPropertyChangedCallback(ItemsSourceProperty, ItemsSourceOnPropertyChanged);
        Unloaded += (_, _) =>
        {
            UnregisterPropertyChangedCallback(ScrollViewProperty, _scrollViewOnPropertyChangedToken);
            UnregisterPropertyChangedCallback(ItemsSourceProperty, itemsSourceOnPropertyChangedToken);
        };
    }

    #region PropertyChanged

    private static void OnItemHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minItemHeight = advancedItemsView.MinItemHeight;
        switch (advancedItemsView.Layout)
        {
            case RiverFlowLayout linedFlowLayout:
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
        switch (advancedItemsView.Layout)
        {
            case StaggeredLayout staggeredLayout:
                staggeredLayout.DesiredColumnWidth = minItemWidth;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinItemWidth = minItemWidth;
                break;
        }
    }

    private static void OnMinRowSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var advancedItemsView = o.To<AdvancedItemsView>();
        var minRowSpacing = advancedItemsView.MinRowSpacing;
        switch (advancedItemsView.Layout)
        {
            case RiverFlowLayout linedFlowLayout:
                linedFlowLayout.VerticalSpacing = minRowSpacing;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinRowSpacing = minRowSpacing;
                break;
            case StaggeredLayout staggeredLayout:
                staggeredLayout.RowSpacing = minRowSpacing;
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
            case RiverFlowLayout linedFlowLayout:
                linedFlowLayout.HorizontalSpacing = minColumnSpacing;
                break;
            case UniformGridLayout uniformGridLayout:
                uniformGridLayout.MinColumnSpacing = minColumnSpacing;
                break;
            case StaggeredLayout staggeredLayout:
                staggeredLayout.ColumnSpacing = minColumnSpacing;
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
            ItemsViewLayoutType.LinedFlow => new RiverFlowLayout
            {
                // ItemsStretch = LinedFlowLayoutItemsStretch.Fill,
                LineHeight = minItemHeight,
                VerticalSpacing = minRowSpacing,
                HorizontalSpacing = minColumnSpacing
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
            ItemsViewLayoutType.Staggered => new StaggeredLayout
            {
                ColumnSpacing = minColumnSpacing,
                RowSpacing = minRowSpacing,
                DesiredColumnWidth = minItemWidth
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

    private static void OnIsLoadingMoreChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var aiv = o.To<AdvancedItemsView>();
        if (aiv is { DisableLoadingText: false, _loadingGrid: not null })
            aiv._loadingGrid.Visibility = aiv is { IsLoadingMore: true } ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region EventHandlers

    private readonly long _scrollViewOnPropertyChangedToken;

    /// <summary>
    /// 本方法之后会触发<see cref="AdvancedItemsViewOnSizeChanged"/>
    /// </summary>
    private void ScrollViewOnPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        UnregisterPropertyChangedCallback(ScrollViewProperty, _scrollViewOnPropertyChangedToken);
        ScrollView.ViewChanged += ScrollView_ViewChanged;
        ScrollView.PointerWheelChanged += ScrollView_PointerWheelChanged;
        _itemsRepeater = ScrollView.Content.To<ItemsRepeater>();

        if (!DisableLoadingText)
        {
            _loadingGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed,
                Padding = new(40),
                RowSpacing = 40,
                RowDefinitions =
                {
                    new() { Height = GridLength.Auto },
                    new() { Height = GridLength.Auto }
                },
                Children =
                {
                    new ProgressRing
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        IsActive = true,
                        IsIndeterminate = true,
                        Width = 50,
                        Height = 50
                    }.LetChain(t => Grid.SetRow(t, 0)),
                    new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = AdvancedItemsViewResources.LoadingMore,
                        FontSize = 28
                    }.LetChain(t => Grid.SetRow(t, 1))
                }
            }.LetChain(t => Grid.SetRow(t, 1));

            ScrollView.Content = new Grid
            {
                RowDefinitions =
                {
                    new() { Height = GridLength.Auto },
                    new() { Height = GridLength.Auto },
                },
                Children =
                {
                    _itemsRepeater.LetChain(t => Grid.SetRow(t, 0)),
                    _loadingGrid
                }
            };
        }
        _itemsRepeater.SizeChanged += AdvancedItemsViewOnSizeChanged;
    }

    private static void ScrollView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollView { ComputedVerticalScrollMode: ScrollingScrollMode.Disabled, ComputedHorizontalScrollMode: ScrollingScrollMode.Enabled } scrollView)
            _ = scrollView.AddScrollVelocity(new(-e.GetCurrentPoint(scrollView).Properties.MouseWheelDelta, 0), null);
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

    private Grid? _loadingGrid;

    private ItemsRepeater _itemsRepeater = null!;

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

    private int GetItemsCount()
    {
        return ItemsSource switch
        {
            Array array => array.Length,
            ICollection list => list.Count,
            IEnumerable enumerable => enumerable.Cast<object>().Count(),
            null => 0,
            _ => ThrowHelper.ArgumentOutOfRange<object, int>(ItemsSource)
        };
    }

    #endregion
}
