// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Pixeval.Collections;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// Provides attached properties for incremental loading behavior on <see cref="ItemsControl"/> when its panel is an <see cref="AdaptiveGrid"/>.
/// </summary>
public static class AdaptiveGridIncrementalLoadingBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsEnabled",
            typeof(AdaptiveGridIncrementalLoadingBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<int> ItemsPerBatchProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, int>(
            "ItemsPerBatch",
            typeof(AdaptiveGridIncrementalLoadingBehavior),
            defaultValue: 20);

    private static readonly AttachedProperty<bool> IsLoadCheckQueuedProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsLoadCheckQueued",
            typeof(AdaptiveGridIncrementalLoadingBehavior),
            defaultValue: false);

    private static readonly AttachedProperty<WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>?> WeakEventListenerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>?>(
            "WeakEventListener",
            typeof(AdaptiveGridIncrementalLoadingBehavior));

    private static readonly AttachedProperty<AdaptiveGrid?> AttachedAdaptiveGridProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, AdaptiveGrid?>(
            "AttachedAdaptiveGrid",
            typeof(AdaptiveGridIncrementalLoadingBehavior));

    private static readonly AttachedProperty<ItemsControl?> AdaptiveGridOwnerProperty =
        AvaloniaProperty.RegisterAttached<AdaptiveGrid, ItemsControl?>(
            "AdaptiveGridOwner",
            typeof(AdaptiveGridIncrementalLoadingBehavior));

    static AdaptiveGridIncrementalLoadingBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<ItemsControl>(OnIsEnabledChanged);
    }

    extension(ItemsControl itemsControl)
    {
        private bool AdaptiveGridLoadingIsEnabled
        {
            get => itemsControl.GetValue(IsEnabledProperty);
            set => itemsControl.SetValue(IsEnabledProperty, value);
        }

        private int AdaptiveGridItemsPerBatch
        {
            get => itemsControl.GetValue(ItemsPerBatchProperty);
            set => itemsControl.SetValue(ItemsPerBatchProperty, value);
        }

        private bool IsLoadCheckQueued
        {
            get => itemsControl.GetValue(IsLoadCheckQueuedProperty);
            set => itemsControl.SetValue(IsLoadCheckQueuedProperty, value);
        }

        private bool SharedIsLoadingMore
        {
            get => itemsControl.GetValue(IncrementalLoadingBehavior.IsLoadingMoreProperty);
            set => itemsControl.SetValue(IncrementalLoadingBehavior.IsLoadingMoreProperty, value);
        }

        private WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>? AdaptiveGridWeakEventListener
        {
            get => itemsControl.GetValue(WeakEventListenerProperty);
            set => itemsControl.SetValue(WeakEventListenerProperty, value);
        }

        private AdaptiveGrid? AttachedAdaptiveGrid
        {
            get => itemsControl.GetValue(AttachedAdaptiveGridProperty);
            set => itemsControl.SetValue(AttachedAdaptiveGridProperty, value);
        }

        private void RequestLoadCheck()
        {
            if (itemsControl.IsLoadCheckQueued)
                return;

            itemsControl.IsLoadCheckQueued = true;
            Dispatcher.UIThread.Post(
                static async state =>
                {
                    if (state is not ItemsControl control)
                        return;

                    control.IsLoadCheckQueued = false;
                    await control.TryLoadMoreItemsAsync();
                },
                itemsControl,
                DispatcherPriority.Loaded);
        }

        private async Task TryLoadMoreItemsAsync()
        {
            if (!itemsControl.IsLoaded || !itemsControl.IsEffectivelyVisible)
                return;

            if (itemsControl.SharedIsLoadingMore)
                return;

            if ((itemsControl.AttachedAdaptiveGrid ?? itemsControl.AttachAdaptiveGrid()) is not { IsEffectivelyVisible: true } adaptiveGrid)
                return;

            try
            {
                itemsControl.SharedIsLoadingMore = true;

                while (itemsControl.ItemsSource is IIncrementalLoading { HasMoreItems: true } source && itemsControl.ShouldLoadMore(adaptiveGrid))
                {
                    try
                    {
                        if (await source.LoadMoreItemsAsync(itemsControl.AdaptiveGridItemsPerBatch) <= 0)
                            break;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (!itemsControl.IsLoaded || !itemsControl.IsEffectivelyVisible)
                        break;
                }
            }
            finally
            {
                itemsControl.SharedIsLoadingMore = false;
            }
        }

        private bool ShouldLoadMore(AdaptiveGrid adaptiveGrid)
        {
            if (itemsControl.ItemsSource is not IIncrementalLoading { HasMoreItems: true })
                return false;

            var itemCount = itemsControl.GetDisplayedItemCount();
            if (itemCount is 0)
                return true;

            return adaptiveGrid is { Lines: > 0, ItemsPerLine: > 0 } && itemCount < adaptiveGrid.Lines * adaptiveGrid.ItemsPerLine;
        }

        private int GetDisplayedItemCount()
        {
            if (itemsControl.ItemsSource is ICollection collection)
                return collection.Count;

            if (itemsControl.AttachedAdaptiveGrid is not { } adaptiveGrid)
                return 0;

            var count = 0;
            foreach (var child in adaptiveGrid.Children)
            {
                if (child.IsVisible)
                    ++count;
            }

            return count;
        }

        private void AttachItemsSource(IIncrementalLoading source)
        {
            itemsControl.DetachItemsSource();

            if (source is INotifyCollectionChanged ncc)
            {
                var listener = itemsControl.AdaptiveGridWeakEventListener =
                    new(itemsControl)
                    {
                        OnEventAction = static (control, _, _) => control.RequestLoadCheck(),
                        OnDetachAction = weakListener => ncc.CollectionChanged -= weakListener.OnEvent
                    };
                ncc.CollectionChanged += listener.OnEvent;
            }

            itemsControl.RequestLoadCheck();
        }

        private AdaptiveGrid? AttachAdaptiveGrid()
        {
            itemsControl.DetachAdaptiveGrid();

            var adaptiveGrid = itemsControl.FindDescendantOfType<AdaptiveGrid>();
            if (adaptiveGrid is null)
                return null;

            itemsControl.AttachedAdaptiveGrid = adaptiveGrid;
            adaptiveGrid.SetValue(AdaptiveGridOwnerProperty, itemsControl);
            adaptiveGrid.PropertyChanged += OnAdaptiveGridPropertyChanged;
            return adaptiveGrid;
        }

        private void AttachAdaptiveGridLater()
        {
            Dispatcher.UIThread.Post(
                static state =>
                {
                    if (state is not ItemsControl control || !control.AdaptiveGridLoadingIsEnabled || !control.IsLoaded)
                        return;

                    control.AttachAdaptiveGrid();
                    control.RequestLoadCheck();
                },
                itemsControl,
                DispatcherPriority.Loaded);
        }

        private void DetachAdaptiveGrid()
        {
            if (itemsControl.AttachedAdaptiveGrid is { } adaptiveGrid)
            {
                adaptiveGrid.PropertyChanged -= OnAdaptiveGridPropertyChanged;
                adaptiveGrid.ClearValue(AdaptiveGridOwnerProperty);
                itemsControl.AttachedAdaptiveGrid = null;
            }
        }

        private void DetachItemsSource()
        {
            itemsControl.AdaptiveGridWeakEventListener?.Detach();
            itemsControl.AdaptiveGridWeakEventListener = null;
        }

        private void ReattachItemsSource()
        {
            itemsControl.DetachItemsSource();
            if (itemsControl.ItemsSource is IIncrementalLoading source)
                itemsControl.AttachItemsSource(source);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool GetIsEnabled(ItemsControl element) => element.GetValue(IsEnabledProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsEnabled(ItemsControl element, bool value) => element.SetValue(IsEnabledProperty, value);

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
            itemsControl.PropertyChanged += OnItemsControlPropertyChanged;

            if (itemsControl.IsLoaded)
            {
                itemsControl.AttachAdaptiveGridLater();
                itemsControl.ReattachItemsSource();
            }
        }
        else
        {
            itemsControl.Loaded -= OnItemsControlLoaded;
            itemsControl.Unloaded -= OnItemsControlUnloaded;
            itemsControl.PropertyChanged -= OnItemsControlPropertyChanged;
            itemsControl.DetachItemsSource();
            itemsControl.DetachAdaptiveGrid();
            itemsControl.IsLoadCheckQueued = false;
        }
    }

    private static void OnItemsControlLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.AttachAdaptiveGridLater();
        itemsControl.ReattachItemsSource();
    }

    private static void OnItemsControlUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.DetachItemsSource();
        itemsControl.DetachAdaptiveGrid();
        itemsControl.IsLoadCheckQueued = false;
    }

    private static void OnItemsControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        if (e.Property == ItemsControl.ItemsSourceProperty)
        {
            itemsControl.ReattachItemsSource();
            itemsControl.RequestLoadCheck();
            return;
        }

        if (e.Property == ItemsControl.ItemsPanelProperty)
        {
            itemsControl.DetachAdaptiveGrid();
            itemsControl.AttachAdaptiveGridLater();
        }
    }

    private static void OnAdaptiveGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not AdaptiveGrid adaptiveGrid)
            return;

        if (e.Property != AdaptiveGrid.LinesProperty && e.Property != AdaptiveGrid.ItemsPerLineProperty)
            return;

        if (adaptiveGrid.GetValue(AdaptiveGridOwnerProperty) is { } itemsControl)
            itemsControl.RequestLoadCheck();
    }
}