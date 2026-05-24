// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Models.Home;

public enum HomePageCardKind
{
    RecommendedWorks,
    RecommendedUsers,
    RecommendedNovels,
    RankingWorks,
    Spotlights,
    SingleImage
}

public record HomePageCardLayout
{
    public HomePageCardKind Kind { get; set; }

    public int Column { get; set; }

    public int Row { get; set; }

    public int ColumnSpan { get; set; } = 1;

    public int RowSpan { get; set; } = 1;
}
