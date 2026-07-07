// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalBookmarkTagDto(
    string? Name,
    int Count,
    bool IsAll)
{
    public static PixevalBookmarkTagDto FromBookmarkTag(BookmarkTag tag, bool isAll = false) =>
        new(tag.Name, tag.Count, isAll);
}