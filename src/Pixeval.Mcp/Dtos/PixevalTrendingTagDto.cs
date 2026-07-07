// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalTrendingTagDto(
    string Tag,
    string TranslatedName,
    PixevalWorkDto SampleWork)
{
    public static PixevalTrendingTagDto FromTrendingTag(TrendingTag tag) =>
        new(tag.Tag, tag.TranslatedName, PixevalWorkDto.FromIllustration(tag.Illustration));
}