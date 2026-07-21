// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalHistoryListDto(
    PixevalHistoryType Type,
    int Skip,
    int RequestedCount,
    int Total,
    int Count,
    IReadOnlyList<PixevalHistoryItemDto> Items,
    PixevalWorkFilterAnalysisDto? Filter = null);
