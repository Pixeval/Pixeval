// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Media;
using FluentIcons.Common;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed record HomeCardTemplate(
    HomePageCardKind Kind,
    string Title,
    string Description,
    int DefaultColumnSpan,
    int DefaultRowSpan,
    Color Color)
{
    public IBrush Brush { get; } = new SolidColorBrush(Color);

    public string SizeText => $"{DefaultColumnSpan} x {DefaultRowSpan}";

    public Symbol Symbol => Kind switch
    {
        HomePageCardKind.RecommendedWorks => Symbol.ImageMultiple,
        HomePageCardKind.RecommendedUsers => Symbol.PeopleCommunity,
        HomePageCardKind.RecommendedNovels => Symbol.BookOpen,
        HomePageCardKind.RankingWorks => Symbol.ArrowTrendingLines,
        HomePageCardKind.Spotlights => Symbol.SlideTextSparkle,
        HomePageCardKind.SingleImage => Symbol.Image,
        _ => Symbol.RectangleLandscape
    };
}
