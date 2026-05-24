// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public readonly record struct HomeCardBounds(int Column, int Row, int ColumnSpan, int RowSpan);

public enum HomeCardEditAction
{
    Move,
    ResizeLeft,
    ResizeTop,
    ResizeRight,
    ResizeBottom,
    ResizeTopLeft,
    ResizeTopRight,
    ResizeBottomRight,
    ResizeBottomLeft
}

public sealed class HomeCardSelectedEventArgs(HomePageCardLayout card, bool refreshSelection) : EventArgs
{
    public HomePageCardLayout Card { get; } = card;

    public bool RefreshSelection { get; } = refreshSelection;
}

public sealed class HomeCardEditPreviewEventArgs(HomePageCardLayout card, HomeCardBounds bounds) : EventArgs
{
    public HomePageCardLayout Card { get; } = card;

    public HomeCardBounds Bounds { get; } = bounds;

    public bool Accepted { get; set; }
}

public sealed class HomeCardEditCompletedEventArgs(HomePageCardLayout card, bool hasChanged) : EventArgs
{
    public HomePageCardLayout Card { get; } = card;

    public bool HasChanged { get; } = hasChanged;
}

public static class HomeCardBoundsFactory
{
    public static HomeCardBounds From(HomePageCardLayout card) => new(card.Column, card.Row, card.ColumnSpan, card.RowSpan);
}
