// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalCommentListDto(
    int Count,
    IReadOnlyList<PixevalCommentDto> Comments,
    bool HasMore = false,
    string? NextCursor = null);
