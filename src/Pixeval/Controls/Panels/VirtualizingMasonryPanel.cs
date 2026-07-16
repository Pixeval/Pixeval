// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Pixeval.Controls;

/// <summary>
/// Virtualizes a masonry layout while retaining natural item sizes and column placement state.
/// </summary>
public class VirtualizingMasonryPanel : CachedVirtualizingPanel, IOrientationBasedMeasures
{
    public static readonly StyledProperty<double> ColumnWidthProperty =
        MasonryPanel.ColumnWidthProperty.AddOwner<VirtualizingMasonryPanel>();

    public static readonly StyledProperty<double> ColumnSpacingProperty =
        MasonryPanel.ColumnSpacingProperty.AddOwner<VirtualizingMasonryPanel>();

    public static readonly StyledProperty<double> RowSpacingProperty =
        MasonryPanel.RowSpacingProperty.AddOwner<VirtualizingMasonryPanel>();

    public static readonly StyledProperty<double> CacheLengthProperty =
        AvaloniaProperty.Register<VirtualizingMasonryPanel, double>(nameof(CacheLength), 0.5, validate: value => value is >= 0 and <= 2);

    private readonly List<Size?> _naturalSizes = [];
    private readonly List<Rect> _itemRects = [];
    private readonly List<List<int>> _columnItems = [];
    private readonly HashSet<int> _lastRealizedIndices = [];

    private Rect _viewport;
    private LayoutKey _layoutKey;
    private double[] _columnHeights = [];
    private int _layoutItemCount;
    private int _unmeasuredCount;
    private bool _layoutValid;
    private bool _isInLayout;
    private Size _panelSize;

    static VirtualizingMasonryPanel()
    {
        AffectsMeasure<VirtualizingMasonryPanel>(
            ColumnWidthProperty,
            ColumnSpacingProperty,
            RowSpacingProperty,
            CacheLengthProperty,
            HorizontalAlignmentProperty);
    }

