// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Models.Home;

public static class HomeCardLayoutEngine
{
    public static bool CanPlace(
        IReadOnlyCollection<HomePageCardLayout> cards,
        HomePageCardLayout? movingCard,
        HomeCardBounds bounds,
        int rowCount,
        int columnCount)
    {
        if (!IsWithinGrid(bounds, rowCount, columnCount))
            return false;

        return cards.All(card => ReferenceEquals(card, movingCard) || !Overlaps(HomeCardBounds.From(card), bounds));
    }

    public static bool TryFindFreePosition(
        IReadOnlyCollection<HomePageCardLayout> cards,
        int columnSpan,
        int rowSpan,
        int rowCount,
        int columnCount,
        out int column,
        out int row)
    {
        for (row = 0; row <= rowCount - rowSpan; row++)
            for (column = 0; column <= columnCount - columnSpan; column++)
                if (CanPlace(cards, null, new(column, row, columnSpan, rowSpan), rowCount, columnCount))
                    return true;

        column = row = 0;
        return false;
    }

    public static HomeCardBounds Clamp(HomeCardBounds bounds, int rowCount, int columnCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columnCount, 1);

        var columnSpan = int.Clamp(bounds.ColumnSpan, 1, columnCount);
        var rowSpan = int.Clamp(bounds.RowSpan, 1, rowCount);
        return new(
            int.Clamp(bounds.Column, 0, columnCount - columnSpan),
            int.Clamp(bounds.Row, 0, rowCount - rowSpan),
            columnSpan,
            rowSpan);
    }

    public static bool CanResizeGrid(IReadOnlyCollection<HomePageCardLayout> cards, int rowCount, int columnCount) =>
        cards.All(card => IsWithinGrid(HomeCardBounds.From(card), rowCount, columnCount));

    public static bool IsWithinGrid(HomeCardBounds bounds, int rowCount, int columnCount) =>
        bounds is { Column: >= 0, Row: >= 0, ColumnSpan: >= 1, RowSpan: >= 1 }
        && bounds.Column + bounds.ColumnSpan <= columnCount
        && bounds.Row + bounds.RowSpan <= rowCount;

    public static bool Overlaps(HomeCardBounds first, HomeCardBounds second) =>
        first.Column < second.Column + second.ColumnSpan
        && first.Column + first.ColumnSpan > second.Column
        && first.Row < second.Row + second.RowSpan
        && first.Row + first.RowSpan > second.Row;
}
