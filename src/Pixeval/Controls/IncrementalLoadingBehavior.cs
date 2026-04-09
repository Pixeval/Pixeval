// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Pixeval.Collections;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// Provides attached properties for incremental loading behavior on ItemsControl.
/// </summary>
public static class IncrementalLoadingBehavior
{
    /// <summary>
    /// Attached property to enable incremental loading on an ItemsControl.
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

    public static readonly AttachedProperty<WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>> WeakEventListenerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>>(
            "WeakEventListener",
            typeof(IncrementalLoadingBehavior));

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

        public WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs> WeakEventListener
        {
            get => itemsControl.GetValue(WeakEventListenerProperty);
            private set => itemsControl.SetValue(WeakEventListenerProperty, value);
        }

        private async Task<bool> TryLoadMoreItemsAsync()
        {
            // Don't load if already loading
            if (itemsControl is not { IsLoadingMore: false })
                return false;
            try
            {
                itemsControl.IsLoadingMore = true;

                if (itemsControl is not { ItemsSource: ISupportIncrementalLoading { HasMoreItems: true } source })
                    return false;

                if (itemsControl.FindDescendantOfType<ScrollViewer>() is not { } scrollViewer)
                    return false;

                if (scrollViewer.FindDescendantOfType<IScrollable>() is not { } scrollable)
                    return false;

                var isNearBottom =
                    (scrollable.CanVerticallyScroll &&
                     scrollViewer.Offset.Y + scrollViewer.Viewport.Height >=
                     scrollViewer.Extent.Height - itemsControl.Threshold)
                    || (scrollable.CanHorizontallyScroll &&
                        scrollViewer.Offset.X + scrollViewer.Viewport.Width >=
                        scrollViewer.Extent.Width - itemsControl.Threshold);

                if (isNearBottom)
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

                return isNearBottom;
            }
            finally
            {
                itemsControl.IsLoadingMore = false;
            }
        }

        private void AttachItemsSource(ISupportIncrementalLoading sil)
        {
            // Find the ScrollViewer and trigger initial load
            if (sil is INotifyCollectionChanged ncc)
            {
                itemsControl.WeakEventListener?.Detach();

                var listener = itemsControl.WeakEventListener =
                    new(itemsControl)
                    {
                        // ReSharper disable once AsyncVoidLambda
                        OnEventAction = async (source, changed, arg) => await source.TryLoadMoreItemsAsync(),
                        OnDetachAction = listener => ncc.CollectionChanged -= listener.OnEvent
                    };
                ncc.CollectionChanged += listener.OnEvent;
            }

            _ = itemsControl.InitialLoadAsync();
        }

        private void AttachScrollViewer()
        {
            // Find the ScrollViewer inside the ItemsControl
            var scrollViewer = itemsControl.FindDescendantOfType<ScrollViewer>();
            if (scrollViewer is not null)
            {
                scrollViewer.PropertyChanged += (sender, args) =>
                {
                    if (args.Property == ScrollViewer.OffsetProperty
                        || args.Property == Visual.BoundsProperty)
                        _ = itemsControl.TryLoadMoreItemsAsync();
                };
                scrollViewer.ScrollChanged += async (sender, args) => await itemsControl.TryLoadMoreItemsAsync();

                // Initial load: fill the viewport if needed
                _ = itemsControl.InitialLoadAsync();
            }
        }

        private void DetachScrollViewer()
        {
            // Reset loading state
            itemsControl.IsLoadingMore = false;
        }

        /// <summary>
        /// Performs initial loading to fill the viewport if it's not scrollable yet.
        /// </summary>
        private async Task InitialLoadAsync()
        {
            await Task.Delay(50);

            while (true)
            {
                if (!await itemsControl.TryLoadMoreItemsAsync())
                    break;
                // 等待UI加载
                await Task.Delay(50);
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
            if (itemsControl.ItemsSource is ISupportIncrementalLoading sil)
                itemsControl.AttachItemsSource(sil);
            itemsControl.PropertyChanged += OnItemsControlOnPropertyChanged;
        }
        else
        {
            itemsControl.Loaded -= OnItemsControlLoaded;
            itemsControl.Unloaded -= OnItemsControlUnloaded;
            itemsControl.PropertyChanged -= OnItemsControlOnPropertyChanged;
            itemsControl.DetachScrollViewer();
        }
    }

    private static void OnItemsControlOnPropertyChanged(object? s, AvaloniaPropertyChangedEventArgs e)
    {
        if (s is not ItemsControl ic)
            return;
        if (e.Property == ItemsControl.ItemsSourceProperty && e.NewValue is ISupportIncrementalLoading sil)
            ic.AttachItemsSource(sil);
    }

    private static void OnItemsControlLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
            itemsControl.AttachScrollViewer();
    }

    private static void OnItemsControlUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
            itemsControl.DetachScrollViewer();
    }
}