    public VirtualizingMasonryPanel()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    public double ColumnWidth
    {
        get => GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public double CacheLength
    {
        get => GetValue(CacheLengthProperty);
        set => SetValue(CacheLengthProperty, value);
    }

    Orientation IOrientationBasedMeasures.ScrollOrientation => Orientation.Vertical;

    protected override Size MeasureOverride(Size availableSize)
    {
        _isInLayout = true;
        try
        {
            var items = Items;
            EnsureCacheShape(items.Count);
            if (items.Count is 0)
            {
                RecycleAll();
                ResetLayout();
                return default;
            }

            EnsureNaturalSizes(items);
            BuildLayout(availableSize, items.Count);
            var indices = GetRealizationIndices();
            RecycleOutside(indices);
            foreach (var index in indices)
            {
                var child = Realize(items[index], index);
                child.Measure(_itemRects[index].Size);
            }

            _lastRealizedIndices.Clear();
            _lastRealizedIndices.UnionWith(indices);
            return _panelSize;
        }
        finally
        {
            _isInLayout = false;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var (index, child) in Realized)
            child.Arrange(_itemRects[index]);

        return finalSize;
    }

    protected override Control? ScrollIntoView(int index)
    {
        if (_isInLayout || index < 0 || index >= Items.Count || index >= _itemRects.Count)
            return null;

        var child = Realize(Items[index], index);
        child.Measure(_itemRects[index].Size);
        child.Arrange(_itemRects[index]);
        child.BringIntoView();
        InvalidateMeasure();
        return child;
    }

    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewStartingIndex: >= 0, NewItems.Count: > 0 })
        {
            var index = e.NewStartingIndex;
            var count = e.NewItems.Count;
            _naturalSizes.InsertRange(index, Enumerable.Repeat<Size?>(null, count));
            _unmeasuredCount += count;
            ShiftRealizedIndices(index, count);

            var oldCount = items.Count - count;
            if (index != oldCount)
                _layoutValid = false;
        }
        else
        {
            ResetContainers();
            _naturalSizes.Clear();
            _unmeasuredCount = 0;
            ResetLayout();
        }

        _lastRealizedIndices.Clear();
        InvalidateMeasure();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColumnWidthProperty
            || change.Property == ColumnSpacingProperty
            || change.Property == RowSpacingProperty
            || change.Property == HorizontalAlignmentProperty)
            _layoutValid = false;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _viewport = e.EffectiveViewport;
        if (_layoutValid && GetRealizationIndices().SetEquals(_lastRealizedIndices))
            return;

        InvalidateMeasure();
    }

    private void EnsureCacheShape(int itemCount)
    {
        while (_naturalSizes.Count < itemCount)
        {
            _naturalSizes.Add(null);
            _unmeasuredCount++;
        }

        if (_naturalSizes.Count <= itemCount)
            return;

        _naturalSizes.RemoveRange(itemCount, _naturalSizes.Count - itemCount);
        _unmeasuredCount = _naturalSizes.Count(size => size is null);
        _layoutValid = false;
    }

    private void EnsureNaturalSizes(IReadOnlyList<object?> items)
    {
        if (_unmeasuredCount is 0)
            return;

        var width = double.Max(1, ColumnWidth);
        var constraint = new Size(width, double.PositiveInfinity);
        for (var i = 0; i < items.Count; i++)
        {
            if (_naturalSizes[i] is not null)
                continue;

            var wasRealized = Realized.TryGetValue(i, out var child);
            child ??= GetOrCreateElement(items[i], i);
            child.Measure(constraint);
            var size = child.DesiredSize;
            _naturalSizes[i] = IsUsableSize(size) ? size : new Size(width, width);
            _unmeasuredCount--;

            if (!wasRealized)
                RecycleElement(child);
        }
    }

    private void BuildLayout(Size availableSize, int itemCount)
    {
        var layout = GetLayout(availableSize.Width, itemCount);
        var key = new LayoutKey(layout.ColumnCount, layout.ColumnWidth, layout.DesiredWidth, ColumnSpacing, RowSpacing, HorizontalAlignment);
        var canContinue = _layoutValid && _layoutKey == key && _layoutItemCount <= itemCount;
        if (!canContinue)
        {
            _itemRects.Clear();
            _columnItems.Clear();
            _columnHeights = new double[layout.ColumnCount];
            for (var i = 0; i < layout.ColumnCount; i++)
                _columnItems.Add([]);
            _layoutItemCount = 0;
        }

        var horizontalOffset = HorizontalAlignment switch
        {
            HorizontalAlignment.Right => double.Max(0, layout.DesiredWidth - layout.TotalWidth),
            HorizontalAlignment.Center => double.Max(0, layout.DesiredWidth - layout.TotalWidth) / 2,
            _ => 0
        };

        for (var i = _layoutItemCount; i < itemCount; i++)
        {
            var column = GetShortestColumn(_columnHeights);
            var natural = _naturalSizes[i] ?? new Size(layout.ColumnWidth, layout.ColumnWidth);
            var height = IsUsableSize(natural)
                ? layout.ColumnWidth * natural.Height / natural.Width
                : layout.ColumnWidth;
            var y = _columnHeights[column];
            if (_columnItems[column].Count > 0)
                y += RowSpacing;
            var x = horizontalOffset + column * (layout.ColumnWidth + ColumnSpacing);
            _itemRects.Add(new Rect(x, y, layout.ColumnWidth, height));
            _columnItems[column].Add(i);
            _columnHeights[column] = y + height;
        }

        _layoutItemCount = itemCount;
        _layoutKey = key;
        _layoutValid = true;
        _panelSize = new Size(layout.DesiredWidth, _columnHeights.Max());
    }

    private HashSet<int> GetRealizationIndices()
    {
        var result = new HashSet<int>();
        if (_itemRects.Count is 0)
            return result;

        var viewportLength = _viewport.Height;
        if (!double.IsFinite(viewportLength) || viewportLength <= 0)
            viewportLength = _itemRects[0].Height;
        var cache = viewportLength * CacheLength;
        var start = double.Max(0, double.IsFinite(_viewport.Y) ? _viewport.Y - cache : 0);
        var end = double.Min(_panelSize.Height, start + viewportLength + cache * 2);

        foreach (var column in _columnItems)
        {
            var first = FindFirstIntersecting(column, start);
            for (var i = first; i < column.Count; i++)
            {
                var index = column[i];
                if (_itemRects[index].Top > end)
                    break;
                result.Add(index);
            }
        }

        if (result.Count is 0)
            result.Add(0);
        return result;
    }

    private int FindFirstIntersecting(IReadOnlyList<int> column, double start)
    {
        var left = 0;
        var right = column.Count - 1;
        var result = column.Count;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (_itemRects[column[mid]].Bottom >= start)
            {
                result = mid;
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return result;
    }

    private void ResetLayout()
    {
        _itemRects.Clear();
        _columnItems.Clear();
        _columnHeights = [];
        _layoutItemCount = 0;
        _layoutValid = false;
        _panelSize = default;
    }

    private (int ColumnCount, double ColumnWidth, double TotalWidth, double DesiredWidth) GetLayout(double availableWidth, int itemCount)
    {
        if (double.IsNaN(availableWidth) || availableWidth < 0)
            availableWidth = 0;

        var hasFiniteWidth = double.IsFinite(availableWidth);
        var desiredColumnWidth = double.Max(0, ColumnWidth);
        var columnWidth = hasFiniteWidth ? double.Min(desiredColumnWidth, availableWidth) : desiredColumnWidth;
        var columnCount = GetColumnCount(availableWidth, columnWidth, itemCount);

        if (HorizontalAlignment is HorizontalAlignment.Stretch && hasFiniteWidth)
        {
            var spacingWidth = (columnCount - 1) * ColumnSpacing;
            columnWidth = LayoutAlignmentHelper.GetDistributableSpace(availableWidth, spacingWidth) / columnCount;
        }

        var totalWidth = GetTotalWidth(columnWidth, columnCount);
        return (columnCount, columnWidth, totalWidth, hasFiniteWidth ? availableWidth : totalWidth);
    }

    private int GetColumnCount(double availableWidth, double columnWidth, int itemCount)
    {
        if (!double.IsFinite(availableWidth))
            return int.Max(1, itemCount);
        if (columnWidth <= 0)
            return 1;

        var columnCount = int.Max(1, (int) Math.Floor(availableWidth / columnWidth));
        while (columnCount > 1 && GetTotalWidth(columnWidth, columnCount) > availableWidth)
            columnCount--;
        return columnCount;
    }

    private double GetTotalWidth(double columnWidth, int columnCount) =>
        columnWidth + (columnCount - 1) * (columnWidth + ColumnSpacing);

    private static int GetShortestColumn(IReadOnlyList<double> columnHeights)
    {
        var result = 0;
        for (var i = 1; i < columnHeights.Count; i++)
        {
            if (columnHeights[i] < columnHeights[result])
                result = i;
        }

        return result;
    }

    private static bool IsUsableSize(Size size) =>
        double.IsFinite(size.Width) && size.Width > 0
                                    && double.IsFinite(size.Height) && size.Height > 0;

    private readonly record struct LayoutKey(
        int ColumnCount,
        double ColumnWidth,
        double DesiredWidth,
        double ColumnSpacing,
        double RowSpacing,
        HorizontalAlignment HorizontalAlignment);
}
