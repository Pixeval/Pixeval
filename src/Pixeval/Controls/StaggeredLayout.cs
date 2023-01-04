#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/StaggeredLayout.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;

namespace Pixeval.Controls;

/// <summary>
/// The StaggeredLayout allows for layout of items in a column approach 
/// where an item will be added to whichever column has used the least amount of space.
/// </summary>
public sealed class StaggeredLayout : VirtualizingLayout
{
    /// <summary>
    /// The horizontal alignment characteristics of its children.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
        set => SetValue(HorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// The space between one item and the next item in its column.
    /// </summary>
    public double RowSpacing
    {
        get => _rowSpacing;
        set => SetValue(RowSpacingProperty, value);
    }

    /// <summary>
    /// The space between each columns.
    /// </summary>
    public double ColumnSpacing
    {
        get => _columnSpacing;
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// The desired width of each column. 
    /// The width of columns can exceed the DesiredColumnWidth if the HorizontalAlignment is set to Stretch.
    /// </summary>
    public double DesiredColumnWidth
    {
        get => _desiredColumnWidth;
        set => SetValue(DesiredColumnWidthProperty, value);
    }

    /// <summary>
    /// The dimensions of the space between the edge and its child as a Thickness value. 
    /// Thickness is a structure that stores dimension values using pixel measures.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    #region Dependency Properties
    /// <summary>
    /// Represents the HorizontalAlignment property.
    /// </summary>
    public static DependencyProperty HorizontalAlignmentProperty { get; }
        = DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(StaggeredLayout), new PropertyMetadata(default(HorizontalAlignment), OnLayoutPropertyChanged));

    /// <summary>
    /// Represents the RowSpacing property.
    /// </summary>
    public static DependencyProperty RowSpacingProperty { get; }
        = DependencyProperty.Register("RowSpacing", typeof(double), typeof(StaggeredLayout), new PropertyMetadata(0.0, OnLayoutPropertyChanged));

    /// <summary>
    /// Represents the ColumnSpacing property.
    /// </summary>
    public static DependencyProperty ColumnSpacingProperty { get; }
        = DependencyProperty.Register("ColumnSpacing", typeof(double), typeof(StaggeredLayout), new PropertyMetadata(0.0, OnLayoutPropertyChanged));

    /// <summary>
    /// Represents the DesireColumnWidth property.
    /// </summary>
    public static DependencyProperty DesiredColumnWidthProperty { get; }
        = DependencyProperty.Register("DesiredColumnWidth", typeof(double), typeof(StaggeredLayout), new PropertyMetadata(250.0, OnLayoutPropertyChanged));

    /// <summary>
    /// Represents the Padding property.
    /// </summary>
    public static DependencyProperty PaddingProperty { get; }
        = DependencyProperty.Register("Padding", typeof(Thickness), typeof(StaggeredLayout), new PropertyMetadata(default(Thickness), OnLayoutPropertyChanged));
    #endregion

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        if (context.ItemCount == 0)
        {
            return new Size(availableSize.Width, 0.0);
        }

        GetMetrics(availableSize, out var columns, out var colWidth);
        Columns = columns;
        ColumnWidth = colWidth;

        var state = (StaggeredLayoutState)context.LayoutState;
        var reason = state.UpdateViewport(context.RealizationRect);

        if (reason == ViewportChangeReason.HorizontalResize || InvalidateMeasureCache)
        {
            state.ElementCache.InvalidateAllMeasure();
            state.PositionCache.RemoveFrom(0);
        }
        else if (InvalidatePositionCache)
        {
            state.PositionCache.RemoveFrom(0);
        }

        InvalidateMeasureCache = false;
        InvalidatePositionCache = false;

        for (var i = 0; i < context.ItemCount; i++)
        {
            double x;
            if (i < state.PositionCache.Count)
            {
                // Position cached.
                x = state.PositionCache[i].Top;
            }
            else
            {
                // Compute + Cache.
                var element = state.ElementCache.GetOrCreateElementAt(i);
                if (element?.Measured is false)
                {
                    element.Element?.Measure(new Size(ColumnWidth, availableSize.Height));
                    element.Measured = true;
                }
                int col;
                double top;
                if (state.PositionCache.Count < Columns)
                {
                    col = state.PositionCache.Count;
                    top = Padding.Top;
                }
                else
                {
                    col = state.PositionCache.GetNextTargetColumn(out top);
                    top += RowSpacing;
                }
                state.PositionCache.Add(new StaggeredLayoutPosition(col, top, element!.Element!.DesiredSize.Height));
                x = top;
            }

            if (x < context.RealizationRect.Top)
            {
                state.ElementCache.RecycleElementAt(i);
            }
            else if (x > context.RealizationRect.Bottom)
            {
                state.ElementCache.RemoveElementsFrom(i);
                break;
            }
        }

        var estimatedHeight = state.PositionCache.AverageHeight * context.ItemCount;
        return new Size(availableSize.Width, estimatedHeight);
    }
    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var horizontalOffset = Padding.Left;
        var totalWidth = ColumnWidth + ((Columns - 1) * (ColumnWidth + _columnSpacing));
        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Right:
                horizontalOffset += finalSize.Width - Padding.Left - Padding.Right - totalWidth;
                break;
            case HorizontalAlignment.Center:
                horizontalOffset += (finalSize.Width - Padding.Left - Padding.Right - totalWidth) / 2;
                break;
        }

        var state = (StaggeredLayoutState)context.LayoutState;
        state.PositionCache.GetRealizationBound(context.RealizationRect, out var start, out var end);

        for (var i = start; i < end; i++)
        {
            var element = state.ElementCache.GetOrCreateElementAt(i);
            var position = state.PositionCache[i];
            var x = horizontalOffset + (ColumnWidth + ColumnSpacing) * position.Column;
            var bound = new Rect(x, position.Top, ColumnWidth, position.Height);
            element?.Element?.Arrange(bound);
        }

        return base.ArrangeOverride(context, finalSize);
    }
    protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        var state = (StaggeredLayoutState)context.LayoutState;
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                state.PositionCache.RemoveFrom(0);
                state.ElementCache.RemoveElementsFrom(0);
                break;
            case NotifyCollectionChangedAction.Add:
                state.PositionCache.RemoveFrom(args.NewStartingIndex);
                state.ElementCache.ReserveSpaceAt(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                state.PositionCache.RemoveFrom(args.OldStartingIndex);
                state.ElementCache.RemoveElementAt(args.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                state.PositionCache.RemoveFrom(args.OldStartingIndex);
                state.ElementCache.InvalidateMeasureAt(args.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Move:
                var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
                state.PositionCache.RemoveFrom(start);
                state.ElementCache.MoveElement(args.OldStartingIndex, args.NewStartingIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        InvalidateMeasure();
    }

    protected override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = new StaggeredLayoutState(context);
    }
    protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = null;
    }

    private void GetMetrics(Size availableSize, out int columnsCount, out double columnWidth)
    {
        // Remove padding.
        var availableWidth = availableSize.Width - Padding.Left - Padding.Right;

        // Column width cannot be wider than the available width.
        columnWidth = Math.Min(DesiredColumnWidth, availableWidth);
        // There should be at least 1 column.
        columnsCount = Math.Max(1, (int)Math.Floor(availableWidth / columnWidth));

        // If horizontal alignment is stretch,
        // Then the entire width is divided.
        if (HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            var contentWidth = availableWidth - ((columnsCount - 1) * ColumnSpacing);
            columnWidth = contentWidth / columnsCount;
            return;
        }

        // If not, we first need to see if space is enough
        // for columns and spacing.
        var totalWidth = columnWidth + ((columnsCount - 1) * (columnWidth + ColumnSpacing));
        // If not, remove one column.
        // Note that if there is only one column, this will never be executed.
        if (totalWidth > availableWidth)
        {
            columnsCount--;
        }
    }
    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var layout = (StaggeredLayout)d;

        // For col spacing and padding and desired width change, we need to invalidate measure cache.
        // for row others, we need to invalidate position cache.

        if (e.Property == RowSpacingProperty)
        {
            layout._rowSpacing = (double)e.NewValue;
            layout.InvalidatePositionCache = true;
        }
        else if (e.Property == ColumnSpacingProperty)
        {
            layout._columnSpacing = (double)e.NewValue;
            layout.InvalidateMeasureCache = true;
            layout.InvalidatePositionCache = true;
        }
        else if (e.Property == DesiredColumnWidthProperty)
        {
            layout._desiredColumnWidth = (double)e.NewValue;
            layout.InvalidateMeasureCache = true;
            layout.InvalidatePositionCache = true;
        }
        else if (e.Property == PaddingProperty)
        {
            layout.InvalidateMeasureCache = true;
            layout.InvalidatePositionCache = true;
        }
        else if (e.Property == HorizontalAlignmentProperty)
        {
            layout.InvalidatePositionCache = true;
        }

        layout.InvalidateMeasure();
    }

    private int Columns { get; set; }
    private double ColumnWidth { get; set; }
    private bool InvalidateMeasureCache { get; set; }
    private bool InvalidatePositionCache { get; set; }

    private double _rowSpacing;
    private double _columnSpacing;
    private double _desiredColumnWidth = 250.0;
}

