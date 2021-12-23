#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/JustifiedLayout.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;

namespace Pixeval.Util.UI;

/// <summary>
///     Flickr's Justified Layout in C#
///     https://github.com/flickr/justified-layout
/// </summary>
public class JustifiedLayout
{
    public double ContainerHeight;

    public List<LayoutItem> LayoutItems = new();

    public List<JustifiedLayoutRow> Rows = new();

    public JustifiedLayout(JustifiedLayoutConfig? config = null)
    {
        Config = config ?? new JustifiedLayoutConfig();
        ContainerHeight = Config.ContainerPadding.Top;
    }

    public JustifiedLayoutConfig Config { get; set; }

    public JustifiedLayoutRow CreateNewRow()
    {
        var isBreakoutRow = false;
        if (Config.FullWidthBreakoutRowCadence != null)
        {
            if ((Rows.Count + 1) % Config.FullWidthBreakoutRowCadence! == 0)
            {
                isBreakoutRow = true;
            }
        }

        return new JustifiedLayoutRow
        {
            Top = ContainerHeight,
            Left = Config.ContainerPadding.Left,
            Width = Config.ContainerWidth - Config.ContainerPadding.Left - Config.ContainerPadding.Right,
            Spacing = Config.BoxSpacing.Horizontal,
            TargetRowHeight = Config.TargetRowHeight,
            TargetRowHeightTolerance = Config.TargetRowHeightTolerance,
            EdgeCaseMinRowHeight = 0.5 * Config.TargetRowHeight,
            EdgeCaseMaxRowHeight = 2 * Config.TargetRowHeight,
            IsBreakoutRow = isBreakoutRow,
            LayoutStyle = Config.LayoutStyle
        }.Init();
    }

    public IList<LayoutItem> AddRow(JustifiedLayoutRow row)
    {
        Rows.Add(row);
        LayoutItems.AddRange(row.Items);
        ContainerHeight += row.Height + Config.BoxSpacing.Vertical;

        return row.Items;
    }

    public ComputedLayout ComputeLayout(double[] boxes)
    {
        return ComputeLayout(boxes.Select(x => new LayoutItem(x)).ToList());
    }

    public ComputedLayout ComputeLayout(IList<LayoutItem> itemLayoutData)
    {
        var laidOutItems = new List<LayoutItem>();
        if (Config.IsForceAspectRatio)
        {
            foreach (var itemData in itemLayoutData)
            {
                itemData.ForceAspectRatio = Config.ForceAspectRatio;
            }
        }

        JustifiedLayoutRow? currentRow = null;

        bool itemAdded;
        _ = itemLayoutData.Any(itemData =>
        {
            //if (isNaN(itemData.aspectRatio))
            //{
            //    throw new Error("Item " + i + " has an invalid aspect ratio");
            //}

            // If not currently building up a row, make a new one.
            currentRow ??= CreateNewRow();

            // Attempt to add item to the current row.
            itemAdded = currentRow.AddItem(itemData);

            if (!currentRow.IsLayoutComplete())
            {
                return false;
            }

            // Row is filled; add it and start a new one
            laidOutItems.AddRange(AddRow(currentRow));

            if (Rows.Count >= Config.MaxNumRows)
            {
                return true;
            }

            currentRow = CreateNewRow();

            // Item was rejected; add it to its own row
            if (itemAdded)
            {
                return false;
            }

            itemAdded = currentRow.AddItem(itemData);

            if (!currentRow.IsLayoutComplete())
            {
                return false;
            }

            // If the rejected item fills a row on its own, add the row and start another new one
            laidOutItems.AddRange(AddRow(currentRow));
            if (Rows.Count >= Config.MaxNumRows)
            {
                return true;
            }

            currentRow = CreateNewRow();

            return false;
        });

        // Handle any leftover content (orphans) depending on where they lie
        // in this layout update, and in the total content set.
        if (currentRow != null && currentRow.Items.Count > 0 && Config.ShowWidows)
        {
            // Last page of all content or orphan suppression is suppressed; lay out orphans.
            if (Rows.Count > 0)
            {
                // Only Match previous row's height if it exists and it isn't a breakout row
                var nextToLastRowHeight = Rows[^1].IsBreakoutRow ? Rows[^1].TargetRowHeight : Rows[^1].Height;

                currentRow.ForceComplete(nextToLastRowHeight);
            }
            else
            {
                // ...else use target height if there is no other row height to reference.
                currentRow.ForceComplete();
            }

            laidOutItems.AddRange(AddRow(currentRow));
            Config.WidowCount = currentRow.Items.Count;
        }

        // We need to clean up the bottom container padding
        // First remove the height added for box spacing
        ContainerHeight -= Config.BoxSpacing.Vertical;
        // Then add our bottom container padding
        ContainerHeight += Config.ContainerPadding.Bottom;

        return new ComputedLayout
        {
            ContainerHeight = ContainerHeight,
            WidowCount = Config.WidowCount,
            Boxes = LayoutItems.ToList()
        };
    }
}

