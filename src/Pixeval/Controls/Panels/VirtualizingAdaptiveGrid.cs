// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace Pixeval.Controls;

/// <summary>
/// Virtualizes a uniform adaptive grid with fixed item and line sizes.
/// </summary>
public class VirtualizingAdaptiveGrid : VirtualizingPanel, INavigableContainer, IOrientationBasedMeasures, IAdaptiveGridLayoutInfo
{
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, double>(nameof(ItemSpacing));

    public static readonly DirectProperty<VirtualizingAdaptiveGrid, int> LinesProperty =
        AvaloniaProperty.RegisterDirect<VirtualizingAdaptiveGrid, int>(nameof(Lines), control => control.Lines);

    public static readonly DirectProperty<VirtualizingAdaptiveGrid, int> ItemsPerLineProperty =
        AvaloniaProperty.RegisterDirect<VirtualizingAdaptiveGrid, int>(nameof(ItemsPerLine), control => control.ItemsPerLine);

    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, double>(nameof(LineSpacing));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<WrapPanelItemsAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, WrapPanelItemsAlignment>(nameof(ItemsAlignment), WrapPanelItemsAlignment.Stretch);

    public static readonly StyledProperty<double> ItemSizeProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, double>(nameof(ItemSize), double.NaN);

    public static readonly StyledProperty<double> LineSizeProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, double>(nameof(LineSize), double.NaN);

    public static readonly StyledProperty<int> MaxLinesProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, int>(nameof(MaxLines), -1, validate: value => value >= -1);

    public static readonly StyledProperty<int> MaxItemsInLineProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, int>(nameof(MaxItemsInLine), -1, validate: value => value >= -1);

    public static readonly StyledProperty<int> MinLinesProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, int>(nameof(MinLines), -1, validate: value => value >= -1);

    public static readonly StyledProperty<int> MinItemsInLineProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, int>(nameof(MinItemsInLine), -1, validate: value => value >= -1);

    public static readonly StyledProperty<double> CacheLengthProperty =
        AvaloniaProperty.Register<VirtualizingAdaptiveGrid, double>(nameof(CacheLength), 0.5, validate: value => value is >= 0 and <= 2);

    private static readonly AttachedProperty<object?> RecycleKeyProperty =
        AvaloniaProperty.RegisterAttached<VirtualizingAdaptiveGrid, Control, object?>("RecycleKey");

    private static readonly object _ItemIsItsOwnContainer = new();

    private readonly Dictionary<int, Control> _realized = [];
    private readonly Dictionary<object, Stack<Control>> _recyclePool = [];

    private Orientation _scrollOrientation = Orientation.Vertical;
    private Rect _viewport;
    private LayoutSnapshot _layout;
    private Size _lastAvailableSize = new(double.NaN, double.NaN);
    private Size _lastChildConstraint = new(double.NaN, double.NaN);
    private (int Start, int End) _lastRange = (0, -1);
    private readonly HashSet<int> _needsMeasure = [];
    private bool _isInLayout;

    static VirtualizingAdaptiveGrid()
    {
        AffectsMeasure<VirtualizingAdaptiveGrid>(
            ItemSpacingProperty,
            LineSpacingProperty,
            OrientationProperty,
            ItemsAlignmentProperty,
            ItemSizeProperty,
            LineSizeProperty,
            MaxLinesProperty,
            MaxItemsInLineProperty,
            MinLinesProperty,
            MinItemsInLineProperty,
            CacheLengthProperty);
        AffectsArrange<VirtualizingAdaptiveGrid>(ItemsAlignmentProperty);
    }

    public VirtualizingAdaptiveGrid()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public int Lines
    {
        get;
        private set => SetAndRaise(LinesProperty, ref field, value);
    }

