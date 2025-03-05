// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WinUI3Utilities;

namespace Pixeval.Controls;

internal class RiverFlowLayoutState(VirtualizingLayoutContext context)
{
    private readonly List<RiverFlowItem> _items = [];

    public Size Spacing { get; internal set; }

    public double LineHeight { get; internal set; }

    public double AvailableWidth { get; internal set; }

    internal RiverFlowItem GetItemAt(int index)
    {
        if (index < 0)
        {
            ThrowHelper.IndexOutOfRange();
        }

        if (index <= _items.Count - 1)
        {
            return _items[index];
        }
        else
        {
            var item = new RiverFlowItem(index);
            _items.Add(item);
            return item;
        }
    }

    internal void Clear()
    {
        _items.Clear();
    }

    internal void ClearMeasureFromIndex(int index)
    {
        if (index >= _items.Count)
        {
            // Item was added/removed, but we haven't realized that far yet
            return;
        }

        foreach (var item in _items.Skip(index))
        {
            item.Measure = null;
            item.Position = null;
        }
    }

    internal void ClearMeasure()
    {
        foreach (var item in _items)
        {
            item.Measure = null;
            item.Position = null;
        }
    }

    internal double GetHeight()
    {
        if (_items.Count is 0)
        {
            return 0;
        }

        var lastPosition = null as Point?;

        for (var i = _items.Count - 1; i >= 0; --i)
        {
            var item = _items[i];

            if (item.Position is null)
            {
                continue;
            }

            if (lastPosition is not null && lastPosition.Value.Y > item.Position.Value.Y)
            {
                // This is a row above the last item.
                break;
            }

            lastPosition = item.Position;
        }

        return lastPosition?.Y + LineHeight ?? 0;
    }

    internal void RecycleElementAt(int index)
    {
        var element = context.GetOrCreateElementAt(index);
        context.RecycleElement(element);
    }
}