public record ComputedLayout
{
    public double ContainerHeight;
    public int WidowCount;
    public List<LayoutItem> Boxes { get; set; } = new();
}

public record Spacing
{
    public Spacing(int horizontal, int vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public Spacing(int value)
    {
        Horizontal = value;
        Vertical = value;
    }

    public int Horizontal { get; set; }
    public int Vertical { get; set; }
}

public record JustifiedLayoutConfig
{
    public int ContainerWidth { get; set; } = 1060;
    public Thickness ContainerPadding { get; set; } = new(10);
    public Spacing BoxSpacing { get; set; } = new(10);
    public int TargetRowHeight { get; set; } = 320;
    public double TargetRowHeightTolerance { get; set; } = 0.25;
    public int MaxNumRows { get; set; } = int.MaxValue;
    public bool IsForceAspectRatio { get; set; } = false;
    public double? ForceAspectRatio { get; set; }
    public bool ShowWidows { get; set; } = true;
    public int? FullWidthBreakoutRowCadence { get; set; } = null;
    public WidowLayoutStyle LayoutStyle { get; set; } = WidowLayoutStyle.Left;
    public int WidowCount { get; set; }
}

public class JustifiedLayoutRow
{
    public IList<LayoutItem> Items;

    public JustifiedLayoutRow()
    {
        Height = 0;
        Items = new List<LayoutItem>();
    }

