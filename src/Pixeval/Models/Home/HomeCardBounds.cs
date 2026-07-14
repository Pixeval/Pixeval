// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Models.Home;

public readonly record struct HomeCardBounds(int Column, int Row, int ColumnSpan, int RowSpan)
{
    public static HomeCardBounds From(HomePageCardLayout card) => new(card.Column, card.Row, card.ColumnSpan, card.RowSpan);

    public void ApplyTo(HomePageCardLayout card)
    {
        card.Column = Column;
        card.Row = Row;
        card.ColumnSpan = ColumnSpan;
        card.RowSpan = RowSpan;
    }
}
