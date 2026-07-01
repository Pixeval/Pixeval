// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// Arranges child elements into a masonry grid pattern where items are added to the column that has used the least amount of space.
/// </summary>
public class MasonryPanel : Panel
{
    /// <summary>
    /// Identifies the <see cref="ColumnWidth"/> styled property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnWidthProperty =
        AvaloniaProperty.Register<MasonryPanel, double>(nameof(ColumnWidth), defaultValue: 250d);

    /// <summary>
    /// Identifies the <see cref="ColumnSpacing"/> styled property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<MasonryPanel, double>(nameof(ColumnSpacing));

    /// <summary>
    /// Identifies the <see cref="RowSpacing"/> styled property.
    /// </summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<MasonryPanel, double>(nameof(RowSpacing));

    /// <summary>
    /// Initializes static members of the <see cref="MasonryPanel"/> class.
    /// </summary>
    static MasonryPanel()
    {
        AffectsMeasure<MasonryPanel>(ColumnWidthProperty, ColumnSpacingProperty, RowSpacingProperty, HorizontalAlignmentProperty);
    }

    /// <summary>
    /// Gets or sets the desired width for each column.
    /// </summary>
    /// <remarks>
    /// The width of columns can exceed the DesiredColumnWidth if the HorizontalAlignment is set to Stretch.
    /// </remarks>
    public double ColumnWidth
    {
        get => GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between columns of items.
    /// </summary>
    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between rows of items.
    /// </summary>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            return new Size(0, 0);
        }

        var (columnCount, columnWidth, _, desiredWidth) = GetLayout(availableSize.Width, Children.Count);
        var availableHeight = Math.Max(0, availableSize.Height);
        var columnHeights = new double[columnCount];
        var itemsPerColumn = new int[columnCount];

        for (int i = 0; i < Children.Count; i++)
        {
            var columnIndex = GetColumnIndex(columnHeights);

            var child = Children[i];
            child.Measure(new Size(columnWidth, availableHeight));
            var elementSize = child.DesiredSize;
            columnHeights[columnIndex] += elementSize.Height + (itemsPerColumn[columnIndex] > 0 && child.IsVisible ? RowSpacing : 0);
            if (child.IsVisible)
            {
                itemsPerColumn[columnIndex]++;
            }
        }

        double desiredHeight = columnHeights.Max();

        return new Size(desiredWidth, desiredHeight);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0)
        {
            return finalSize;
        }

        var (columnCount, columnWidth, totalWidth, _) = GetLayout(finalSize.Width, Children.Count);
        var remainingWidth = MathUtilities.GreaterThan(finalSize.Width, totalWidth)
            ? finalSize.Width - totalWidth
            : 0;
        var horizontalOffset = HorizontalAlignment switch
        {
            HorizontalAlignment.Right => remainingWidth,
            HorizontalAlignment.Center => remainingWidth / 2,
            _ => 0
        };

        var columnHeights = new double[columnCount];
        var itemsPerColumn = new int[columnCount];

        for (int i = 0; i < Children.Count; i++)
        {
            var columnIndex = GetColumnIndex(columnHeights);

            var child = Children[i];
            var elementSize = child.DesiredSize;

            double elementHeight = elementSize.Height;

            double itemHorizontalOffset = horizontalOffset + (columnWidth * columnIndex) + (ColumnSpacing * columnIndex);
            double itemVerticalOffset = columnHeights[columnIndex] + (itemsPerColumn[columnIndex] > 0 && child.IsVisible ? RowSpacing : 0);

            Rect bounds = new Rect(itemHorizontalOffset, itemVerticalOffset, columnWidth, elementHeight);
            child.Arrange(bounds);

            columnHeights[columnIndex] = itemVerticalOffset + elementSize.Height;
            if (child.IsVisible)
            {
                itemsPerColumn[columnIndex]++;
            }
        }

        return finalSize;
    }

    private (int ColumnCount, double ColumnWidth, double TotalWidth, double DesiredWidth) GetLayout(double availableWidth, int itemCount)
    {
        if (double.IsNaN(availableWidth) || MathUtilities.GreaterThan(0, availableWidth))
        {
            availableWidth = 0;
        }

        var hasFiniteWidth = !double.IsInfinity(availableWidth);
        var desiredColumnWidth = Math.Max(0, ColumnWidth);
        var columnWidth = hasFiniteWidth ? Math.Min(desiredColumnWidth, availableWidth) : desiredColumnWidth;
        var columnCount = GetColumnCount(availableWidth, columnWidth, itemCount);

        if (HorizontalAlignment is HorizontalAlignment.Stretch && hasFiniteWidth)
        {
            var spacingWidth = (columnCount - 1) * ColumnSpacing;
            columnWidth = LayoutAlignmentHelper.GetDistributableSpace(availableWidth, spacingWidth) / columnCount;
        }

        var totalWidth = GetTotalWidth(columnWidth, columnCount);
        var desiredWidth = hasFiniteWidth ? availableWidth : totalWidth;
        return (columnCount, columnWidth, totalWidth, desiredWidth);
    }

    private int GetColumnCount(double availableWidth, double columnWidth, int itemCount)
    {
        if (double.IsInfinity(availableWidth))
        {
            return Math.Max(1, itemCount);
        }

        if (!MathUtilities.GreaterThan(columnWidth, 0))
        {
            return 1;
        }

        var columnCount = Math.Max(1, (int) Math.Floor(availableWidth / columnWidth));
        while (columnCount > 1 && MathUtilities.GreaterThan(GetTotalWidth(columnWidth, columnCount), availableWidth))
        {
            --columnCount;
        }

        return columnCount;
    }

    private double GetTotalWidth(double columnWidth, int columnCount) =>
        columnWidth + ((columnCount - 1) * (columnWidth + ColumnSpacing));

    private static int GetColumnIndex(double[] columnHeights)
    {
        int columnIndex = 0;
        double height = columnHeights[0];
        for (int i = 1; i < columnHeights.Length; i++)
        {
            if (MathUtilities.GreaterThan(height, columnHeights[i]))
            {
                columnIndex = i;
                height = columnHeights[i];
            }
        }

        return columnIndex;
    }
}
