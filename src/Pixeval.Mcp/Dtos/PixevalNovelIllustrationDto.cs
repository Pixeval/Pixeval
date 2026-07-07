// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalNovelIllustrationDto(
    long Id,
    int Page,
    bool Visible,
    string? AvailableMessage,
    string Title,
    string Description,
    string ThumbnailUrl,
    string? OriginalUrl,
    string WebsiteUrl,
    string PixevalUri,
    PixevalNovelUserDto User,
    IReadOnlyList<string> Tags);