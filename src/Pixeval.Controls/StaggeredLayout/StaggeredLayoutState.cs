#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/StaggeredLayoutState.cs
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

using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pixeval.Controls;

internal class StaggeredLayoutState(VirtualizingLayoutContext context)
{
    private readonly List<StaggeredItem> _items = [];
    private readonly Dictionary<int, StaggeredColumnLayout> _columnLayout = [];
    private double _lastAverageHeight;

    public double ColumnWidth { get; internal set; }

    public int NumberOfColumns => _columnLayout.Count;

    public double RowSpacing { get; internal set; }

    internal void AddItemToColumn(StaggeredItem item, int columnIndex)
    {
        if (_columnLayout.TryGetValue(columnIndex, out var columnLayout) == false)
        {
            columnLayout = [];
            _columnLayout[columnIndex] = columnLayout;
        }

        if (columnLayout.Contains(item) == false)
        {
            columnLayout.Add(item);
        }
    }

    internal StaggeredItem GetItemAt(int index)
    {
        if (index < 0)
        {
            throw new IndexOutOfRangeException();
        }

        if (index <= (_items.Count - 1))
        {
            return _items[index];
        }
        else
        {
            var item = new StaggeredItem(index);
            _items.Add(item);
            return item;
        }
    }

    internal StaggeredColumnLayout GetColumnLayout(int columnIndex)
    {
        _columnLayout.TryGetValue(columnIndex, out var columnLayout);
        return columnLayout!;
    }

    /// <summary>
    /// Clear everything that has been calculated.
    /// </summary>
    internal void Clear()
    {
        _columnLayout.Clear();
        _items.Clear();
    }

    /// <summary>
    /// Clear the layout columns so they will be recalculated.
    /// </summary>
    internal void ClearColumns()
    {
        _columnLayout.Clear();
    }

    /// <summary>
    /// Gets the estimated height of the layout.
    /// </summary>
    /// <returns>The estimated height of the layout.</returns>
    /// <remarks>
    /// If all the items have been calculated then the actual height will be returned.
    /// If all the items have not been calculated then an estimated height will be calculated based on the average height of the items.
    /// </remarks>
    internal double GetHeight()
    {
        var desiredHeight = _columnLayout.Values.Max(c => c.Height);

        var itemCount = _columnLayout.Values.Sum(c => c.Count);
        if (itemCount == context.ItemCount)
        {
            return desiredHeight;
        }

        var averageHeight = _columnLayout.Sum(kvp => kvp.Value.Height / kvp.Value.Count);

        averageHeight /= _columnLayout.Count;
        var estimatedHeight = (averageHeight * context.ItemCount) / _columnLayout.Count;
        if (estimatedHeight > desiredHeight)
        {
            desiredHeight = estimatedHeight;
        }

        if (Math.Abs(desiredHeight - _lastAverageHeight) < 5)
        {
            return _lastAverageHeight;
        }

        _lastAverageHeight = desiredHeight;
        return desiredHeight;
    }

    internal void RecycleElementAt(int index)
    {
        var element = context.GetOrCreateElementAt(index);
        context.RecycleElement(element);
    }

    internal void RemoveFromIndex(int index)
    {
        if (index >= _items.Count)
        {
            // Item was added/removed but we haven't realized that far yet
            return;
        }

        var numToRemove = _items.Count - index;
        _items.RemoveRange(index, numToRemove);

        foreach (var (_, layout) in _columnLayout)
        {
            for (var i = 0; i < layout.Count; i++)
            {
                if (layout[i].Index >= index)
                {
                    numToRemove = layout.Count - i;
                    layout.RemoveRange(i, numToRemove);
                    break;
                }
            }
        }
    }

    internal void RemoveRange(int startIndex, int endIndex)
    {
        for (var i = startIndex; i <= endIndex; i++)
        {
            if (i > _items.Count)
            {
                break;
            }

            var item = _items[i];
            item.Height = 0;
            item.Top = 0;

            // We must recycle all elements to ensure that it gets the correct context
            RecycleElementAt(i);
        }

        foreach (var (_, layout) in _columnLayout)
        {
            for (var i = 0; i < layout.Count; i++)
            {
                if ((startIndex <= layout[i].Index) && (layout[i].Index <= endIndex))
                {
                    var numToRemove = layout.Count - i;
                    layout.RemoveRange(i, numToRemove);
                    break;
                }
            }
        }
    }
}
