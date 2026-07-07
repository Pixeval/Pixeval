// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalHistoryArtworkDto(
    string Id,
    string SerializeKey,
    string Title,
    IReadOnlyList<PixevalTagDto> Tags,
    string? ThumbnailUrl,
    DateTimeOffset CreateDate,
    string SafeRating,
    string ImageType,
    bool IsAiGenerated,
    int TotalBookmarks,
    int? Width,
    int? Height,
    double? AspectRatio,
    PixevalWorkDto? PixivWork);