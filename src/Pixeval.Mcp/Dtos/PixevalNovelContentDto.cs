// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalNovelContentDto(
    PixevalWorkDto Novel,
    long Id,
    string Title,
    long? SeriesId,
    string? SeriesTitle,
    bool? SeriesIsWatched,
    long UserId,
    string CoverUrl,
    IReadOnlyList<string> Tags,
    string Caption,
    DateTimeOffset Date,
    string AiType,
    bool IsOriginal,
    string Language,
    int TextLength,
    string Text,
    string? Markdown,
    PixevalNovelRatingDto Rating,
    PixevalNovelMarkerDto? Marker,
    PixevalNovelNavigationPairDto? SeriesNavigation,
    IReadOnlyList<PixevalNovelImageDto> Images,
    IReadOnlyList<PixevalNovelIllustrationDto> Illustrations,
    IReadOnlyList<PixevalNovelGlossaryItemDto> GlossaryItems);