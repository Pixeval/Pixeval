// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Pixeval.Collections;
using Pixeval.Utilities;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Controls;

/// <summary>
/// Coalesces collection, layout, size, and scroll signals into serialized incremental load checks.
/// </summary>
public static class IncrementalLoadingBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsEnabled",
            typeof(IncrementalLoadingBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<double> ThresholdProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, double>(
            "Threshold",
            typeof(IncrementalLoadingBehavior),
            defaultValue: 100);

    public static readonly AttachedProperty<int> ItemsPerBatchProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, int>(
            "ItemsPerBatch",
            typeof(IncrementalLoadingBehavior),
            defaultValue: 20);

    public static readonly AttachedProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsLoadingMore",
            typeof(IncrementalLoadingBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<ScrollViewer?> AttachedScrollViewerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, ScrollViewer?>(
            "AttachedScrollViewer",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<ItemsControl?> ScrollViewerOwnerProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ItemsControl?>(
            "ScrollViewerOwner",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<bool> IsLoadCheckQueuedProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsLoadCheckQueued",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<bool> HasPendingLoadCheckProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "HasPendingLoadCheck",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>?> ItemsListenerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>?>(
            "ItemsListener",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<IAdaptiveGridLayoutInfo?> AttachedAdaptiveGridProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, IAdaptiveGridLayoutInfo?>(
            "AttachedAdaptiveGrid",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<ItemsControl?> AdaptiveGridOwnerProperty =
        AvaloniaProperty.RegisterAttached<Control, ItemsControl?>(
            "AdaptiveGridOwner",
            typeof(IncrementalLoadingBehavior));

    static IncrementalLoadingBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<ItemsControl>(OnIsEnabledChanged);
    }

    extension(ItemsControl itemsControl)
    {
        private bool IncrementalLoadingIsEnabled => itemsControl.GetValue(IsEnabledProperty);

        private double Threshold => itemsControl.GetValue(ThresholdProperty);

        private int ItemsPerBatch => itemsControl.GetValue(ItemsPerBatchProperty);

        private bool IsLoadingMore
        {
            get => itemsControl.GetValue(IsLoadingMoreProperty);
            set => itemsControl.SetValue(IsLoadingMoreProperty, value);
        }

        private ScrollViewer? AttachedScrollViewer
        {
            get => itemsControl.GetValue(AttachedScrollViewerProperty);
            set => itemsControl.SetValue(AttachedScrollViewerProperty, value);
        }

        private bool IsLoadCheckQueued
        {
            get => itemsControl.GetValue(IsLoadCheckQueuedProperty);
            set => itemsControl.SetValue(IsLoadCheckQueuedProperty, value);
        }

        private bool HasPendingLoadCheck
        {
            get => itemsControl.GetValue(HasPendingLoadCheckProperty);
            set => itemsControl.SetValue(HasPendingLoadCheckProperty, value);
        }

        private WeakEventListener<ItemsControl, object?, NotifyCollectionChangedEventArgs>? ItemsListener
        {
            get => itemsControl.GetValue(ItemsListenerProperty);
            set => itemsControl.SetValue(ItemsListenerProperty, value);
        }

        private IAdaptiveGridLayoutInfo? AttachedAdaptiveGrid
        {
            get => itemsControl.GetValue(AttachedAdaptiveGridProperty);
            set => itemsControl.SetValue(AttachedAdaptiveGridProperty, value);
        }

        private void RequestLoadCheck()
        {
            if (!itemsControl.IncrementalLoadingIsEnabled)
                return;

            if (itemsControl.IsLoadingMore)
            {
                itemsControl.HasPendingLoadCheck = true;
                return;
            }

            if (itemsControl.IsLoadCheckQueued)
                return;

            itemsControl.IsLoadCheckQueued = true;
            Dispatcher.UIThread.Post(
                static async state =>
                {
                    if (state is not ItemsControl control)
                        return;

                    control.IsLoadCheckQueued = false;
                    if (control.IncrementalLoadingIsEnabled)
                        await control.TryLoadMoreItemsAsync();
                },
                itemsControl,
                DispatcherPriority.Background);
        }

        private async Task TryLoadMoreItemsAsync()
        {
            if (!itemsControl.IsLoaded || !itemsControl.IsEffectivelyVisible)
                return;

            if (itemsControl.IsLoadingMore)
            {
                itemsControl.HasPendingLoadCheck = true;
                return;
            }

            if (itemsControl.ItemsSource is not IIncrementalLoading { HasMoreItems: true } source
                || !itemsControl.ShouldLoadMore())
                return;

            var loadedCount = 0;
            itemsControl.IsLoadingMore = true;
            try
            {
                try
                {
                    loadedCount = await source.LoadMoreItemsAsync(itemsControl.ItemsPerBatch);
                }
                catch (OperationCanceledException)
                {
                    // Closing or replacing the source is the expected cancellation path.
                }
            }
            finally
            {
                itemsControl.IsLoadingMore = false;
                var shouldCheckAgain = loadedCount > 0 || itemsControl.HasPendingLoadCheck;
                itemsControl.HasPendingLoadCheck = false;
                if (shouldCheckAgain)
                    itemsControl.RequestLoadCheck();
            }
        }

        private bool ShouldLoadMore()
        {
            if (itemsControl.ItemsSource is not IIncrementalLoading { HasMoreItems: true })
                return false;

            var itemCount = itemsControl.ItemsSource is ICollection collection
                ? collection.Count
                : itemsControl.Items.Count;
            if (itemCount is 0)
                return true;

            if (itemsControl.AttachedAdaptiveGrid is { Lines: > 0, ItemsPerLine: > 0 } grid
                && itemCount < grid.Lines * grid.ItemsPerLine)
                return true;

            if (itemsControl.AttachedScrollViewer is not { IsEffectivelyVisible: true } scrollViewer
                || scrollViewer.FindDescendantOfType<ScrollPresenter>() is not { ContentOrientation: var orientation and not ScrollContentOrientation.None })
                return false;

            return orientation is ScrollContentOrientation.Vertical or ScrollContentOrientation.Both
                   && scrollViewer.Viewport.Height > 0
                   && scrollViewer.Extent.Height > 0
                   && scrollViewer.Offset.Y + scrollViewer.Viewport.Height >= scrollViewer.Extent.Height - itemsControl.Threshold
                   || orientation is ScrollContentOrientation.Horizontal or ScrollContentOrientation.Both
                   && scrollViewer.Viewport.Width > 0
                   && scrollViewer.Extent.Width > 0
                   && scrollViewer.Offset.X + scrollViewer.Viewport.Width >= scrollViewer.Extent.Width - itemsControl.Threshold;
        }

        private void AttachSources()
        {
            itemsControl.AttachScrollViewer();
            itemsControl.AttachItemsSource();
            itemsControl.AttachAdaptiveGridLater();
        }

        private void DetachSources()
        {
            itemsControl.DetachScrollViewer();
            itemsControl.DetachItemsSource();
            itemsControl.DetachAdaptiveGrid();
            itemsControl.IsLoadCheckQueued = false;
            itemsControl.HasPendingLoadCheck = false;
        }

        private void AttachScrollViewer()
        {
            itemsControl.DetachScrollViewer();
            if (itemsControl.FindDescendantOfType<ScrollViewer>() is not { } scrollViewer)
                return;

            itemsControl.AttachedScrollViewer = scrollViewer;
            scrollViewer.SetValue(ScrollViewerOwnerProperty, itemsControl);
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.SizeChanged += OnScrollViewerSizeChanged;
        }

        private void DetachScrollViewer()
        {
            if (itemsControl.AttachedScrollViewer is not { } scrollViewer)
                return;

            scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
            scrollViewer.SizeChanged -= OnScrollViewerSizeChanged;
            scrollViewer.ClearValue(ScrollViewerOwnerProperty);
            itemsControl.AttachedScrollViewer = null;
        }

        private void AttachItemsSource()
        {
            itemsControl.DetachItemsSource();
            if (itemsControl.ItemsSource is not INotifyCollectionChanged collection)
                return;

            var listener = itemsControl.ItemsListener = new(itemsControl)
            {
                OnEventAction = static (control, _, _) =>
                {
                    // A completed load schedules one post-layout check. Its individual Add events
                    // must not enqueue another check against the pre-layout scroll extent.
                    if (!control.IsLoadingMore)
                        control.RequestLoadCheck();
                },
                OnDetachAction = weakListener => collection.CollectionChanged -= weakListener.OnEvent
            };
            collection.CollectionChanged += listener.OnEvent;
        }

        private void DetachItemsSource()
        {
            itemsControl.ItemsListener?.Detach();
            itemsControl.ItemsListener = null;
        }

        private void AttachAdaptiveGridLater()
        {
            Dispatcher.UIThread.Post(
                static state =>
                {
                    if (state is not ItemsControl control
                        || !control.IncrementalLoadingIsEnabled
                        || !control.IsLoaded)
                        return;

                    control.AttachAdaptiveGrid();
                    control.RequestLoadCheck();
                },
                itemsControl,
                DispatcherPriority.Loaded);
        }

        private void AttachAdaptiveGrid()
        {
            itemsControl.DetachAdaptiveGrid();
            if (itemsControl.GetVisualDescendants().OfType<IAdaptiveGridLayoutInfo>().FirstOrDefault() is not { } adaptiveGrid)
                return;

            itemsControl.AttachedAdaptiveGrid = adaptiveGrid;
            ((Control) adaptiveGrid).SetValue(AdaptiveGridOwnerProperty, itemsControl);
            adaptiveGrid.PropertyChanged += OnAdaptiveGridPropertyChanged;
        }

        private void DetachAdaptiveGrid()
        {
            if (itemsControl.AttachedAdaptiveGrid is not { } adaptiveGrid)
                return;

            adaptiveGrid.PropertyChanged -= OnAdaptiveGridPropertyChanged;
            ((Control) adaptiveGrid).ClearValue(AdaptiveGridOwnerProperty);
            itemsControl.AttachedAdaptiveGrid = null;
        }
    }

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
            itemsControl.TemplateApplied += OnItemsControlTemplateApplied;
            itemsControl.Unloaded += OnItemsControlUnloaded;
            itemsControl.PropertyChanged += OnItemsControlPropertyChanged;
            itemsControl.SizeChanged += OnItemsControlSizeChanged;

            if (itemsControl.IsLoaded)
            {
                itemsControl.AttachSources();
                itemsControl.RequestLoadCheck();
            }
        }
        else
        {
            itemsControl.Loaded -= OnItemsControlLoaded;
            itemsControl.TemplateApplied -= OnItemsControlTemplateApplied;
            itemsControl.Unloaded -= OnItemsControlUnloaded;
            itemsControl.PropertyChanged -= OnItemsControlPropertyChanged;
            itemsControl.SizeChanged -= OnItemsControlSizeChanged;
            itemsControl.DetachSources();
        }
    }

    private static void OnItemsControlLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.AttachSources();
        itemsControl.RequestLoadCheck();
    }

    private static void OnItemsControlTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl || !itemsControl.IsLoaded)
            return;

        itemsControl.AttachSources();
        itemsControl.RequestLoadCheck();
    }

    private static void OnItemsControlUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
            itemsControl.DetachSources();
    }

    private static void OnItemsControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        if (e.Property == ItemsControl.ItemsSourceProperty)
        {
            itemsControl.AttachItemsSource();
            itemsControl.RequestLoadCheck();
        }
        else if (e.Property == ItemsControl.ItemsPanelProperty)
        {
            itemsControl.DetachAdaptiveGrid();
            itemsControl.AttachAdaptiveGridLater();
            itemsControl.RequestLoadCheck();
        }
    }

    private static void OnItemsControlSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
            itemsControl.RequestLoadCheck();
    }

    private static void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer
            && scrollViewer.GetValue(ScrollViewerOwnerProperty) is { } itemsControl)
            itemsControl.RequestLoadCheck();
    }

    private static void OnScrollViewerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer
            && scrollViewer.GetValue(ScrollViewerOwnerProperty) is { } itemsControl)
            itemsControl.RequestLoadCheck();
    }

    private static void OnAdaptiveGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not IAdaptiveGridLayoutInfo adaptiveGrid
            || sender is not Control control
            || e.Property != AdaptiveGrid.LinesProperty
            && e.Property != AdaptiveGrid.ItemsPerLineProperty
            && e.Property != VirtualizingAdaptiveGrid.LinesProperty
            && e.Property != VirtualizingAdaptiveGrid.ItemsPerLineProperty)
            return;

        if (control.GetValue(AdaptiveGridOwnerProperty) is { } itemsControl)
            itemsControl.RequestLoadCheck();
    }
}
