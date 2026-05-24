// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;
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

    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<AdaptiveGrid, double>(nameof(ItemWidth), double.NaN);

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<AdaptiveGrid, double>(nameof(ItemHeight), double.NaN);

    static AdaptiveGrid()
    {
        AffectsMeasure<AdaptiveGrid>(ItemSpacingProperty, LineSpacingProperty, OrientationProperty, ItemWidthProperty, ItemHeightProperty);
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

        foreach (var t in children.Where(t => !t.IsVisible))
        {
            t.Arrange(default);
        }

        for (var lineIndex = 0; lineIndex < layout.LineCount; ++lineIndex)
        {
            if (lineIndex > 0)
                accumulatedMajor += layout.LineSpacing;

            var line = layout.Lines[lineIndex];
            var minorStart = layout.GetLineMinorStart(lineIndex);
            var minorSpacing = layout.SlotSpacing;
            var slotMinor = layout.SlotMinor;

            for (var itemIndex = 0; itemIndex < line.Count; ++itemIndex)
            {
                var childIndex = layout.VisibleIndices[line.Start + itemIndex];
                children[childIndex].Arrange(this.MinorMajorRect(minorStart, accumulatedMajor, slotMinor, line.Major));
                minorStart += slotMinor + minorSpacing;
            }

            accumulatedMajor += line.Major;
        }

        return finalSize;
    }

    private LayoutState BuildLayout(Size availableSize, bool measureChildren)
    {
        var itemSpacing = ItemSpacing;
        var lineSpacing = LineSpacing;
        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        var itemWidthSet = !double.IsNaN(itemWidth);
        var itemHeightSet = !double.IsNaN(itemHeight);
        var itemMinorSet = this.Minor(itemWidthSet, itemHeightSet);
        var itemMajorSet = this.Major(itemWidthSet, itemHeightSet);
        var configuredMinor = this.Minor(itemWidth, itemHeight);
        var configuredMajor = this.Major(itemWidth, itemHeight);
        var children = Children;

        var childConstraint = new Size(
            itemWidthSet ? itemWidth : availableSize.Width,
            itemHeightSet ? itemHeight : availableSize.Height);

        var baseMinor = itemMinorSet ? configuredMinor : 0;
        var visibleCount = 0;
        var visibleIndices = new List<int>(children.Count);

        for (var i = 0; i < children.Count; ++i)
        {
            var child = children[i];
            if (measureChildren)
                child.Measure(childConstraint);

            if (!child.IsVisible)
                continue;

            ++visibleCount;
            visibleIndices.Add(i);
            if (!itemMinorSet)
                baseMinor = Max(baseMinor, this.Minor(child.DesiredSize));
        }

        if (visibleCount == 0)
            return new LayoutState([], [], 0, 0, lineSpacing, itemSpacing, 0, ItemsAlignment, this, new Size());

        if (!MathUtilities.GreaterThan(baseMinor, 0))
            baseMinor = 1;

        var itemsPerLine = GetItemsPerLine(this.Minor(availableSize), baseMinor, itemSpacing, visibleCount);
        var measuredMinor = (itemsPerLine * baseMinor) + (Max(0, itemsPerLine - 1) * itemSpacing);
        var panelMinor = measuredMinor;
        var itemsAlignment = ItemsAlignment;

        if (this.Minor(availableSize) is double.PositiveInfinity)
            itemsAlignment = WrapPanelItemsAlignment.Start;

        if (itemsAlignment is WrapPanelItemsAlignment.Justify or WrapPanelItemsAlignment.Stretch &&
            !double.IsPositiveInfinity(this.Minor(availableSize)))
            panelMinor = this.Minor(availableSize);

        var lines = new LineState[(visibleCount + itemsPerLine - 1) / itemsPerLine];
        var actualLineCount = 0;
        var currentStart = -1;
        var currentCount = 0;
        double currentMajor = 0;

        for (var i = 0; i < visibleIndices.Count; ++i)
        {
            var childIndex = visibleIndices[i];
            var child = children[childIndex];

            if (currentCount == 0)
            {
                currentStart = i;
                currentMajor = 0;
            }

            currentCount++;
            currentMajor = Max(currentMajor, itemMajorSet ? configuredMajor : this.Major(child.DesiredSize));

            if (currentCount == itemsPerLine)
            {
                lines[actualLineCount++] = new LineState(currentStart, currentCount, currentMajor);
                currentStart = -1;
                currentCount = 0;
                currentMajor = 0;
            }
        }

        if (currentCount > 0)
            lines[actualLineCount++] = new LineState(currentStart, currentCount, currentMajor);

        double totalMajor = 0;
        for (var i = 0; i < actualLineCount; ++i)
            totalMajor += lines[i].Major;

        if (actualLineCount > 1)
            totalMajor += lineSpacing * (actualLineCount - 1);

        var panelSize = this.MinorMajorSize(panelMinor, totalMajor);
        return new LayoutState(lines, [.. visibleIndices], actualLineCount, itemsPerLine, lineSpacing,
            itemSpacing, baseMinor, itemsAlignment, this, panelSize);
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

    private readonly record struct LineState(int Start, int Count, double Major);

    private readonly record struct LayoutState(
        LineState[] Lines,
        int[] VisibleIndices,
        int LineCount,
        int ItemsPerLine,
        double LineSpacing,
        double ItemSpacing,
        double BaseMinor,
        WrapPanelItemsAlignment ItemsAlignment,
        IOrientationBasedMeasures Measures,
        Size PanelSize)
    {
        private double AvailableMinor => Measures.Minor(PanelSize);

        public double SlotMinor => ItemsAlignment switch
        {
            WrapPanelItemsAlignment.Stretch when ItemsPerLine > 0 =>
                (AvailableMinor - (Max(0, ItemsPerLine - 1) * ItemSpacing) - 0.01) / ItemsPerLine,
            _ => BaseMinor
        };

        public double SlotSpacing => ItemsAlignment switch
        {
            WrapPanelItemsAlignment.Justify when ItemsPerLine > 1 =>
                (AvailableMinor - (ItemsPerLine * BaseMinor) - 0.01) / (ItemsPerLine - 1),
            _ => ItemSpacing
        };

        private double SlotStride => SlotMinor + SlotSpacing;

        private int GetLeadingEmptySlotCount(int lineIndex)
        {
            var line = Lines[lineIndex];
            var emptySlotCount = Max(0, ItemsPerLine - line.Count);
            return ItemsAlignment switch
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