internal class StaggeredLayoutState
{
    public StaggeredLayoutState(VirtualizingLayoutContext context) => ElementCache = new ElementCache(context);

    public Rect LastViewport { get; private set; }
    public ElementCache ElementCache { get; }
    public PositionCache PositionCache { get; } = new();

    public ViewportChangeReason UpdateViewport(Rect viewport)
    {
        ViewportChangeReason reason;
        if (Math.Abs(LastViewport.Width - viewport.Width) > 0.1)
        {
            reason = ViewportChangeReason.HorizontalResize;
        }
        else if (Math.Abs(LastViewport.Height - viewport.Height) > 0.1)
        {
            reason = ViewportChangeReason.VerticalResize;
        }
        else
        {
            reason = ViewportChangeReason.Scroll;
        }
        LastViewport = viewport;
        return reason;
    }
}

internal class ElementCache
{
    public ElementCache(VirtualizingLayoutContext context) => _context = context;

    public bool ContainsCacheFor(int index)
    {
        return index < _elementCache.Count && _elementCache[index] != null;
    }
    public void ReserveSpaceAt(int index)
    {
        if (index < _elementCache.Count)
        {
            _elementCache.Insert(index, null);
        }
    }
    public ElementCacheItem? GetOrCreateElementAt(int index)
    {
        if (ContainsCacheFor(index))
        {
            return _elementCache[index];
        }
        else
        {
            EnsureCount(index + 1);
            var element = _context.GetOrCreateElementAt(index, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
            _elementCache[index] = new ElementCacheItem { Element = element };
            return _elementCache[index];
        }
    }
    public void InvalidateAllMeasure()
    {
        for (var i = 0; i < _elementCache.Count; i++)
        {
            InvalidateMeasureAt(i);
        }
    }
    public void InvalidateMeasureAt(int index)
    {
        if (ContainsCacheFor(index))
        {
            _elementCache[index]!.Measured = false;
        }
    }
    public void RemoveElementAt(int index)
    {
        RecycleElementAt(index);
        if (index < _elementCache.Count)
        {
            _elementCache.RemoveAt(index);
        }
    }
    public void RecycleElementAt(int index)
    {
        if (ContainsCacheFor(index))
        {
            _context.RecycleElement(_elementCache[index]?.Element);
            _elementCache[index] = null;
        }
        TrimEnd();
    }
    public void RemoveElementsFrom(int start)
    {
        while (start < _elementCache.Count)
        {
            RemoveElementAt(start);
        }
    }
    public void MoveElement(int oldIndex, int newIndex)
    {
        if (oldIndex < _elementCache.Count)
        {
            var element = _elementCache[oldIndex];
            _elementCache.RemoveAt(oldIndex);
            EnsureCount(newIndex + 1);
            ReserveSpaceAt(newIndex);
            _elementCache[newIndex] = element;
        }
    }

    private void EnsureCount(int count)
    {
        while (_elementCache.Count < count)
        {
            _elementCache.Add(null);
        }
    }
    private void TrimEnd()
    {
        while (_elementCache.Count > 0 && _elementCache[^1] == null)
        {
            _elementCache.RemoveAt(_elementCache.Count - 1);
        }
    }

    private readonly List<ElementCacheItem?> _elementCache = new();
    private readonly VirtualizingLayoutContext _context;
}

internal class PositionCache
{
    public int Count => _positionCache.Count;
    public double CachedHeight { get; private set; }
    public double AverageHeight => CachedHeight / Count;
    public StaggeredLayoutPosition this[int index] => _positionCache[index];

