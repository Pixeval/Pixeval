#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/RiverFlowLayoutState.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
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
