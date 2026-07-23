// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Pixeval.Controls;

/// <summary>
/// Provides container realization and recycling for panels whose layout is cached independently from controls.
/// </summary>
public abstract class CachedVirtualizingPanel : VirtualizingPanel
{
    private static readonly AttachedProperty<object?> RecycleKeyProperty =
        AvaloniaProperty.RegisterAttached<CachedVirtualizingPanel, Control, object?>("RecycleKey");

    private static readonly object _ItemIsItsOwnContainer = new();

    private readonly Dictionary<object, Stack<Control>> _recyclePool = [];

    protected Dictionary<int, Control> Realized { get; } = [];

    protected override Control? ContainerFromIndex(int index) =>
        index >= 0 && index < Items.Count && Realized.TryGetValue(index, out var container)
            ? container
            : null;

    protected override IEnumerable<Control>? GetRealizedContainers() => Realized.Values;

    protected override int IndexFromContainer(Control container)
    {
        foreach (var (index, realized) in Realized)
        {
            if (ReferenceEquals(realized, container))
                return index;
        }

        return -1;
    }

    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var count = Items.Count;
        if (count is 0)
            return null;

        var index = from is Control control ? IndexFromContainer(control) : -1;
        index = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => count - 1,
            NavigationDirection.Next or NavigationDirection.Right or NavigationDirection.Down => index + 1,
            NavigationDirection.Previous or NavigationDirection.Left or NavigationDirection.Up => index - 1,
            _ => -1
        };

        if (wrap)
            index = (index % count + count) % count;

        return index >= 0 && index < count ? ScrollIntoView(index) : null;
    }

    protected Control Realize(object? item, int index)
    {
        if (Realized.TryGetValue(index, out var realized))
            return realized;

        var container = GetOrCreateElement(item, index);
        Realized.Add(index, container);
        return container;
    }

    protected Control GetOrCreateElement(object? item, int index)
    {
        var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");

        if (!generator.NeedsContainer(item, index, out var recycleKey))
            return GetItemAsOwnContainer((Control) item!, item, index);

        if (recycleKey is not null
            && _recyclePool.TryGetValue(recycleKey, out var pool)
            && pool.Count > 0)
        {
            var recycled = pool.Pop();
            recycled.SetCurrentValue(IsVisibleProperty, true);
            generator.PrepareItemContainer(recycled, item, index);
            recycled.SetValue(RecycleKeyProperty, recycleKey);
            AddInternalChild(recycled);
            if (recycled is ListBoxItem)
                recycled.ClearValue(ListBoxItem.IsSelectedProperty);
            generator.ItemContainerPrepared(recycled, item, index);
            return recycled;
        }

        var container = generator.CreateContainer(item, index, recycleKey);
        container.SetValue(RecycleKeyProperty, recycleKey);
        generator.PrepareItemContainer(container, item, index);
        AddInternalChild(container);
        generator.ItemContainerPrepared(container, item, index);
        return container;
    }

    protected void RecycleOutsideRange(int start, int end)
    {
        foreach (var index in Realized.Keys.ToArray())
        {
            if (index >= start && index <= end)
                continue;

            var container = Realized[index];
            Realized.Remove(index);
            RecycleElement(container);
        }
    }

    protected void RecycleOutside(IReadOnlySet<int> indices)
    {
        foreach (var index in Realized.Keys.ToArray())
        {
            if (indices.Contains(index))
                continue;

            var container = Realized[index];
            Realized.Remove(index);
            RecycleElement(container);
        }
    }

    protected void RecycleAll(bool removeOwnContainers = false)
    {
        foreach (var container in Realized.Values.ToArray())
            RecycleElement(container, removeOwnContainers);

        Realized.Clear();
    }

    protected void ResetContainers()
    {
        RecycleAll(removeOwnContainers: true);
        _recyclePool.Clear();
    }

    protected void ShiftRealizedIndices(int startingIndex, int delta)
    {
        if (delta is 0 || Realized.Count is 0)
            return;

        var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");
        var shifted = Realized
            .OrderByDescending(pair => pair.Key)
            .Where(pair => pair.Key >= startingIndex)
            .ToArray();

        foreach (var (oldIndex, container) in shifted)
        {
            Realized.Remove(oldIndex);
            var newIndex = oldIndex + delta;
            Realized.Add(newIndex, container);
            generator.ItemContainerIndexChanged(container, oldIndex, newIndex);
        }
    }

    protected void RecycleElement(Control container, bool removeOwnContainer = false)
    {
        var recycleKey = container.GetValue(RecycleKeyProperty);

        if (recycleKey == _ItemIsItsOwnContainer)
        {
            if (removeOwnContainer)
            {
                RemoveInternalChild(container);
                container.ClearValue(RecycleKeyProperty);
            }
            else
            {
                container.SetCurrentValue(IsVisibleProperty, false);
            }

            return;
        }

        var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");
        generator.ClearItemContainer(container);
        RemoveInternalChild(container);

        if (recycleKey is null)
            return;

        container.SetCurrentValue(IsVisibleProperty, false);
        if (!_recyclePool.TryGetValue(recycleKey, out var pool))
        {
            pool = new Stack<Control>();
            _recyclePool.Add(recycleKey, pool);
        }

        pool.Push(container);
    }

    private Control GetItemAsOwnContainer(Control control, object? item, int index)
    {
        if (!control.IsSet(RecycleKeyProperty))
        {
            var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");
            generator.PrepareItemContainer(control, item, index);
            AddInternalChild(control);
            control.SetValue(RecycleKeyProperty, _ItemIsItsOwnContainer);
            generator.ItemContainerPrepared(control, item, index);
        }

        control.SetCurrentValue(IsVisibleProperty, true);
        return control;
    }
}
