// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Pixeval.Controls;

/// <summary>
/// A single-line virtualizing panel that measures every item once and keeps only viewport items realized.
/// </summary>
public class VirtualizingStackPanel : VirtualizingPanel, IScrollSnapPointsInfo
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<VirtualizingStackPanel>();

    public static readonly StyledProperty<double> SpacingProperty =
        StackPanel.SpacingProperty.AddOwner<VirtualizingStackPanel>();

    public static readonly StyledProperty<double> CacheLengthProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, double>(nameof(CacheLength), 0.5, validate: v => v is >= 0 and <= 2);

    public static readonly StyledProperty<double> SnapPointScaleProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, double>(nameof(SnapPointScale), 1, validate: v => double.IsFinite(v) && v > 0);

    public static readonly StyledProperty<double> ViewportScaleProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, double>(nameof(ViewportScale), 1, validate: v => double.IsFinite(v) && v > 0);

    public static readonly StyledProperty<Vector> ViewportOffsetProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, Vector>(nameof(ViewportOffset));

    public static readonly StyledProperty<Size> ViewportSizeProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, Size>(nameof(ViewportSize));

    public static readonly StyledProperty<bool> AreHorizontalSnapPointsRegularProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, bool>(nameof(AreHorizontalSnapPointsRegular));

    public static readonly StyledProperty<bool> AreVerticalSnapPointsRegularProperty =
        AvaloniaProperty.Register<VirtualizingStackPanel, bool>(nameof(AreVerticalSnapPointsRegular));

    public static readonly RoutedEvent<RoutedEventArgs> HorizontalSnapPointsChangedEvent =
        RoutedEvent.Register<VirtualizingStackPanel, RoutedEventArgs>(nameof(HorizontalSnapPointsChanged), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> VerticalSnapPointsChangedEvent =
        RoutedEvent.Register<VirtualizingStackPanel, RoutedEventArgs>(nameof(VerticalSnapPointsChanged), RoutingStrategies.Bubble);

    private static readonly AttachedProperty<object?> RecycleKeyProperty =
        AvaloniaProperty.RegisterAttached<VirtualizingStackPanel, Control, object?>("RecycleKey");

    private static readonly object _ItemIsItsOwnContainer = new();

    private readonly Dictionary<int, Control> _realized = [];
    private readonly Dictionary<object, Stack<Control>> _recyclePool = [];
    private readonly List<Size?> _measuredSizes = [];
    private readonly List<double> _offsets = [];
    private readonly List<double> _sizeU = [];

    private double _extentU;
    private double _maxV;
    private Rect _effectiveViewport;
    private Rect _viewport;
    private Size _lastChildConstraint = new(double.NaN, double.NaN);
    private Size _lastAvailableSize = new(double.NaN, double.NaN);
    private (int Start, int End) _lastRange = (0, -1);
    private int _unmeasuredCount;
    private bool _offsetsDirty = true;
    private int _measureGeneration;
    private bool _isInLayout;

    static VirtualizingStackPanel()
    {
        AffectsMeasure<VirtualizingStackPanel>(
            OrientationProperty,
            SpacingProperty,
            CacheLengthProperty);
    }

    public VirtualizingStackPanel()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    public event EventHandler<RoutedEventArgs>? HorizontalSnapPointsChanged
    {
        add => AddHandler(HorizontalSnapPointsChangedEvent, value);
        remove => RemoveHandler(HorizontalSnapPointsChangedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? VerticalSnapPointsChanged
    {
        add => AddHandler(VerticalSnapPointsChangedEvent, value);
        remove => RemoveHandler(VerticalSnapPointsChangedEvent, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public double CacheLength
    {
        get => GetValue(CacheLengthProperty);
        set => SetValue(CacheLengthProperty, value);
    }

    public double SnapPointScale
    {
        get => GetValue(SnapPointScaleProperty);
        set => SetValue(SnapPointScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets the composition scale applied to the panel by its scrolling ancestor.
    /// </summary>
    public double ViewportScale
    {
        get => GetValue(ViewportScaleProperty);
        set => SetValue(ViewportScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets the current offset supplied by the scrolling ancestor in scaled scroll coordinates.
    /// </summary>
    public Vector ViewportOffset
    {
        get => GetValue(ViewportOffsetProperty);
        set => SetValue(ViewportOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the current viewport size supplied by the scrolling ancestor.
    /// </summary>
    public Size ViewportSize
    {
        get => GetValue(ViewportSizeProperty);
        set => SetValue(ViewportSizeProperty, value);
    }

    public bool AreHorizontalSnapPointsRegular
    {
        get => GetValue(AreHorizontalSnapPointsRegularProperty);
        set => SetValue(AreHorizontalSnapPointsRegularProperty, value);
    }

    public bool AreVerticalSnapPointsRegular
    {
        get => GetValue(AreVerticalSnapPointsRegularProperty);
        set => SetValue(AreVerticalSnapPointsRegularProperty, value);
    }

    public int FirstRealizedIndex => _realized.Count is 0 ? -1 : _realized.Keys.Min();

    public int LastRealizedIndex => _realized.Count is 0 ? -1 : _realized.Keys.Max();

    /// <summary>
    /// Gets the unscaled start offset of an item without realizing its container.
    /// </summary>
    internal bool TryGetItemOffset(int index, out double offset)
    {
        if (_unmeasuredCount is not 0 || index < 0 || index >= _measuredSizes.Count)
        {
            offset = default;
            return false;
        }

        RebuildOffsetsIfDirty();
        offset = _offsets[index];
        return true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _isInLayout = true;

        try
        {
            var items = Items;
            _lastAvailableSize = availableSize;
            EnsureCacheShape(items.Count);

            if (items.Count is 0)
            {
                RecycleAllRealized();
                return default;
            }

            UpdateChildConstraint(GetChildConstraint(availableSize));
            EnsureMeasured(items, availableSize);
            RebuildOffsetsIfDirty();

            var range = GetRealizationRange(items.Count, availableSize);
            RecycleOutsideRange(range.Start, range.End);
            RealizeRange(items, range.Start, range.End, availableSize);

            var childConstraint = GetChildConstraint(availableSize);
            foreach (var (index, container) in _realized)
            {
                container.Measure(childConstraint);
                UpdateMeasuredSizeFromRealized(index, container.DesiredSize);
            }

            RebuildOffsetsIfDirty();
            _lastRange = range;

            var panelV = GetPanelV(availableSize);
            return Orientation is Orientation.Horizontal
                ? new Size(_extentU, panelV)
                : new Size(panelV, _extentU);
        }
        finally
        {
            _isInLayout = false;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var orientation = Orientation;

        foreach (var (index, container) in _realized)
        {
            var size = GetLayoutSize(index);
            var u = GetArrangedU(index);
            var rect = orientation is Orientation.Horizontal
                ? new Rect(u, 0, GetU(size), double.Max(finalSize.Height, GetV(size)))
                : new Rect(0, u, double.Max(finalSize.Width, GetV(size)), GetU(size));

            container.Arrange(rect);
        }

        RaiseSnapPointsChanged(orientation);
        return finalSize;
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
        if (_isInLayout || index < 0 || index >= Items.Count)
            return null;

        if (_measuredSizes.Count <= index || _measuredSizes[index] is null || _offsets.Count <= index)
        {
            InvalidateMeasure();
            return null;
        }

        if (ContainerFromIndex(index) is not { } container)
        {
            container = GetOrCreateElement(Items[index], index);
            _realized[index] = container;
            container.Measure(_lastChildConstraint);
            UpdateMeasuredSizeFromRealized(index, container.DesiredSize);
            RebuildOffsetsIfDirty();
        }

        var size = GetLayoutSize(index);
        var u = GetArrangedU(index);
        var rect = Orientation is Orientation.Horizontal
            ? new Rect(u, 0, GetU(size), GetV(size))
            : new Rect(0, u, GetV(size), GetU(size));
        container.Arrange(rect);
        container.BringIntoView();
        InvalidateMeasure();
        return container;
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
            NavigationDirection.Next => index + 1,
            NavigationDirection.Previous => index is -1 ? -1 : index - 1,
            NavigationDirection.Left when Orientation is Orientation.Horizontal && index is not -1 =>
                index - 1,
            NavigationDirection.Right when Orientation is Orientation.Horizontal && index is not -1 =>
                index + 1,
            NavigationDirection.Up when Orientation is Orientation.Vertical && index is not -1 =>
                index - 1,
            NavigationDirection.Down when Orientation is Orientation.Vertical && index is not -1 =>
                index + 1,
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

    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewStartingIndex: >= 0, NewItems.Count: > 0 })
        {
            var index = e.NewStartingIndex;
            var count = e.NewItems.Count;
            _measuredSizes.InsertRange(index, Enumerable.Repeat<Size?>(null, count));
            _unmeasuredCount += count;
            ShiftRealizedIndices(index, count);
            _lastRange = (0, -1);
            _offsetsDirty = true;
        }
        else
        {
            ResetItems();
        }

        InvalidateMeasure();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SpacingProperty || change.Property == OrientationProperty)
            _offsetsDirty = true;
        else if (change.Property == SnapPointScaleProperty)
            RaiseSnapPointsChanged(Orientation);
        else if (change.Property == ViewportScaleProperty
                 || change.Property == ViewportOffsetProperty
                 || change.Property == ViewportSizeProperty)
            UpdateViewport();
    }

    public IReadOnlyList<double> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment snapPointsAlignment)
    {
        if (orientation != Orientation)
            return [];

        if (SnapPointScale is 1)
            return _offsets;

        return [.. _offsets.Select(offset => offset * SnapPointScale)];
    }

    public double GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment snapPointsAlignment, out double offset)
    {
        offset = 0;
        return 0;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _effectiveViewport = e.EffectiveViewport;
        UpdateViewport();
    }

    private void UpdateViewport()
    {
        _viewport = ResolveUnscaledViewport(
            _effectiveViewport,
            ViewportOffset,
            ViewportSize,
            ViewportScale);

        if (!_offsetsDirty && Items.Count > 0)
        {
            var range = GetRealizationRange(Items.Count, _lastAvailableSize);
            if (range == _lastRange)
                return;
        }

        InvalidateMeasure();
    }

    internal static Rect ResolveUnscaledViewport(
        Rect effectiveViewport,
        Vector viewportOffset,
        Size viewportSize,
        double viewportScale)
    {
        var viewport = IsUsableViewport(viewportOffset, viewportSize)
            ? new Rect(viewportOffset.X, viewportOffset.Y, viewportSize.Width, viewportSize.Height)
            : effectiveViewport;
        return ToUnscaledViewport(viewport, viewportScale);
    }

    internal static Rect ToUnscaledViewport(Rect effectiveViewport, double viewportScale) =>
        new(
            effectiveViewport.X / viewportScale,
            effectiveViewport.Y / viewportScale,
            effectiveViewport.Width / viewportScale,
            effectiveViewport.Height / viewportScale);

    private void RaiseSnapPointsChanged(Orientation orientation) =>
        RaiseEvent(new RoutedEventArgs(orientation is Orientation.Horizontal
            ? HorizontalSnapPointsChangedEvent
            : VerticalSnapPointsChangedEvent));

    private void EnsureCacheShape(int itemCount)
    {
        while (_measuredSizes.Count < itemCount)
        {
            _measuredSizes.Add(null);
            _unmeasuredCount++;
        }

        if (_measuredSizes.Count > itemCount)
        {
            for (var i = itemCount; i < _measuredSizes.Count; i++)
            {
                if (_measuredSizes[i] is null)
                    _unmeasuredCount--;
            }

            _measuredSizes.RemoveRange(itemCount, _measuredSizes.Count - itemCount);
        }
    }

    private void EnsureMeasured(IReadOnlyList<object?> items, Size availableSize)
    {
        if (_unmeasuredCount is 0)
            return;

        var generation = _measureGeneration;
        var childConstraint = GetChildConstraint(availableSize);

        for (var i = 0; i < items.Count; i++)
        {
            if (_measuredSizes[i] is not null)
                continue;

            var wasRealized = _realized.TryGetValue(i, out var container);
            container ??= GetOrCreateElement(items[i], i);
            container.Measure(childConstraint);
            var measuredSize = container.DesiredSize;
            if (!IsUsableSize(measuredSize))
            {
                container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                measuredSize = container.DesiredSize;
            }

            SetMeasuredSize(i, measuredSize);

            if (generation != _measureGeneration)
                return;

            if (!wasRealized)
                RecycleElement(container);
        }
    }

    private void UpdateChildConstraint(Size childConstraint)
    {
        if (_lastChildConstraint == childConstraint)
            return;

        _lastChildConstraint = childConstraint;
        _offsetsDirty = true;
    }

    private void ResetItems()
    {
        RecycleAllRealized(removeOwnContainers: true);
        _recyclePool.Clear();
        _measuredSizes.Clear();
        _offsets.Clear();
        _sizeU.Clear();
        _extentU = 0;
        _maxV = 0;
        _lastRange = (0, -1);
        _unmeasuredCount = 0;
        _offsetsDirty = true;
        _measureGeneration++;
    }

    private void SetMeasuredSize(int index, Size size)
    {
        var wasUnmeasured = _measuredSizes[index] is null;
        if (_measuredSizes[index] == size)
            return;

        _measuredSizes[index] = size;
        if (wasUnmeasured)
            _unmeasuredCount--;
        _offsetsDirty = true;
    }

    private void UpdateMeasuredSizeFromRealized(int index, Size size)
    {
        if (!IsUsableSize(size))
            return;

        if (_measuredSizes[index] is not { } cached
            || !IsUsableSize(cached)
            || double.Abs((cached.Width / cached.Height) - (size.Width / size.Height)) > 0.001)
            SetMeasuredSize(index, size);
    }

    private void RebuildOffsetsIfDirty()
    {
        if (!_offsetsDirty)
            return;

        RebuildOffsets();
        _offsetsDirty = false;
    }

    private void RebuildOffsets()
    {
        _offsets.Clear();
        _sizeU.Clear();

        var spacing = Spacing;
        var accumulated = 0.0;
        var maxV = 0.0;

        for (var i = 0; i < _measuredSizes.Count; i++)
        {
            var size = GetLayoutSize(i);
            var u = GetU(size);
            var v = GetV(size);

            _offsets.Add(accumulated);
            _sizeU.Add(u);
            maxV = double.Max(maxV, v);
            accumulated += u;

            if (i < _measuredSizes.Count - 1)
                accumulated += spacing;
        }

        _extentU = accumulated;
        _maxV = maxV;
    }

    private (int Start, int End) GetRealizationRange(int itemCount, Size availableSize)
    {
        if (itemCount is 0)
            return (0, -1);

        var viewportStart = GetViewportStart();
        var viewportLength = GetViewportLength(availableSize);
        if (viewportLength <= 0 || double.IsInfinity(viewportLength))
            viewportLength = GetU(availableSize);
        if (viewportLength <= 0 || double.IsInfinity(viewportLength))
            viewportLength = _sizeU.Count > 0 ? _sizeU[0] : 0;

        var cache = viewportLength * CacheLength;
        var start = double.Max(0, viewportStart - cache);
        var end = double.Min(_extentU, viewportStart + viewportLength + cache);

        var first = FindFirstIntersecting(start);
        var last = FindLastIntersecting(end);

        if (first < 0 || last < 0)
            return (0, int.Min(1, itemCount) - 1);

        return (int.Clamp(first, 0, itemCount - 1), int.Clamp(last, 0, itemCount - 1));
    }

    private double GetViewportStart()
    {
        var start = Orientation is Orientation.Horizontal ? _viewport.X : _viewport.Y;
        return double.IsFinite(start) ? double.Clamp(start, 0, double.Max(0, _extentU)) : 0;
    }

    private double GetViewportLength(Size availableSize)
    {
        var length = Orientation is Orientation.Horizontal ? _viewport.Width : _viewport.Height;
        if (double.IsFinite(length) && length > 0)
            return length;

        return GetU(availableSize);
    }

    private int FindFirstIntersecting(double u)
    {
        var left = 0;
        var right = _offsets.Count - 1;
        var result = right;
        while (left <= right)
        {
            var mid = left + ((right - left) / 2);
            if (_offsets[mid] + _sizeU[mid] >= u)
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

    private int FindLastIntersecting(double u)
    {
        var left = 0;
        var right = _offsets.Count - 1;
        var result = left;
        while (left <= right)
        {
            var mid = left + ((right - left) / 2);
            if (_offsets[mid] <= u)
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

    private void RealizeRange(IReadOnlyList<object?> items, int start, int end, Size availableSize)
    {
        if (end < start)
            return;

        var childConstraint = GetChildConstraint(availableSize);

        for (var i = start; i <= end; i++)
        {
            if (_realized.ContainsKey(i))
                continue;

            var container = GetOrCreateElement(items[i], i);
            _realized[i] = container;
            container.Measure(childConstraint);
        }
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
            RecycleElement(container);
        }
    }

    private void RecycleAllRealized(bool removeOwnContainers = false)
    {
        foreach (var container in _realized.Values.ToArray())
            RecycleElement(container, removeOwnContainers);

        _realized.Clear();
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

    private Size GetChildConstraint(Size availableSize)
    {
        var availableV = GetV(availableSize);
        if (!double.IsFinite(availableV) || availableV <= 0)
            availableV = Orientation is Orientation.Horizontal ? _viewport.Height : _viewport.Width;
        if (!double.IsFinite(availableV) || availableV <= 0)
            availableV = double.PositiveInfinity;

        return Orientation is Orientation.Horizontal
            ? new Size(double.PositiveInfinity, availableV)
            : new Size(availableV, double.PositiveInfinity);
    }

    private Size GetLayoutSize(int index)
    {
        if (_measuredSizes[index] is not { } measured || !IsUsableSize(measured))
            return default;

        if (Orientation is Orientation.Horizontal
            && double.IsFinite(_lastChildConstraint.Height)
            && _lastChildConstraint.Height > 0)
            return new Size(_lastChildConstraint.Height * measured.Width / measured.Height, _lastChildConstraint.Height);

        if (Orientation is Orientation.Vertical
            && double.IsFinite(_lastChildConstraint.Width)
            && _lastChildConstraint.Width > 0)
            return new Size(_lastChildConstraint.Width, _lastChildConstraint.Width * measured.Height / measured.Width);

        return measured;
    }

    private double GetPanelV(Size availableSize)
    {
        var availableV = GetV(availableSize);
        if (!double.IsFinite(availableV) || availableV <= 0)
            availableV = Orientation is Orientation.Horizontal ? _viewport.Height : _viewport.Width;
        return double.IsFinite(availableV) && availableV > 0 ? double.Max(_maxV, availableV) : _maxV;
    }

    private void ShiftRealizedIndices(int startingIndex, int delta)
    {
        if (delta is 0 || _realized.Count is 0)
            return;

        var generator = ItemContainerGenerator ?? throw new InvalidOperationException($"The panel is not attached to an {nameof(ItemsControl)}.");
        var shifted = _realized
            .OrderByDescending(pair => pair.Key)
            .Where(pair => pair.Key >= startingIndex)
            .ToArray();

        foreach (var (oldIndex, container) in shifted)
        {
            _realized.Remove(oldIndex);
            var newIndex = oldIndex + delta;
            _realized.Add(newIndex, container);
            generator.ItemContainerIndexChanged(container, oldIndex, newIndex);
        }
    }

    private static bool IsUsableSize(Size size) =>
        double.IsFinite(size.Width) && size.Width > 0
                                    && double.IsFinite(size.Height) && size.Height > 0;

    private static bool IsUsableViewport(Vector offset, Size size) =>
        double.IsFinite(offset.X)
        && double.IsFinite(offset.Y)
        && IsUsableSize(size);

    private double GetArrangedU(int index) => _offsets[index];

    private double GetU(Size size) => Orientation is Orientation.Horizontal ? size.Width : size.Height;

    private double GetV(Size size) => Orientation is Orientation.Horizontal ? size.Height : size.Width;
}
