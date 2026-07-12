// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Global.Enum;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalBookmarkTagListDto(
    long UserId,
    SimpleWorkType WorkType,
    PrivacyPolicy Privacy,
    int Count,
    IReadOnlyList<PixevalBookmarkTagDto> Tags,
    bool HasMore = false,
    string? NextCursor = null);
