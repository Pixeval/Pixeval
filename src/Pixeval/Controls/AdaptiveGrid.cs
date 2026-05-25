// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Pixeval.Utilities;
using static System.Math;

namespace Pixeval.Controls;

/// <summary>
/// Positions child elements in a uniform adaptive grid.
/// The number of items in each row/column is determined automatically from the available size
/// and the minimum item size.
/// </summary>
public class AdaptiveGrid : Panel, INavigableContainer, IOrientationBasedMeasures
{
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<AdaptiveGrid, double>(nameof(ItemSpacing));

    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<AdaptiveGrid, double>(nameof(LineSpacing));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AdaptiveGrid, Orientation>(nameof(Orientation),
            defaultValue: Orientation.Horizontal);

    public static readonly StyledProperty<WrapPanelItemsAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<AdaptiveGrid, WrapPanelItemsAlignment>(nameof(ItemsAlignment),
            defaultValue: WrapPanelItemsAlignment.Stretch);

    public static readonly StyledProperty<double> ItemSizeProperty =
        AvaloniaProperty.Register<AdaptiveGrid, double>(nameof(ItemSize), double.NaN);

    public static readonly StyledProperty<double> LineSizeProperty =
        AvaloniaProperty.Register<AdaptiveGrid, double>(nameof(LineSize), double.NaN);

    public static readonly StyledProperty<int> MaxLinesProperty =
        AvaloniaProperty.Register<AdaptiveGrid, int>(nameof(MaxLines), -1, validate: value => value >= -1);

    public static readonly StyledProperty<int> MaxItemsInLineProperty =
        AvaloniaProperty.Register<AdaptiveGrid, int>(nameof(MaxItemsInLine), -1, validate: value => value >= -1);

    public static readonly StyledProperty<int> MinLinesProperty =
        AvaloniaProperty.Register<AdaptiveGrid, int>(nameof(MinLines), -1, validate: value => value >= -1);

    public static readonly StyledProperty<int> MinItemsInLineProperty =
        AvaloniaProperty.Register<AdaptiveGrid, int>(nameof(MinItemsInLine), -1, validate: value => value >= -1);

    static AdaptiveGrid()
    {
        AffectsMeasure<AdaptiveGrid>(
            ItemSpacingProperty,
            LineSpacingProperty,
            OrientationProperty,
            ItemsAlignmentProperty,
            ItemSizeProperty,
            LineSizeProperty,
            MaxLinesProperty,
            MaxItemsInLineProperty,
            MinLinesProperty,
            MinItemsInLineProperty);
        AffectsArrange<AdaptiveGrid>(ItemsAlignmentProperty);
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

    private Orientation ScrollOrientation { get; set; } = Orientation.Vertical;

    Orientation IOrientationBasedMeasures.ScrollOrientation => ScrollOrientation;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
            ScrollOrientation = Orientation is Orientation.Horizontal
                ? Orientation.Vertical
                : Orientation.Horizontal;
    }

    IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var children = Children;
        if (children.Count is 0)
            return null;

        var index = from is not null ? children.IndexOf((Control) from) : -1;

        switch (direction)
        {
            case NavigationDirection.First:
                index = 0;
                break;
            case NavigationDirection.Last:
                index = children.Count - 1;
                break;
            case NavigationDirection.Next:
            case NavigationDirection.Right:
            case NavigationDirection.Down:
                ++index;
                break;
            case NavigationDirection.Previous:
            case NavigationDirection.Left:
            case NavigationDirection.Up:
                --index;
                break;
        }

        if (wrap && children.Count > 0)
        {
            if (index < 0)
                index = children.Count - 1;
            else if (index >= children.Count)
                index = 0;
        }

        return index >= 0 && index < children.Count ? children[index] : null;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var layout = BuildLayout(constraint, measureChildren: true);
        return layout.PanelSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var layout = BuildLayout(finalSize, measureChildren: false);
        var children = Children;
        double accumulatedMajor = 0;

        var visibleCursor = 0;
        for (var childIndex = 0; childIndex < children.Count; ++childIndex)
        {
            var isArranged = visibleCursor < layout.VisibleIndices.Length && layout.VisibleIndices[visibleCursor] == childIndex;
            if (isArranged)
            {
                ++visibleCursor;
                continue;
            }

            children[childIndex].Arrange(default);
        }

        for (var lineIndex = 0; lineIndex < layout.LineCount; ++lineIndex)
        {
            if (lineIndex > 0)
                accumulatedMajor += layout.LineSlotSpacing;

            var line = layout.Lines[lineIndex];
            var minorStart = layout.GetLineMinorStart(lineIndex);
            var minorSpacing = layout.SlotSpacing;
            var slotMinor = layout.SlotMinor;

            for (var itemIndex = 0; itemIndex < line.Count; ++itemIndex)
            {
                var childIndex = layout.VisibleIndices[line.Start + itemIndex];
                children[childIndex].Arrange(this.MinorMajorRect(minorStart, accumulatedMajor, slotMinor, layout.LineMajor));
                minorStart += slotMinor + minorSpacing;
            }

            accumulatedMajor += layout.LineMajor;
        }