    public int ItemsPerLine
    {
        get;
        private set => SetAndRaise(ItemsPerLineProperty, ref field, value);
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

    public double ItemSize
    {
        get => GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
    }

    public double LineSize
    {
        get => GetValue(LineSizeProperty);
        set => SetValue(LineSizeProperty, value);
    }

    public int MaxLines
    {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

    public int MaxItemsInLine
    {
        get => GetValue(MaxItemsInLineProperty);
        set => SetValue(MaxItemsInLineProperty, value);
    }

    public int MinLines
    {
        get => GetValue(MinLinesProperty);
        set => SetValue(MinLinesProperty, value);
    }

    public int MinItemsInLine
    {
        get => GetValue(MinItemsInLineProperty);
        set => SetValue(MinItemsInLineProperty, value);
    }

    public double CacheLength
    {
        get => GetValue(CacheLengthProperty);
        set => SetValue(CacheLengthProperty, value);
    }

    Orientation IOrientationBasedMeasures.ScrollOrientation => _scrollOrientation;

    /// <summary>
    /// Gets whether the finite scroll-axis size is a hard layout capacity instead of a viewport over a scrollable extent.
    /// </summary>
    protected virtual bool ConstrainToAvailableMajor => false;

    protected override Size MeasureOverride(Size availableSize)
    {
        _isInLayout = true;

        try
        {
            var items = Items;
            _lastAvailableSize = availableSize;
            if (items.Count is 0)
            {
                RecycleAllRealized();
                _layout = default;
                Lines = 0;
                ItemsPerLine = 0;
                _lastRange = (0, -1);
                return default;
            }

            _layout = BuildLayout(availableSize, items.Count);
            var range = GetRealizationRange(items.Count);
            RecycleOutsideRange(range.Start, range.End);
            RealizeRange(items, range.Start, range.End);

            var measureAll = _lastChildConstraint != _layout.ChildConstraint;
            foreach (var (index, child) in _realized)
            {
                if (measureAll || _needsMeasure.Remove(index))
                    child.Measure(_layout.ChildConstraint);
            }

            _lastChildConstraint = _layout.ChildConstraint;
            _lastRange = range;
            Lines = _layout.ViewportLineCount;
            ItemsPerLine = _layout.ItemsPerLine;
            return _layout.PanelSize;
        }
        finally
        {
            _isInLayout = false;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var layout = _layout;

        foreach (var (index, child) in _realized)
            child.Arrange(GetItemRect(index, layout));

        return finalSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
            _scrollOrientation = Orientation is Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
    }

    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewStartingIndex: >= 0, NewItems.Count: > 0 })
        {
            ShiftRealizedIndices(e.NewStartingIndex, e.NewItems.Count);
        }
        else
        {
            RecycleAllRealized(removeOwnContainers: true);
            _recyclePool.Clear();
            _needsMeasure.Clear();
        }

        _lastRange = (0, -1);
        InvalidateMeasure();
    }

    protected override Control? ContainerFromIndex(int index) =>
        index >= 0 && index < Items.Count && _realized.TryGetValue(index, out var container)
            ? container
            : null;

    protected override IEnumerable<Control>? GetRealizedContainers() => _realized.Values;

    protected override int IndexFromContainer(Control container)
    {
        foreach (var (index, realized) in _realized)
        {
            if (ReferenceEquals(realized, container))
                return index;
        }

        return -1;
    }

    protected override Control? ScrollIntoView(int index)
    {
        if (_isInLayout
            || index < 0
            || index >= Items.Count
            || index >= _layout.DisplayedItemCount
            || _layout.ItemsPerLine is 0)
            return null;

        if (ContainerFromIndex(index) is not { } container)
        {
            container = GetOrCreateElement(Items[index], index);
            _realized[index] = container;
            container.Measure(_layout.ChildConstraint);
        }

        container.Arrange(GetItemRect(index, _layout));
        container.BringIntoView();
        InvalidateMeasure();
        return container;
    }

    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var count = Items.Count;
        if (count is 0)
            return null;

