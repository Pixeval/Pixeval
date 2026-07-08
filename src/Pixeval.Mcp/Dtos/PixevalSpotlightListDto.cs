// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSpotlightListDto(
    int Count,
    IReadOnlyList<PixevalSpotlightDto> Spotlights,
    bool HasMore = false,
    string? NextCursor = null);
