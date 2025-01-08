// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<double>("LineSpacing", "0d", nameof(LayoutPropertyChanged))]
[DependencyProperty<double>("MinItemSpacing", "0d", nameof(LayoutPropertyChanged))]
[DependencyProperty<double>("LineHeight", "50d", nameof(LayoutPropertyChanged))]
public sealed partial class RiverFlowLayout : VirtualizingLayout
{
    private static void LayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RiverFlowLayout jp)
        {
            jp.InvalidateMeasure();
            jp.InvalidateArrange();
        }
    }

    /// <inheritdoc />
    protected override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = new RiverFlowLayoutState(context);
        base.InitializeForContextCore(context);
    }

    /// <inheritdoc />
    protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = null;
        base.UninitializeForContextCore(context);
    }

    /// <inheritdoc />
    protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        var state = (RiverFlowLayoutState)context.LayoutState;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                state.ClearMeasureFromIndex(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Move:
                var minIndex = Math.Min(args.NewStartingIndex, args.OldStartingIndex);
                state.ClearMeasureFromIndex(minIndex);
                state.RecycleElementAt(args.OldStartingIndex);
                state.RecycleElementAt(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                state.ClearMeasureFromIndex(args.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                state.ClearMeasureFromIndex(args.NewStartingIndex);
                state.RecycleElementAt(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                state.Clear();
                break;
        }

        base.OnItemsChangedCore(context, source, args);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size parentMeasure)
    {
        var spacingMeasure = new Size(MinItemSpacing, LineSpacing);

        var state = (RiverFlowLayoutState)context.LayoutState;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (state.AvailableWidth != parentMeasure.Width
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            || spacingMeasure != state.Spacing)
        {
            state.ClearMeasure();
            state.AvailableWidth = parentMeasure.Width;
            state.Spacing = spacingMeasure;
        }
        state.LineHeight = LineHeight;

        var realizationBounds = context.RealizationRect;
        Point? nextPosition = new Point();
        var currentRow = new List<RiverFlowItem>();
        var currentRowLength = .0;
        for (var i = 0; i < context.ItemCount; ++i)
        {
            Point currentPosition;
            var item = state.GetItemAt(i);

            if (nextPosition is { } nextPos)
            {
                item.Position = currentPosition = nextPos;
                nextPosition = null;
            }
            else
                currentPosition = item.Position ?? new Point();

            if (currentPosition.Y + LineHeight < realizationBounds.Top)
            {
                // Item is "above" the bounds
                if (item.Element is not null)
                {
                    context.RecycleElement(item.Element);
                    item.Element = null;
                }

                continue;
            }

            if (currentPosition.Y > realizationBounds.Bottom)
            {
                // Item is "below" the bounds.
                if (item.Element is not null)
                {
                    context.RecycleElement(item.Element);
                    item.Element = null;
                }

                // We don't need to measure anything below the bounds
                break;
            }

            item.Element = context.GetOrCreateElementAt(i);
            item.Element.Measure(new(double.PositiveInfinity, LineHeight));
            if (item.DesiredSize is null)
            {
                item.DesiredSize = item.Element.DesiredSize;
            }
            else if (item.DesiredSize != item.Element!.DesiredSize)
            {
                state.ClearMeasureFromIndex(i + 1);
                item.DesiredSize = item.Element.DesiredSize;
            }

            if (CalcNextPosition(item.DesiredSize.Value) && currentPosition.Y > realizationBounds.Bottom)
            {
                // Item is "below" the bounds.
                if (item.Element is not null)
                {
                    context.RecycleElement(item.Element);
                    item.Element = null;
                }

                // We don't need to measure anything below the bounds
                break;
            }

            continue;

            bool CalcNextPosition(Size desiredSize)
            {
                item.Measure = desiredSize;

                if (desiredSize.Width is 0)
                {
                    nextPosition = currentPosition;
                    return false;
                }

                var excessLength = currentPosition.X + desiredSize.Width - parentMeasure.Width;

                if (excessLength + spacingMeasure.Width > 0)
                {
                    var shrinkScale = (parentMeasure.Width - currentRow.Count * spacingMeasure.Width) / (currentRowLength + desiredSize.Width);
                    var enlargeScale = (parentMeasure.Width - (currentRow.Count - 1) * spacingMeasure.Width) / currentRowLength;

                    // shrinkScale < enlargeScale
                    // find the one that is closer to 1
                    // length excessed
                    if (1 / shrinkScale < enlargeScale)
                    {
                        currentRow.Add(item);
                        // is not used before next assignment
                        // currentRowLength += currentMeasure.Width;
                        Resize(shrinkScale);
                        currentRow.Clear();
                        currentRowLength = 0;
                        // New Row
                        nextPosition = currentPosition = new(0, currentPosition.Y + LineHeight + spacingMeasure.Height);
                    }
                    // length exceeded after adding space
                    else
                    {
                        Resize(enlargeScale);
                        currentRow.Clear();
                        currentRowLength = 0;
                        // New Row
                        item.Position = currentPosition = new(0, currentPosition.Y + LineHeight + spacingMeasure.Height);

                        currentRow.Add(item);
                        currentRowLength += desiredSize.Width;

                        currentPosition.X += desiredSize.Width + spacingMeasure.Width;
                        nextPosition = currentPosition;

                        return true;
                    }

                    void Resize(double scale)
                    {
                        var nextPositionX = .0;
                        var tempPosition = currentPosition;
                        foreach (var justifiedItem in currentRow)
                        {
                            tempPosition.X = nextPositionX;
                            justifiedItem.Position = tempPosition;
                            var tempMeasure = justifiedItem.Measure!.Value;
                            tempMeasure.Width *= scale;
                            justifiedItem.Measure = tempMeasure;
                            justifiedItem.Element?.Measure(tempMeasure);
                            nextPositionX = tempPosition.X + tempMeasure.Width + spacingMeasure.Width;
                        }
                    }
                }
                else
                {
                    currentRow.Add(item);
                    currentRowLength += desiredSize.Width;

                    currentPosition.X += desiredSize.Width + spacingMeasure.Width;
                    nextPosition = currentPosition;
                }

                return false;
            }
        }
        // update value with the last line
        // if the last loop is (parentMeasure.Width > currentMeasure.Width + lineMeasure.Width) the total isn't calculated then calculate it
        // if the last loop is (parentMeasure.Width > currentMeasure.Width) the currentMeasure isn't added to the total so add it here
        // for the last condition it is zeros so adding it will make no difference
        // this way is faster than an if condition in every loop for checking the last item
        // Propagating an infinite size causes a crash. This can happen if the parent is scrollable and infinite in the opposite
        // axis to the panel. Clearing to zero prevents the crash.
        // This is likely an incorrect use of the control by the developer, however we need stability here so setting a default that won't crash.
        var totalMeasure = new Size
        {
            Width = double.IsInfinity(parentMeasure.Width) ? 0 : Math.Ceiling(parentMeasure.Width),
            Height = state.GetHeight()
        };

        return totalMeasure;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size parentMeasure)
    {
        if (context.ItemCount > 0)
        {
            var realizationBounds = context.RealizationRect;
            //  var viewHeight = realizationBounds.Height /= 3;
            //  realizationBounds.Y += viewHeight;

            var state = (RiverFlowLayoutState)context.LayoutState;
            bool ArrangeItem(RiverFlowItem item)
            {
                if (item is { Measure: null } or { Position: null })
                {
                    return false;
                }

                var desiredMeasure = item.Measure.Value;

                var position = item.Position.Value;

                if (position.Y + desiredMeasure.Height >= realizationBounds.Top && position.Y <= realizationBounds.Bottom)
                {
                    // place the item
                    var child = context.GetOrCreateElementAt(item.Index);
                    child.Arrange(new(position, desiredMeasure));
                }
                else if (position.Y > realizationBounds.Bottom)
                {
                    return false;
                }

                return true;
            }

            for (var i = 0; i < context.ItemCount; ++i)
            {
                _ = ArrangeItem(state.GetItemAt(i));
            }
        }

        return parentMeasure;
    }
}
