// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalTrendingTagListDto(
    int Count,
    IReadOnlyList<PixevalTrendingTagDto> Tags);