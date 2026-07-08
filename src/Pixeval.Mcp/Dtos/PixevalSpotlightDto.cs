// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSpotlightDto(
    long Id,
    string Title,
    string PureTitle,
    string ThumbnailUrl,
    string ArticleUrl,
    DateTimeOffset PublishDate,
    string Category,
    string SubcategoryLabel,
    string WebsiteUrl,
    string PixevalUri)
{
    public static PixevalSpotlightDto FromSpotlight(Spotlight spotlight) =>
        new(
            spotlight.Id,
            spotlight.Title,
            spotlight.PureTitle,
            spotlight.Thumbnail,
            spotlight.ArticleUrl,
            spotlight.PublishDate,
            spotlight.Category.ToString(),
            spotlight.SubcategoryLabel,
            spotlight.WebsiteUri.ToString(),
            spotlight.AppUri.ToString());
}
