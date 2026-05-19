// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Pixeval.Collections;
using Pixeval.Utilities;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Controls;

/// <summary>
/// Provides attached properties for incremental loading behavior on <see cref="ItemsControl"/>.
/// </summary>
public static class IncrementalLoadingBehavior
{
    /// <summary>
    /// Attached property to enable incremental loading on an <see cref="ItemsControl"/>.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsEnabled",
            typeof(IncrementalLoadingBehavior),
            defaultValue: false);

    /// <summary>
    /// Attached property to set the threshold (in pixels) before the end to trigger loading.
    /// </summary>
    public static readonly AttachedProperty<double> ThresholdProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, double>(
            "Threshold",
            typeof(IncrementalLoadingBehavior),
            defaultValue: 100.0);

    /// <summary>
    /// Attached property to set the number of items to load per batch.
    /// </summary>
    public static readonly AttachedProperty<int> ItemsPerBatchProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, int>(
            "ItemsPerBatch",
            typeof(IncrementalLoadingBehavior),
            defaultValue: 20);

    /// <summary>
    /// Attached property indicating whether the control is currently loading more items.
    /// </summary>
    public static readonly AttachedProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsLoadingMore",
            typeof(IncrementalLoadingBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>?> WeakEventListenerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>?>(
            "WeakEventListener",
            typeof(IncrementalLoadingBehavior));

    public static readonly AttachedProperty<ScrollViewer?> AttachedScrollViewerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, ScrollViewer?>(
            "AttachedScrollViewer",
            typeof(IncrementalLoadingBehavior));

    public static readonly AttachedProperty<ItemsControl?> ScrollViewerOwnerProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ItemsControl?>(
            "ScrollViewerOwner",
            typeof(IncrementalLoadingBehavior));

    public static readonly AttachedProperty<int> InitialLoadVersionProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, int>(
            "InitialLoadVersion",
            typeof(IncrementalLoadingBehavior),
            defaultValue: 0);

    static IncrementalLoadingBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<ItemsControl>(OnIsEnabledChanged);
    }

    extension(ItemsControl itemsControl)
    {
        public bool IsEnabled
        {
            get => itemsControl.GetValue(IsEnabledProperty);
            set => itemsControl.SetValue(IsEnabledProperty, value);
        }

        public double Threshold
        {
            get => itemsControl.GetValue(ThresholdProperty);
            set => itemsControl.SetValue(ThresholdProperty, value);
        }

        public int ItemsPerBatch
        {
            get => itemsControl.GetValue(ItemsPerBatchProperty);
            set => itemsControl.SetValue(ItemsPerBatchProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the control is currently loading more items.
        /// </summary>
        public bool IsLoadingMore
        {
            get => itemsControl.GetValue(IsLoadingMoreProperty);
            private set => itemsControl.SetValue(IsLoadingMoreProperty, value);
        }

        public WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>? WeakEventListener
        {
            get => itemsControl.GetValue(WeakEventListenerProperty);
            private set => itemsControl.SetValue(WeakEventListenerProperty, value);
        }

        public ScrollViewer? AttachedScrollViewer
        {
            get => itemsControl.GetValue(AttachedScrollViewerProperty);
            private set => itemsControl.SetValue(AttachedScrollViewerProperty, value);
        }

        private int InitialLoadVersion
        {
            get => itemsControl.GetValue(InitialLoadVersionProperty);
            set => itemsControl.SetValue(InitialLoadVersionProperty, value);
        }

        private async Task<bool> TryLoadMoreItemsAsync()
        {
            if (!itemsControl.IsLoaded || !itemsControl.IsEffectivelyVisible)
                return false;

            if (itemsControl is not { IsLoadingMore: false })
                return false;

            try
            {
                itemsControl.IsLoadingMore = true;

                if (itemsControl is not { ItemsSource: IIncrementalLoading { HasMoreItems: true } source })
                    return false;

                if ((itemsControl.AttachedScrollViewer ?? itemsControl.FindDescendantOfType<ScrollViewer>()) is not { IsEffectivelyVisible: true } scrollViewer)
                    return false;

                if (scrollViewer.FindDescendantOfType<ScrollPresenter>() is not { ContentOrientation: var orientation and not ScrollContentOrientation.None })
                    return false;

                var isCollectionEmpty = itemsControl.ItemsSource is ICollection { Count: 0 };

                var isNearBottom =
                    (orientation is ScrollContentOrientation.Vertical or ScrollContentOrientation.Both &&
                     scrollViewer.Viewport.Height > 0 &&
                     scrollViewer.Extent.Height > 0 &&
                     scrollViewer.Offset.Y + scrollViewer.Viewport.Height >=
                     scrollViewer.Extent.Height - itemsControl.Threshold)
                    || (orientation is ScrollContentOrientation.Horizontal or ScrollContentOrientation.Both &&
                        scrollViewer.Viewport.Width > 0 &&
                        scrollViewer.Extent.Width > 0 &&
                        scrollViewer.Offset.X + scrollViewer.Viewport.Width >=
                        scrollViewer.Extent.Width - itemsControl.Threshold);

                if (!isCollectionEmpty && !isNearBottom)
                    return false;

                try
                {
                    await source.LoadMoreItemsAsync(itemsControl.ItemsPerBatch);
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }

                return source.HasMoreItems;
            }
            finally
            {
                itemsControl.IsLoadingMore = false;
            }
        }

        private void AttachItemsSource(IIncrementalLoading sil)
        {
            itemsControl.DetachItemsSource();

            if (sil is INotifyCollectionChanged ncc)
            {
                var listener = itemsControl.WeakEventListener =
                    new(itemsControl)
                    {
                        // ReSharper disable once AsyncVoidLambda
                        OnEventAction = async (source, changed, arg) => await source.TryLoadMoreItemsAsync(),
                        OnDetachAction = weakListener => ncc.CollectionChanged -= weakListener.OnEvent
                    };
                ncc.CollectionChanged += listener.OnEvent;
            }

            itemsControl.RequestInitialLoad();
        }

        private void AttachScrollViewer()
        {
            itemsControl.DetachScrollViewer();

            var scrollViewer = itemsControl.FindDescendantOfType<ScrollViewer>();
            if (scrollViewer is null)
                return;

            itemsControl.AttachedScrollViewer = scrollViewer;
            scrollViewer.SetValue(ScrollViewerOwnerProperty, itemsControl);
            scrollViewer.PropertyChanged += OnScrollViewerPropertyChanged;
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            itemsControl.RequestInitialLoad();
        }

        private void DetachScrollViewer()
        {
            if (itemsControl.AttachedScrollViewer is { } scrollViewer)
            {
                scrollViewer.PropertyChanged -= OnScrollViewerPropertyChanged;
                scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
                scrollViewer.ClearValue(ScrollViewerOwnerProperty);
                itemsControl.AttachedScrollViewer = null;
            }

            itemsControl.IsLoadingMore = false;
        }

        private void DetachItemsSource()
        {
            itemsControl.WeakEventListener?.Detach();
            itemsControl.WeakEventListener = null;
        }

        private void RequestInitialLoad()
        {
            var version = checked(itemsControl.InitialLoadVersion + 1);
            itemsControl.InitialLoadVersion = version;
            _ = itemsControl.InitialLoadAsync(version);
        }

        private void CancelInitialLoad()
        {
            itemsControl.InitialLoadVersion = checked(itemsControl.InitialLoadVersion + 1);
        }

        /// <summary>
        /// Performs initial loading to fill the viewport if it's not scrollable yet.
        /// </summary>
        private async Task InitialLoadAsync(int version)
        {
            while (version == itemsControl.InitialLoadVersion)
            {
                await Task.Delay(50);

                if (version != itemsControl.InitialLoadVersion)
                    break;

                if (!itemsControl.IsLoaded || !itemsControl.IsEffectivelyVisible)
                    continue;

                if (!await itemsControl.TryLoadMoreItemsAsync())
                    break;
            }
        }
    }

    // Static accessors for XAML
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetIsEnabled(ItemsControl element) => element.GetValue(IsEnabledProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsEnabled(ItemsControl element, bool value) => element.SetValue(IsEnabledProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static double GetThreshold(ItemsControl element) => element.GetValue(ThresholdProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetThreshold(ItemsControl element, double value) => element.SetValue(ThresholdProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static int GetItemsPerBatch(ItemsControl element) => element.GetValue(ItemsPerBatchProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetItemsPerBatch(ItemsControl element, int value) => element.SetValue(ItemsPerBatchProperty, value);

    private static void OnIsEnabledChanged(ItemsControl itemsControl, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            itemsControl.Loaded += OnItemsControlLoaded;
            itemsControl.Unloaded += OnItemsControlUnloaded;
            itemsControl.PropertyChanged += OnItemsControlOnPropertyChanged;

            if (itemsControl.IsLoaded)
            {
                itemsControl.AttachScrollViewer();
                if (itemsControl.ItemsSource is IIncrementalLoading sil)
                    itemsControl.AttachItemsSource(sil);
            }
        }
        else
        {
            itemsControl.Loaded -= OnItemsControlLoaded;
            itemsControl.Unloaded -= OnItemsControlUnloaded;
            itemsControl.PropertyChanged -= OnItemsControlOnPropertyChanged;
            itemsControl.CancelInitialLoad();
            itemsControl.DetachItemsSource();
            itemsControl.DetachScrollViewer();
        }
    }

    private static void OnItemsControlOnPropertyChanged(object? s, AvaloniaPropertyChangedEventArgs e)
    {
        if (s is not ItemsControl ic || e.Property != ItemsControl.ItemsSourceProperty)
            return;

        ic.CancelInitialLoad();
        ic.DetachItemsSource();

        if (ic is { IsEnabled: true, IsLoaded: true } && e.NewValue is IIncrementalLoading sil)
            ic.AttachItemsSource(sil);
    }

    private static void OnItemsControlLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.AttachScrollViewer();
        if (itemsControl.ItemsSource is IIncrementalLoading sil)
            itemsControl.AttachItemsSource(sil);
    }

    private static void OnItemsControlUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.CancelInitialLoad();
        itemsControl.DetachItemsSource();
        itemsControl.DetachScrollViewer();
    }

    private static void OnScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        if (args.Property != ScrollViewer.OffsetProperty
            && args.Property != Visual.BoundsProperty)
            return;

        if (scrollViewer.GetValue(ScrollViewerOwnerProperty) is { } itemsControl)
            _ = itemsControl.TryLoadMoreItemsAsync();
    }

    private static void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs args)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        if (scrollViewer.GetValue(ScrollViewerOwnerProperty) is { } itemsControl)
            _ = itemsControl.TryLoadMoreItemsAsync();
    }
}