    public void RemoveFrom(int start)
    {
        if (start < _positionCache.Count)
        {
            _positionCache.RemoveRange(start, _positionCache.Count - start);
        }

        CachedHeight = _positionCache.Count == 0 ? 0.0 : _positionCache.Max(x => x.Bottom);
    }
    public void Add(StaggeredLayoutPosition position)
    {
        _positionCache.Add(position);
        if (position.Bottom > CachedHeight)
        {
            CachedHeight = position.Bottom;
        }
    }
    public void GetRealizationBound(Rect viewport, out int start, out int end)
    {
        if (_positionCache.Count == 0)
        {
            start = 0;
            end = 0;
            return;
        }

        var first = _positionCache.First(x => x.Top > viewport.Top);
        start = _positionCache.IndexOf(first);
        if (_positionCache.Exists(x => x.Top > viewport.Bottom))
        {
            var last = _positionCache.First(x => x.Top > viewport.Bottom);
            end = _positionCache.IndexOf(last);
        }
        else
        {
            end = _positionCache.Count;
        }
    }
    public int GetNextTargetColumn(out double bottom)
    {
        var groups = _positionCache.GroupBy(x => x.Column).ToArray();

        var minY = double.MaxValue;
        var col = 0;
        for (var i = 0; i < groups.Length; i++)
        {
            var bot = groups.First(x => x.Key == i).Last().Bottom;
            if (bot < minY)
            {
                minY = bot;
                col = i;
            }
        }
        bottom = minY;
        return col;
    }

    private readonly List<StaggeredLayoutPosition> _positionCache = new();
}

internal struct StaggeredLayoutPosition
{
    public StaggeredLayoutPosition(int column, double top, double height)
    {
        Column = column;
        Top = top;
        Height = height;
    }

    public int Column { get; set; }
    public double Top { get; set; }
    public double Bottom => Top + Height;
    public double Height { get; set; }
}

internal class ElementCacheItem
{
    public UIElement? Element { get; set; }
    public bool Measured { get; set; }
}

internal enum ViewportChangeReason
{
    Scroll,
    VerticalResize,
    HorizontalResize
}