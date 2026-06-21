// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Pixeval.Collections;
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

    public static readonly AttachedProperty<ScrollViewer?> AttachedScrollViewerProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, ScrollViewer?>(
            "AttachedScrollViewer",
            typeof(IncrementalLoadingBehavior));

    public static readonly AttachedProperty<ItemsControl?> ScrollViewerOwnerProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ItemsControl?>(
            "ScrollViewerOwner",
            typeof(IncrementalLoadingBehavior));

    private static readonly AttachedProperty<bool> IsLoadCheckQueuedProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "IsLoadCheckQueued",
            typeof(IncrementalLoadingBehavior),
            defaultValue: false);

    private static readonly AttachedProperty<bool> HasPendingLoadCheckProperty =
        AvaloniaProperty.RegisterAttached<ItemsControl, bool>(
            "HasPendingLoadCheck",
            typeof(IncrementalLoadingBehavior),
            defaultValue: false);

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

        public ScrollViewer? AttachedScrollViewer
        {
            get => itemsControl.GetValue(AttachedScrollViewerProperty);
            private set => itemsControl.SetValue(AttachedScrollViewerProperty, value);
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

        private void RequestLoadCheck()
        {
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
                    await control.TryLoadMoreItemsAsync();
                },
                itemsControl,
                DispatcherPriority.Loaded);
        }

        private async Task TryLoadMoreItemsAsync()
        {
            if (!itemsControl.IsLoaded || !itemsControl.IsEffectivelyVisible)
                return;

            if (itemsControl is not { IsLoadingMore: false })
                return;

            try
            {
                itemsControl.IsLoadingMore = true;

                if (itemsControl is not { ItemsSource: IIncrementalLoading { HasMoreItems: true } source })
                    return;

                if ((itemsControl.AttachedScrollViewer ?? itemsControl.FindDescendantOfType<ScrollViewer>()) is not { IsEffectivelyVisible: true } scrollViewer)
                    return;

                if (scrollViewer.FindDescendantOfType<ScrollPresenter>() is not { ContentOrientation: var orientation and not ScrollContentOrientation.None })
                    return;

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
                    return;

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

            }
            finally
            {
                itemsControl.IsLoadingMore = false;

                if (itemsControl.HasPendingLoadCheck)
                {
                    itemsControl.HasPendingLoadCheck = false;
                    itemsControl.RequestLoadCheck();
                }
            }
        }

        private void AttachScrollViewer()
        {
            itemsControl.DetachScrollViewer();

            var scrollViewer = itemsControl.FindDescendantOfType<ScrollViewer>();
            if (scrollViewer is null)
                return;

            itemsControl.AttachedScrollViewer = scrollViewer;
            scrollViewer.SetValue(ScrollViewerOwnerProperty, itemsControl);
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
        }

        private void DetachScrollViewer()
        {
            if (itemsControl.AttachedScrollViewer is { } scrollViewer)
            {
                scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
                scrollViewer.ClearValue(ScrollViewerOwnerProperty);
                itemsControl.AttachedScrollViewer = null;
            }

            itemsControl.IsLoadingMore = false;
            itemsControl.IsLoadCheckQueued = false;
            itemsControl.HasPendingLoadCheck = false;
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
            itemsControl.TemplateApplied += OnItemsControlTemplateApplied;
            itemsControl.Unloaded += OnItemsControlUnloaded;
            itemsControl.PropertyChanged += OnItemsControlOnPropertyChanged;

            if (itemsControl.IsLoaded)
            {
                itemsControl.AttachScrollViewer();
                itemsControl.RequestLoadCheck();
            }
        }
        else
        {
            itemsControl.Loaded -= OnItemsControlLoaded;
            itemsControl.TemplateApplied -= OnItemsControlTemplateApplied;
            itemsControl.Unloaded -= OnItemsControlUnloaded;
            itemsControl.PropertyChanged -= OnItemsControlOnPropertyChanged;
            itemsControl.DetachScrollViewer();
        }
    }

    private static void OnItemsControlOnPropertyChanged(object? s, AvaloniaPropertyChangedEventArgs e)
    {
        if (s is not ItemsControl ic || e.Property != ItemsControl.ItemsSourceProperty)
            return;

        ic.HasPendingLoadCheck = false;
    }

    private static void OnItemsControlLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.AttachScrollViewer();
        itemsControl.RequestLoadCheck();
    }

    private static void OnItemsControlTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.AttachScrollViewer();
        itemsControl.RequestLoadCheck();
    }

    private static void OnItemsControlUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        itemsControl.DetachScrollViewer();
    }

    private static void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs args)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        if (scrollViewer.GetValue(ScrollViewerOwnerProperty) is { } itemsControl)
            itemsControl.RequestLoadCheck();
    }
}