    public double Top { get; set; }
    public double Left { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    public int Spacing { get; set; }
    public double TargetRowHeight { get; set; }
    public double TargetRowHeightTolerance { get; set; }

    public double MinAspectRatio { get; set; }
    public double MaxAspectRatio { get; set; }

    public double EdgeCaseMinRowHeight { get; set; }
    public double EdgeCaseMaxRowHeight { get; set; }

    public WidowLayoutStyle LayoutStyle { get; set; }
    public bool IsBreakoutRow { get; set; }

    public JustifiedLayoutRow Init()
    {
        MinAspectRatio = Width / TargetRowHeight * (1 - TargetRowHeightTolerance);
        MaxAspectRatio = Width / TargetRowHeight * (1 + TargetRowHeightTolerance);
        return this;
    }

    public bool IsLayoutComplete()
    {
        return Height > 0;
    }


    public bool AddItem(LayoutItem itemData)
    {
        var newItems = Items.Prepend(itemData).ToList();
        // Calculate aspect ratios for items only; exclude spacing
        var rowWidthWithoutSpacing = Width - (newItems.Count - 1) * Spacing;
        var newAspectRatio = newItems.Aggregate(0.0, (sum, item) => item.AspectRatio + sum);

        var targetAspectRatio = rowWidthWithoutSpacing / TargetRowHeight;

        // Handle big full-width breakout photos if we're doing them
        if (IsBreakoutRow)
        {
            // Only do it if there's no other items in this row
            if (Items.Count == 0)
            {
                // Only go full width if this photo is a square or landscape
                if (itemData.AspectRatio >= 1)
                {
                    // Close out the row with a full width photo
                    Items.Add(itemData);
                    CompleteLayout(rowWidthWithoutSpacing / itemData.AspectRatio, WidowLayoutStyle.Justify);
                    return true;
                }
            }
        }

        if (newAspectRatio < MinAspectRatio)
        {
            // New aspect ratio is too narrow / scaled row height is too tall.
            // Accept this item and leave row open for more items.
            Items.Add(itemData with { });
            return true;
        }

        if (newAspectRatio > MaxAspectRatio)
        {
            // New aspect ratio is too wide / scaled row height will be too short.
            // Accept item if the resulting aspect ratio is closer to target than it would be without the item.
            // NOTE: Any row that falls into this block will require cropping/padding on individual items.

            if (Items.Count == 0)
            {
                // When there are no existing items, force acceptance of the new item and complete the layout.
                // This is the pano special case.
                Items.Add(itemData with { });
                CompleteLayout(rowWidthWithoutSpacing / newAspectRatio, WidowLayoutStyle.Justify);
                return true;
            }

            // Calculate width/aspect ratio for row before adding new item
            var previousRowWidthWithoutSpacing = Width - (Items.Count - 1) * Spacing;
            var previousAspectRatio = newItems.Aggregate(0.0, (sum, item) => item.AspectRatio + sum);
            var previousTargetAspectRatio = previousRowWidthWithoutSpacing / TargetRowHeight;

            if (Math.Abs(newAspectRatio - targetAspectRatio) > Math.Abs(previousAspectRatio - previousTargetAspectRatio))
            {
                // Row with new item is us farther away from target than row without; complete layout and reject item.
                CompleteLayout(previousRowWidthWithoutSpacing / previousAspectRatio, WidowLayoutStyle.Justify);
                return false;
            }

            // Row with new item is us closer to target than row without;
            // accept the new item and complete the row layout.
            Items.Add(itemData with { });
            CompleteLayout(rowWidthWithoutSpacing / newAspectRatio, WidowLayoutStyle.Justify);
            return true;
        }

        // New aspect ratio / scaled row height is within tolerance;
        // accept the new item and complete the row layout.
        Items.Add(itemData with { });
        CompleteLayout(rowWidthWithoutSpacing / newAspectRatio, WidowLayoutStyle.Justify);
        return true;
    }

    /**
	 * Set row height and compute item geometry from that height.
	 * Will justify items within the row unless instructed not to.
	 *
	 * @method completeLayout
	 * @param newHeight {Number} Set row height to this value.
	 * @param widowLayoutStyle {String} How should widows display? Supported: left | justify | center
	 */
    public void CompleteLayout(double newHeight, WidowLayoutStyle? widowLayoutStyle)
    {
        var itemWidthSum = Left;
        var rowWidthWithoutSpacing = Width - (Items.Count - 1) * Spacing;
        double clampedToNativeRatio;

        // Justify unless explicitly specified otherwise.
        widowLayoutStyle ??= WidowLayoutStyle.Left;

        // Clamp row height to edge case minimum/maximum.
        var clampedHeight = Math.Max(EdgeCaseMinRowHeight, Math.Min(newHeight, EdgeCaseMaxRowHeight));

        if (newHeight != clampedHeight)
        {
            // If row height was clamped, the resulting row/item aspect ratio will be off,
            // so force it to fit the width (recalculate aspectRatio to match clamped height).
            // NOTE: this will result in cropping/padding commensurate to the amount of clamping.
            Height = clampedHeight;
            clampedToNativeRatio = rowWidthWithoutSpacing / clampedHeight / (rowWidthWithoutSpacing / newHeight);
        }
        else
        {
            // If not clamped, leave ratio at 1.0.
            Height = newHeight;
            clampedToNativeRatio = 1.0;
        }

        // Compute item geometry based on newHeight.
        foreach (var item in Items)
        {
            item.Top = Top;
            item.Width = item.AspectRatio * Height * clampedToNativeRatio;
            item.Height = Height;

            // Left-to-right.
            // TODO right to left
            // item.left = this.width - itemWidthSum - item.width;
            item.Left = itemWidthSum;

            // Increment width.
            itemWidthSum += item.Width + Spacing;
        }

        switch (widowLayoutStyle)
        {
            // If specified, ensure items fill row and distribute error
            // caused by rounding width and height across all items.
            case WidowLayoutStyle.Justify:
            {
                itemWidthSum -= Spacing + Left;

                var errorWidthPerItem = (itemWidthSum - Width) / Items.Count;
                var roundedCumulativeErrors = Items.Select((_, i) => Math.Round((i + 1) * errorWidthPerItem)).ToList();


                if (Items.Count == 1)
                {
                    // For rows with only one item, adjust item width to fill row.
                    var singleItemGeometry = Items[0];
                    singleItemGeometry.Width -= Math.Round(errorWidthPerItem);
                }
                else
                {
                    // For rows with multiple items, adjust item width and shift items to fill the row,
                    // while maintaining equal spacing between items in the row.
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i];
                        if (i > 0)
                        {
                            item.Left -= roundedCumulativeErrors[i - 1];
                            item.Width -= roundedCumulativeErrors[i] - roundedCumulativeErrors[i - 1];
                        }
                        else
                        {
                            item.Width -= roundedCumulativeErrors[i];
                        }
                    }
                }

                break;
            }
            case WidowLayoutStyle.Center:
            {
                // Center widows
                var centerOffset = (Width - itemWidthSum) / 2;
                foreach (var item in Items)
                {
                    item.Left += centerOffset + Spacing;
                }

                break;
            }
        }
    }

    public void ForceComplete(double? rowHeight = null)
    {
        // TODO Handle fitting to width
        // var rowWidthWithoutSpacing = this.width - (this.items.length - 1) * this.spacing,
        // 	currentAspectRatio = this.items.reduce(function (sum, item) {
        // 		return sum + item.aspectRatio;
        // 	}, 0);

        CompleteLayout(rowHeight ?? TargetRowHeight, LayoutStyle);
    }
}

public record LayoutItem
{
    public LayoutItem(double ratio)
    {
        AspectRatio = ratio;
    }

    public LayoutItem(double width, double height)
    {
        AspectRatio = width / height;
    }

    public double AspectRatio { get; set; }
    public double? ForceAspectRatio { get; set; }
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public Thickness Margin => new(Left, Top, 0, 0);
}

public enum WidowLayoutStyle
{
    Left,
    Justify,
    Center
}