        var itemsPerLine = int.Max(ItemsPerLine, 1);
        var index = from is Control control ? IndexFromContainer(control) : -1;
        index = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => count - 1,
            NavigationDirection.Next or NavigationDirection.Right or NavigationDirection.Down => index + 1,
            NavigationDirection.Previous or NavigationDirection.Left or NavigationDirection.Up => index - 1,
            NavigationDirection.PageDown => index + itemsPerLine,
            NavigationDirection.PageUp => index - itemsPerLine,
            _ => -1
        };

        if (wrap)
        {
            if (index < 0)
                index = count - 1;
            else if (index >= count)
                index = 0;
        }

        return index >= 0 && index < count ? ScrollIntoView(index) : null;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _viewport = e.EffectiveViewport;

        if (_layout.ItemsPerLine > 0 && Items.Count > 0)
        {
            var range = GetRealizationRange(Items.Count);
            if (range == _lastRange)
                return;
        }

        InvalidateMeasure();
    }

    private LayoutSnapshot BuildLayout(Size availableSize, int itemCount)
    {
        var itemSpacing = ItemSpacing;
        var lineSpacing = LineSpacing;
        var availableMinor = this.Minor(availableSize);
        var availableMajor = this.Major(availableSize);
        var viewportMajor = this.Major(_viewport.Size);
        var baseMinor = !double.IsNaN(ItemSize) && ItemSize > 0 ? ItemSize : 1;
        var baseMajor = !double.IsNaN(LineSize) && LineSize > 0 ? LineSize : 1;
        var minorAlignment = double.IsFinite(availableMinor) ? ItemsAlignment : WrapPanelItemsAlignment.Start;
        var isCapacityConstrained = ConstrainToAvailableMajor && double.IsFinite(availableMajor);
        var majorAlignment = isCapacityConstrained ? ItemsAlignment : WrapPanelItemsAlignment.Start;
        var itemsPerLine = ConstrainCount(GetItemsPerLine(availableMinor, baseMinor, itemSpacing, itemCount), MinItemsInLine, MaxItemsInLine);
        if (itemsPerLine is 0)
            return default;

        var totalLineCount = GetLineCountForItems(itemCount, itemsPerLine);
        int allocatedLineCount;
        int reportedLineCount;
        int displayedItemCount;
        if (isCapacityConstrained)
        {
            allocatedLineCount = ConstrainCount(
                GetLineCount(availableMajor, baseMajor, lineSpacing, int.MaxValue),
                MinLines,
                MaxLines);
            if (allocatedLineCount is 0)
                return default;

            reportedLineCount = allocatedLineCount;
            displayedItemCount = int.Min(itemCount, allocatedLineCount * itemsPerLine);
        }
        else
        {
            allocatedLineCount = totalLineCount;
            displayedItemCount = itemCount;
            if (!double.IsFinite(viewportMajor) || viewportMajor <= 0)
                viewportMajor = double.IsFinite(availableMajor) && availableMajor > 0 ? availableMajor : baseMajor;
            reportedLineCount = int.Min(
                totalLineCount,
                int.Max(1, (int) Math.Ceiling((viewportMajor + lineSpacing) / (baseMajor + lineSpacing))));
            reportedLineCount = ConstrainCount(reportedLineCount, MinLines, MaxLines);
        }

        var lineCount = GetLineCountForItems(displayedItemCount, itemsPerLine);
        var measuredLineCount = majorAlignment is WrapPanelItemsAlignment.Justify or WrapPanelItemsAlignment.Stretch
            ? allocatedLineCount
            : lineCount;

        var panelMinor = GetStackedSize(itemsPerLine, itemSpacing, baseMinor);
        var panelMajor = GetStackedSize(measuredLineCount, lineSpacing, baseMajor);
        if (minorAlignment is WrapPanelItemsAlignment.Justify or WrapPanelItemsAlignment.Stretch)
            panelMinor = availableMinor;
        if (majorAlignment is WrapPanelItemsAlignment.Justify or WrapPanelItemsAlignment.Stretch)
            panelMajor = availableMajor;

        var slotMinor = GetAlignedSize(availableMinor, itemsPerLine, itemSpacing, baseMinor, minorAlignment);
        var effectiveItemSpacing = GetAlignedSpacing(availableMinor, itemsPerLine, itemSpacing, baseMinor, minorAlignment);
        var minorStart = GetAlignedStart(availableMinor, itemsPerLine, itemSpacing, baseMinor, minorAlignment);
        var lineMajor = GetAlignedSize(availableMajor, allocatedLineCount, lineSpacing, baseMajor, majorAlignment);
        var effectiveLineSpacing = GetAlignedSpacing(availableMajor, allocatedLineCount, lineSpacing, baseMajor, majorAlignment);
        var majorStart = GetAlignedStart(availableMajor, allocatedLineCount, lineSpacing, baseMajor, majorAlignment);

        return new(
            itemsPerLine,
            lineCount,
            reportedLineCount,
            displayedItemCount,
            slotMinor,
            lineMajor,
            effectiveItemSpacing,
            effectiveLineSpacing,
            minorStart,
            majorStart,
            panelMajor,
            this.MinorMajorSize(panelMinor, panelMajor),
            this.MinorMajorSize(slotMinor, lineMajor));
    }

    private (int Start, int End) GetRealizationRange(int itemCount)
    {
        if (itemCount is 0 || _layout is not { ItemsPerLine: > 0, LineCount: > 0, DisplayedItemCount: > 0 })
            return (0, -1);

        var viewportStart = this.MajorStart(_viewport);
        if (!double.IsFinite(viewportStart))
            viewportStart = 0;

        var viewportLength = this.MajorSize(_viewport);
        if (!double.IsFinite(viewportLength) || viewportLength <= 0)
            viewportLength = _layout.LineMajor;

        var cache = viewportLength * CacheLength;
        var startMajor = double.Max(_layout.MajorStart, viewportStart - cache);
        var endMajor = double.Min(_layout.PanelMajor, viewportStart + viewportLength + cache);
        var stride = _layout.LineMajor + _layout.LineSpacing;
        var firstLine = int.Clamp((int) Math.Floor((startMajor - _layout.MajorStart) / stride), 0, _layout.LineCount - 1);
        var lastLine = int.Clamp((int) Math.Floor((endMajor - _layout.MajorStart) / stride), 0, _layout.LineCount - 1);
        var start = firstLine * _layout.ItemsPerLine;
        var end = int.Min(_layout.DisplayedItemCount - 1, ((lastLine + 1) * _layout.ItemsPerLine) - 1);
        return (start, end);
    }

    private void RealizeRange(IReadOnlyList<object?> items, int start, int end)
    {
        if (end < start)
            return;

        for (var i = start; i <= end; i++)
        {
            if (_realized.ContainsKey(i))
                continue;

            var container = GetOrCreateElement(items[i], i);
            _realized[i] = container;
            _needsMeasure.Add(i);
        }
    }

    private Rect GetItemRect(int index, LayoutSnapshot layout)
    {
        var line = index / layout.ItemsPerLine;
        var item = index % layout.ItemsPerLine;
        var minor = layout.MinorStart + item * (layout.SlotMinor + layout.ItemSpacing);
        var major = layout.MajorStart + line * (layout.LineMajor + layout.LineSpacing);
        return this.MinorMajorRect(minor, major, layout.SlotMinor, layout.LineMajor);
    }

    private Control GetOrCreateElement(object? item, int index)
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

    private void RecycleOutsideRange(int start, int end)
    {
        foreach (var index in _realized.Keys.ToArray())
        {
            if (index >= start && index <= end)
                continue;

            var container = _realized[index];
            _realized.Remove(index);
            _needsMeasure.Remove(index);
            RecycleElement(container);
        }
    }

    private void RecycleAllRealized(bool removeOwnContainers = false)
    {
        foreach (var container in _realized.Values.ToArray())
            RecycleElement(container, removeOwnContainers);

        _realized.Clear();
        _needsMeasure.Clear();
    }

    private void RecycleElement(Control container, bool removeOwnContainer = false)
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

    private void ShiftRealizedIndices(int startingIndex, int delta)
    {
        if (delta is 0 || _realized.Count is 0)
            return;

        var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");
        var shifted = _realized
            .OrderByDescending(pair => pair.Key)
            .Where(pair => pair.Key >= startingIndex)
            .ToArray();

        foreach (var (oldIndex, container) in shifted)
        {
            _realized.Remove(oldIndex);
            _needsMeasure.Remove(oldIndex);
            var newIndex = oldIndex + delta;
            _realized.Add(newIndex, container);
            _needsMeasure.Add(newIndex);
            generator.ItemContainerIndexChanged(container, oldIndex, newIndex);
        }
    }

    private static int GetItemsPerLine(double availableMinor, double slotMinor, double spacing, int itemCount)
    {
        if (itemCount is 0)
            return 0;

        if (!double.IsFinite(availableMinor) || availableMinor <= 0)
            return itemCount;

        return int.Max(1, (int) Math.Floor((availableMinor + spacing) / (slotMinor + spacing)));
    }

    private static int GetLineCountForItems(int itemCount, int itemsPerLine) =>
        itemsPerLine <= 0 ? 0 : (itemCount + itemsPerLine - 1) / itemsPerLine;

    private static double GetStackedSize(int count, double spacing, double size) =>
        count <= 0 ? 0 : (count * size) + ((count - 1) * spacing);

    private static int ConstrainCount(int count, int min, int max)
    {
        if (max >= 0)
        {
            count = int.Min(count, max);
            if (min >= 0)
                min = int.Min(min, max);
        }

        if (min >= 0)
            count = int.Max(count, min);
        return int.Max(0, count);
    }

    private static int GetLineCount(double availableMajor, double lineMajor, double lineSpacing, int fallbackCount)
    {
        if (fallbackCount <= 0)
            return 0;
        if (!double.IsFinite(availableMajor))
            return fallbackCount;
        if (lineMajor <= 0)
            return 0;

        var count = Math.Floor((availableMajor + lineSpacing) / (lineMajor + lineSpacing));
        if (count >= fallbackCount)
            return fallbackCount;
        return int.Max(0, (int) count);
    }

    private static double GetAlignedStart(
        double availableSize,
        int itemCount,
        double spacing,
        double baseSize,
        WrapPanelItemsAlignment alignment)
    {
        if (!double.IsFinite(availableSize)
            || itemCount <= 0
            || alignment is not (WrapPanelItemsAlignment.Center or WrapPanelItemsAlignment.End))
            return 0;

        var freeSpace = availableSize - GetStackedSize(itemCount, spacing, baseSize);
        if (freeSpace <= 0)
            return 0;
        return alignment is WrapPanelItemsAlignment.Center ? freeSpace / 2 : freeSpace;
    }

    private static double GetAlignedSize(
        double availableSize,
        int itemCount,
        double spacing,
        double baseSize,
        WrapPanelItemsAlignment alignment) =>
        alignment is WrapPanelItemsAlignment.Stretch && itemCount > 0 && double.IsFinite(availableSize)
            ? LayoutAlignmentHelper.GetDistributableSpace(availableSize, int.Max(0, itemCount - 1) * spacing) / itemCount
            : baseSize;

    private static double GetAlignedSpacing(
        double availableSize,
        int itemCount,
        double spacing,
        double baseSize,
        WrapPanelItemsAlignment alignment) =>
        alignment is WrapPanelItemsAlignment.Justify && itemCount > 1 && double.IsFinite(availableSize)
            ? LayoutAlignmentHelper.GetDistributableSpace(availableSize, double.Max(0, itemCount * baseSize)) / (itemCount - 1)
            : spacing;

    private readonly record struct LayoutSnapshot(
        int ItemsPerLine,
        int LineCount,
        int ViewportLineCount,
        int DisplayedItemCount,
        double SlotMinor,
        double LineMajor,
        double ItemSpacing,
        double LineSpacing,
        double MinorStart,
        double MajorStart,
        double PanelMajor,
        Size PanelSize,
        Size ChildConstraint);
}
