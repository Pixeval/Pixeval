// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSeriesDto(
    long Id,
    string Title,
    PixevalUserDto Author,
    string? MaskText,
    string Url,
    int PublishedContentCount,
    long LatestContentId,
    DateTimeOffset LastPublishedContentDatetime)
{
    public static PixevalSeriesDto FromSeries(Series series) =>
        new(
            series.Id,
            series.Title,
            PixevalUserDto.FromUserInfo(series.User),
            series.MaskText,
            series.Url,
            series.PublishedContentCount,
            series.LatestContentId,
            series.LastPublishedContentDatetime);
}
