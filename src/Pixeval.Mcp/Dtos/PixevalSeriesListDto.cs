// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSeriesListDto(
    int Count,
    IReadOnlyList<PixevalSeriesDto> Series,
    bool HasMore = false,
    string? NextCursor = null);
