// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Pixeval.Models.Options;

namespace Pixeval.Controls;

/// <summary>
/// A <see cref="StackPanel"/> that supports reverse browsing directions.
/// </summary>
public class ReversibleStackPanel : StackPanel, INavigableContainer
{
    public static readonly StyledProperty<BrowseDirection> BrowseDirectionProperty =
        AvaloniaProperty.Register<ReversibleStackPanel, BrowseDirection>(nameof(BrowseDirection));

    static ReversibleStackPanel()
    {
        AffectsArrange<ReversibleStackPanel>(BrowseDirectionProperty);
    }

    public ReversibleStackPanel()
    {
        SyncOrientation();
    }

    public BrowseDirection BrowseDirection
    {
        get => GetValue(BrowseDirectionProperty);
        set => SetValue(BrowseDirectionProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BrowseDirectionProperty)
            SyncOrientation();
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (!IsReversed)
            return base.ArrangeOverride(finalSize);

        var children = Children;
        var spacing = Spacing;
        var position = IsHorizontal ? finalSize.Width : finalSize.Height;

        foreach (var child in children)
        {
            if (!child.IsVisible)
                continue;

            var childLength = IsHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
            position -= childLength;

            var rect = IsHorizontal
                ? new Rect(position, 0, childLength, double.Max(finalSize.Height, child.DesiredSize.Height))
                : new Rect(0, position, double.Max(finalSize.Width, child.DesiredSize.Width), childLength);

            child.Arrange(rect);
            position -= spacing;
        }

        RaiseEvent(new RoutedEventArgs(IsHorizontal ? HorizontalSnapPointsChangedEvent : VerticalSnapPointsChangedEvent));
        return finalSize;
    }

    /// <inheritdoc />
    protected override IInputElement? GetControlInDirection(NavigationDirection direction, Control? from)
    {
        var index = from is not null ? Children.IndexOf(from) : -1;
        index = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => Children.Count - 1,
            NavigationDirection.Next => index + 1,
            NavigationDirection.Previous => index is -1 ? -1 : index - 1,
            NavigationDirection.Left when IsHorizontal && index is not -1 =>
                index + (BrowseDirection is BrowseDirection.RightLeft ? 1 : -1),
            NavigationDirection.Right when IsHorizontal && index is not -1 =>
                index + (BrowseDirection is BrowseDirection.RightLeft ? -1 : 1),
            NavigationDirection.Up when !IsHorizontal && index is not -1 =>
                index + (BrowseDirection is BrowseDirection.BottomUp ? 1 : -1),
            NavigationDirection.Down when !IsHorizontal && index is not -1 =>
                index + (BrowseDirection is BrowseDirection.BottomUp ? -1 : 1),
            _ => -1
        };

        return index >= 0 && index < Children.Count ? Children[index] : null;
    }

    IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var result = GetControlInDirection(direction, from as Control);
        if (result is not null || !wrap)
            return result;

        return direction switch
        {
            _ when MovesTowardsStart(direction) => GetControlInDirection(NavigationDirection.Last, null),
            _ when MovesTowardsEnd(direction) => GetControlInDirection(NavigationDirection.First, null),
            _ => null
        };
    }

    private bool IsHorizontal => BrowseDirection is BrowseDirection.LeftRight or BrowseDirection.RightLeft;

    private bool IsReversed => BrowseDirection is BrowseDirection.RightLeft or BrowseDirection.BottomUp;

    private void SyncOrientation()
    {
        var orientation = IsHorizontal ? Orientation.Horizontal : Orientation.Vertical;
        if (Orientation != orientation)
            Orientation = orientation;
    }

    private bool MovesTowardsStart(NavigationDirection direction) =>
        direction is NavigationDirection.Previous or NavigationDirection.PageUp
        || BrowseDirection switch
        {
            BrowseDirection.LeftRight => direction is NavigationDirection.Left,
            BrowseDirection.RightLeft => direction is NavigationDirection.Right,
            BrowseDirection.TopDown => direction is NavigationDirection.Up,
            BrowseDirection.BottomUp => direction is NavigationDirection.Down,
            _ => false
        };

    private bool MovesTowardsEnd(NavigationDirection direction) =>
        direction is NavigationDirection.Next or NavigationDirection.PageDown
        || BrowseDirection switch
        {
            BrowseDirection.LeftRight => direction is NavigationDirection.Right,
            BrowseDirection.RightLeft => direction is NavigationDirection.Left,
            BrowseDirection.TopDown => direction is NavigationDirection.Down,
            BrowseDirection.BottomUp => direction is NavigationDirection.Up,
            _ => false
        };
}