        return finalSize;
    }

    private LayoutState BuildLayout(Size availableSize, bool measureChildren)
    {
        var itemSpacing = ItemSpacing;
        var lineSpacing = LineSpacing;
        var itemSize = ItemSize;
        var lineSize = LineSize;
        var itemSizeSet = !double.IsNaN(itemSize);
        var lineSizeSet = !double.IsNaN(lineSize);
        var minLines = MinLines;
        var maxLines = MaxLines;
        var minItemsInLine = MinItemsInLine;
        var maxItemsInLine = MaxItemsInLine;
        var children = Children;
        var availableMinor = this.Minor(availableSize);
        var availableMajor = this.Major(availableSize);

        var childConstraint = this.MinorMajorSize(
            itemSizeSet ? itemSize : availableMinor,
            lineSizeSet ? lineSize : availableMajor);

        var baseMinor = itemSizeSet ? itemSize : 0;
        var baseMajor = lineSizeSet ? lineSize : 0;
        var visibleIndices = new List<int>(children.Count);

        for (var i = 0; i < children.Count; ++i)
        {
            var child = children[i];
            if (measureChildren)
                child.Measure(childConstraint);

            if (!child.IsVisible)
                continue;

            visibleIndices.Add(i);
            if (!itemSizeSet)
                baseMinor = Max(baseMinor, this.Minor(child.DesiredSize));
            if (!lineSizeSet)
                baseMajor = Max(baseMajor, this.Major(child.DesiredSize));
        }

        if (visibleIndices.Count is 0)
            return new LayoutState([], [], 0, 0, 0, lineSpacing, itemSpacing, 0, 0,
                WrapPanelItemsAlignment.Start, WrapPanelItemsAlignment.Start, this, new Size());

        if (!MathUtilities.GreaterThan(baseMinor, 0))
            baseMinor = 1;
        if (!MathUtilities.GreaterThan(baseMajor, 0))
            baseMajor = 1;

        var normalItemsPerLine = GetItemsPerLine(availableMinor, baseMinor, itemSpacing, visibleIndices.Count);
        var itemsPerLine = ConstrainCount(normalItemsPerLine, minItemsInLine, maxItemsInLine);

        if (itemsPerLine is 0)
            return new LayoutState([], [], 0, 0, 0, lineSpacing, itemSpacing, baseMajor, baseMinor,
                WrapPanelItemsAlignment.Start, WrapPanelItemsAlignment.Start, this, new Size());

        var totalLineCount = GetLineCountForItems(visibleIndices.Count, itemsPerLine);
        var visibleLineCount = totalLineCount;
        if (!double.IsPositiveInfinity(availableMajor))
            visibleLineCount = Min(visibleLineCount, GetLineCount(availableMajor, baseMajor, lineSpacing, totalLineCount));

        var allocatedLineCount = ConstrainCount(visibleLineCount, minLines, maxLines);

        if (allocatedLineCount is 0)
            return new LayoutState([], [], 0, 0, itemsPerLine, lineSpacing, itemSpacing, baseMajor, baseMinor,
                WrapPanelItemsAlignment.Start, WrapPanelItemsAlignment.Start, this, new Size());

        var displayedItemCount = Min(visibleIndices.Count, allocatedLineCount * itemsPerLine);
        var actualLineCount = GetLineCountForItems(displayedItemCount, itemsPerLine);

        var lines = new LineState[actualLineCount];
        for (var lineIndex = 0; lineIndex < actualLineCount; ++lineIndex)
        {
            var start = lineIndex * itemsPerLine;
            lines[lineIndex] = new LineState(start, Min(itemsPerLine, displayedItemCount - start));
        }

        var measuredMinor = (itemsPerLine * baseMinor) + (Max(0, itemsPerLine - 1) * itemSpacing);
        var measuredMajor = (allocatedLineCount * baseMajor) + (Max(0, allocatedLineCount - 1) * lineSpacing);
        var panelMinor = measuredMinor;
        var panelMajor = measuredMajor;
        var minorAlignment = double.IsPositiveInfinity(availableMinor)
            ? WrapPanelItemsAlignment.Start
            : ItemsAlignment;
        var majorAlignment = double.IsPositiveInfinity(availableMajor)
            ? WrapPanelItemsAlignment.Start
            : ItemsAlignment;

        if (minorAlignment is WrapPanelItemsAlignment.Justify or WrapPanelItemsAlignment.Stretch)
            panelMinor = availableMinor;
        if (majorAlignment is WrapPanelItemsAlignment.Justify or WrapPanelItemsAlignment.Stretch)
            panelMajor = availableMajor;

        var panelSize = this.MinorMajorSize(panelMinor, panelMajor);
        return new LayoutState(lines, [.. visibleIndices.GetRange(0, displayedItemCount)], actualLineCount,
            allocatedLineCount, itemsPerLine, lineSpacing, itemSpacing, baseMajor, baseMinor,
            minorAlignment, majorAlignment, this, panelSize);
    }

    private static int GetItemsPerLine(double availableMinor, double itemMinor, double itemSpacing, int fallbackCount)
    {
        if (fallbackCount <= 0)
            return 1;

        if (double.IsPositiveInfinity(availableMinor))
            return fallbackCount;

        if (!MathUtilities.GreaterThan(itemMinor, 0))
            return 1;

        return Max(1, (int)Floor((availableMinor + itemSpacing) / (itemMinor + itemSpacing)));
    }

    private static int GetLineCountForItems(int itemCount, int itemsPerLine)
    {
        if (itemCount <= 0 || itemsPerLine <= 0)
            return 0;

        return (itemCount + itemsPerLine - 1) / itemsPerLine;
    }

    private static int ConstrainCount(int normalCount, int minCount, int maxCount)
    {
        var effectiveMax = maxCount;
        var effectiveMin = minCount;

        if (effectiveMax >= 0)
            effectiveMin = effectiveMin >= 0 ? Min(effectiveMin, effectiveMax) : effectiveMin;

        if (effectiveMax >= 0)
            normalCount = Min(normalCount, effectiveMax);

        if (effectiveMin >= 0)
            normalCount = Max(normalCount, effectiveMin);

        return Max(0, normalCount);
    }

    private static int GetLineCount(double availableMajor, double lineMajor, double lineSpacing, int fallbackCount)
    {
        if (fallbackCount <= 0)
            return 0;

        if (double.IsPositiveInfinity(availableMajor))
            return fallbackCount;

        if (!MathUtilities.GreaterThan(lineMajor, 0))
            return 0;

        return Max(0, (int)Floor((availableMajor + lineSpacing) / (lineMajor + lineSpacing)));
    }

    private static double GetAlignedSize(double availableSize, int itemCount, double spacing, double baseSize, WrapPanelItemsAlignment itemsAlignment)
    {
        return itemsAlignment switch
        {
            WrapPanelItemsAlignment.Stretch when itemCount > 0 =>
                LayoutAlignmentHelper.GetDistributableSpace(availableSize, Max(0, itemCount - 1) * spacing) / itemCount,
            _ => baseSize
        };
    }

    private static double GetAlignedSpacing(double availableSize, int itemCount, double spacing, double baseSize, WrapPanelItemsAlignment itemsAlignment)
    {
        return itemsAlignment switch
        {
            WrapPanelItemsAlignment.Justify when itemCount > 1 =>
                LayoutAlignmentHelper.GetDistributableSpace(availableSize, itemCount * baseSize) / (itemCount - 1),
            _ => spacing
        };
    }

    private readonly record struct LineState(int Start, int Count);

    private readonly record struct LayoutState(
        LineState[] Lines,
        int[] VisibleIndices,
        int LineCount,
        int AllocatedLineCount,
        int ItemsPerLine,
        double LineSpacing,
        double ItemSpacing,
        double BaseMajor,
        double BaseMinor,
        WrapPanelItemsAlignment MinorAlignment,
        WrapPanelItemsAlignment MajorAlignment,
        IOrientationBasedMeasures Measures,
        Size PanelSize)
    {
        private double AvailableMinor => Measures.Minor(PanelSize);
        private double AvailableMajor => Measures.Major(PanelSize);

        public double SlotMinor => GetAlignedSize(AvailableMinor, ItemsPerLine, ItemSpacing, BaseMinor, MinorAlignment);

        public double SlotSpacing => GetAlignedSpacing(AvailableMinor, ItemsPerLine, ItemSpacing, BaseMinor, MinorAlignment);

        public double LineMajor => GetAlignedSize(AvailableMajor, AllocatedLineCount, LineSpacing, BaseMajor, MajorAlignment);

        public double LineSlotSpacing => GetAlignedSpacing(AvailableMajor, AllocatedLineCount, LineSpacing, BaseMajor, MajorAlignment);

        private double SlotStride => SlotMinor + SlotSpacing;

        private int GetLeadingEmptySlotCount(int lineIndex)
        {
            var line = Lines[lineIndex];
            var emptySlotCount = Max(0, ItemsPerLine - line.Count);
            return MinorAlignment switch
            {
                WrapPanelItemsAlignment.Center => emptySlotCount / 2,
                WrapPanelItemsAlignment.End => emptySlotCount,
                _ => 0
            };
        }

        public double GetLineMinorStart(int lineIndex)
        {
            return GetLeadingEmptySlotCount(lineIndex) * SlotStride;
        }
    }
}
