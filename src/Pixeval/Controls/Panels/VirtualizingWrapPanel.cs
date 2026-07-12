// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// Virtualizes a wrapping layout while retaining natural item sizes and completed line state.
/// </summary>
public class VirtualizingWrapPanel : CachedVirtualizingPanel, IOrientationBasedMeasures
{
    public static readonly StyledProperty<double> ItemSpacingProperty =
        WrapPanel.ItemSpacingProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> LineSpacingProperty =
        WrapPanel.LineSpacingProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<Orientation> OrientationProperty =
        WrapPanel.OrientationProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<WrapPanelItemsAlignment> ItemsAlignmentProperty =
        WrapPanel.ItemsAlignmentProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> ItemWidthProperty =
        WrapPanel.ItemWidthProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> ItemHeightProperty =
        WrapPanel.ItemHeightProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> CacheLengthProperty =
        AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(CacheLength), 0.5, validate: value => value is >= 0 and <= 2);

    private readonly List<Size?> _naturalSizes = [];
    private readonly List<Rect> _itemRects = [];
    private readonly List<LineInfo> _lines = [];

    private Rect _viewport;
    private LayoutKey _layoutKey;
    private Size _panelSize;
    private (int Start, int End) _lastRange = (0, -1);
    private int _layoutItemCount;
    private int _unmeasuredCount;
    private bool _layoutValid;
    private bool _isInLayout;

    static VirtualizingWrapPanel()
    {
        AffectsMeasure<VirtualizingWrapPanel>(
            ItemSpacingProperty,
            LineSpacingProperty,
            OrientationProperty,
            ItemsAlignmentProperty,
            ItemWidthProperty,
            ItemHeightProperty,
            CacheLengthProperty);
    }

    public VirtualizingWrapPanel()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public WrapPanelItemsAlignment ItemsAlignment
    {
        get => GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public double CacheLength
    {
        get => GetValue(CacheLengthProperty);
        set => SetValue(CacheLengthProperty, value);
    }

    Orientation IOrientationBasedMeasures.ScrollOrientation =>
        Orientation is Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;

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
            var range = GetRealizationRange();
            RecycleOutsideRange(range.Start, range.End);
            for (var i = range.Start; i <= range.End; i++)
            {
                var child = Realize(items[i], i);
                child.Measure(_itemRects[i].Size);
            }

            _lastRange = range;
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

        _lastRange = (0, -1);
        InvalidateMeasure();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemSpacingProperty
            || change.Property == LineSpacingProperty
            || change.Property == OrientationProperty
            || change.Property == ItemsAlignmentProperty
            || change.Property == ItemWidthProperty
            || change.Property == ItemHeightProperty)
            _layoutValid = false;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _viewport = e.EffectiveViewport;
        if (_layoutValid && GetRealizationRange() == _lastRange)
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

        var constraint = new Size(
            IsFixed(ItemWidth) ? ItemWidth : double.PositiveInfinity,
            IsFixed(ItemHeight) ? ItemHeight : double.PositiveInfinity);

        for (var i = 0; i < items.Count; i++)
        {
            if (_naturalSizes[i] is not null)
                continue;

            var wasRealized = Realized.TryGetValue(i, out var child);
            child ??= GetOrCreateElement(items[i], i);
            child.Measure(constraint);
            var size = child.DesiredSize;
            _naturalSizes[i] = IsUsableSize(size) ? size : GetFallbackSize();
            _unmeasuredCount--;

            if (!wasRealized)
                RecycleElement(child);
        }
    }

    private void BuildLayout(Size availableSize, int itemCount)
    {
        var availableMinor = this.Minor(availableSize);
        if (!double.IsFinite(availableMinor) || availableMinor <= 0)
            availableMinor = double.PositiveInfinity;

        var key = new LayoutKey(availableMinor, ItemSpacing, LineSpacing, Orientation, ItemsAlignment, ItemWidth, ItemHeight);
        var canContinue = _layoutValid && _layoutKey == key && _layoutItemCount <= itemCount;
        var major = 0d;
        if (!canContinue)
        {
            _itemRects.Clear();
            _lines.Clear();
            _layoutItemCount = 0;
        }
        else if (_layoutItemCount < itemCount && _lines is [.., var previousLastLine])
        {
            _itemRects.RemoveRange(previousLastLine.Start, _itemRects.Count - previousLastLine.Start);
            _layoutItemCount = previousLastLine.Start;
            major = previousLastLine.MajorStart;
            _lines.RemoveAt(_lines.Count - 1);
        }
        else if (_lines is [.., var currentLastLine])
        {
            major = currentLastLine.MajorStart + currentLastLine.MajorSize;
        }

        var lineStart = _layoutItemCount;
        var lineMinor = 0d;
        var lineMajor = 0d;
        for (var i = _layoutItemCount; i < itemCount; i++)
        {
            var size = GetBaseSize(i);
            var itemMinor = this.Minor(size);
            var itemMajor = this.Major(size);
            var spacing = i > lineStart ? ItemSpacing : 0;
            if (i > lineStart && lineMinor + spacing + itemMinor > availableMinor)
            {
                AddLine(lineStart, i, major, lineMajor, lineMinor, availableMinor, isLast: false);
                major += lineMajor + LineSpacing;
                lineStart = i;
                lineMinor = itemMinor;
                lineMajor = itemMajor;
            }
            else
            {
                lineMinor += spacing + itemMinor;
                lineMajor = double.Max(lineMajor, itemMajor);
            }
        }

        if (lineStart < itemCount)
            AddLine(lineStart, itemCount, major, lineMajor, lineMinor, availableMinor, isLast: true);

        _layoutItemCount = itemCount;
        _layoutKey = key;
        _layoutValid = true;

        var panelMajor = _lines is [.., var finalLine] ? finalLine.MajorStart + finalLine.MajorSize : 0;
        var usedMinor = _lines.Count is 0 ? 0 : _lines.Max(line => line.MinorSize);
        var panelMinor = double.IsFinite(availableMinor)
                         && ItemsAlignment is WrapPanelItemsAlignment.Stretch or WrapPanelItemsAlignment.Justify
            ? availableMinor
            : usedMinor;
        _panelSize = this.MinorMajorSize(panelMinor, panelMajor);
    }

    private void AddLine(
        int start,
        int end,
        double majorStart,
        double majorSize,
        double usedMinor,
        double availableMinor,
        bool isLast)
    {
        var count = end - start;
        var spacing = ItemSpacing;
        var minorStart = 0d;
        var stretch = 1d;
        var alignment = ItemsAlignment is WrapPanelItemsAlignment.Stretch && isLast
            ? WrapPanelItemsAlignment.Start
            : ItemsAlignment;

        if (double.IsFinite(availableMinor))
        {
            switch (alignment)
            {
                case WrapPanelItemsAlignment.Start:
                    break;
                case WrapPanelItemsAlignment.Center:
                    minorStart = (availableMinor - usedMinor) / 2;
                    break;
                case WrapPanelItemsAlignment.End:
                    minorStart = availableMinor - usedMinor;
                    break;
                case WrapPanelItemsAlignment.Justify:
                    if (count > 1)
                    {
                        var totalItemMinor = usedMinor - ItemSpacing * (count - 1);
                        spacing = (availableMinor - totalItemMinor) / (count - 1);
                    }

                    break;
                case WrapPanelItemsAlignment.Stretch:
                    var stretchableMinor = usedMinor - ItemSpacing * (count - 1);
                    if (stretchableMinor > 0)
                        stretch = (availableMinor - ItemSpacing * (count - 1)) / stretchableMinor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ItemsAlignment), ItemsAlignment, null);
            }
        }

        var cursor = minorStart;
        for (var i = start; i < end; i++)
        {
            var size = GetBaseSize(i);
            var minor = this.Minor(size) * stretch;
            _itemRects.Add(this.MinorMajorRect(cursor, majorStart, minor, majorSize));
            cursor += minor + spacing;
        }

        _lines.Add(new LineInfo(start, end, majorStart, majorSize, cursor - spacing - minorStart));
    }

    private (int Start, int End) GetRealizationRange()
    {
        if (_lines.Count is 0)
            return (0, -1);

        var viewportStart = this.MajorStart(_viewport);
        if (!double.IsFinite(viewportStart))
            viewportStart = 0;
        var viewportLength = this.MajorSize(_viewport);
        if (!double.IsFinite(viewportLength) || viewportLength <= 0)
            viewportLength = _lines[0].MajorSize;
        var cache = viewportLength * CacheLength;
        var start = double.Max(0, viewportStart - cache);
        var end = double.Min(this.Major(_panelSize), viewportStart + viewportLength + cache);
        var firstLine = FindFirstLine(start);
        var lastLine = FindLastLine(end);
        return (_lines[firstLine].Start, _lines[lastLine].End - 1);
    }

    private int FindFirstLine(double start)
    {
        var left = 0;
        var right = _lines.Count - 1;
        var result = right;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var line = _lines[mid];
            if (line.MajorStart + line.MajorSize >= start)
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

    private int FindLastLine(double end)
    {
        var left = 0;
        var right = _lines.Count - 1;
        var result = 0;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (_lines[mid].MajorStart <= end)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }

    private Size GetBaseSize(int index)
    {
        var natural = _naturalSizes[index] ?? GetFallbackSize();
        var width = IsFixed(ItemWidth) ? ItemWidth : natural.Width;
        var height = IsFixed(ItemHeight) ? ItemHeight : natural.Height;

        if (IsFixed(ItemWidth) && !IsFixed(ItemHeight) && IsUsableSize(natural))
            height = width * natural.Height / natural.Width;
        else if (!IsFixed(ItemWidth) && IsFixed(ItemHeight) && IsUsableSize(natural))
            width = height * natural.Width / natural.Height;

        return new Size(width, height);
    }

    private Size GetFallbackSize()
    {
        var width = IsFixed(ItemWidth) ? ItemWidth : 1;
        var height = IsFixed(ItemHeight) ? ItemHeight : 1;
        return new Size(width, height);
    }

    private void ResetLayout()
    {
        _itemRects.Clear();
        _lines.Clear();
        _layoutItemCount = 0;
        _layoutValid = false;
        _panelSize = default;
    }

    private static bool IsFixed(double value) => double.IsFinite(value) && value > 0;

    private static bool IsUsableSize(Size size) =>
        double.IsFinite(size.Width) && size.Width > 0
                                    && double.IsFinite(size.Height) && size.Height > 0;

    private readonly record struct LineInfo(
        int Start,
        int End,
        double MajorStart,
        double MajorSize,
        double MinorSize);

    private readonly record struct LayoutKey(
        double AvailableMinor,
        double ItemSpacing,
        double LineSpacing,
        Orientation Orientation,
        WrapPanelItemsAlignment ItemsAlignment,
        double ItemWidth,
        double ItemHeight);
}
