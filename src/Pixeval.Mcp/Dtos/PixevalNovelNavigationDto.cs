// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalNovelNavigationDto(
    long Id,
    bool Viewable,
    string ContentOrder,
    string Title,
    string CoverUrl,
    string? ViewableMessage);