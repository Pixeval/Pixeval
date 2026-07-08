// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalWorkListDto(
    int Count,
    IReadOnlyList<PixevalWorkDto> Works,
    PixevalWorkFilterAnalysisDto? Filter = null,
    bool HasMore = false,
    string? NextCursor = null);
