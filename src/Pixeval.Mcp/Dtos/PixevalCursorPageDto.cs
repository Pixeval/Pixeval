// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Global.Enum;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalCursorPageDto(
    string Kind,
    int Count,
    bool HasMore,
    string? NextCursor,
    IReadOnlyList<PixevalWorkDto>? Works = null,
    IReadOnlyList<PixevalUserDto>? Users = null,
    IReadOnlyList<PixevalSeriesDto>? Series = null,
    IReadOnlyList<PixevalSpotlightDto>? Spotlights = null,
    IReadOnlyList<PixevalCommentDto>? Comments = null,
    IReadOnlyList<PixevalBookmarkTagDto>? BookmarkTags = null,
    PixevalWorkFilterAnalysisDto? Filter = null,
    long? UserId = null,
    SimpleWorkType? WorkType = null,
    PrivacyPolicy? Privacy = null);
