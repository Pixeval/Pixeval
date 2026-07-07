// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalTagDto(
    string Name,
    string? TranslatedName)
{
    public static PixevalTagDto FromTag(Tag tag) => new(tag.Name, tag.TranslatedName);
}