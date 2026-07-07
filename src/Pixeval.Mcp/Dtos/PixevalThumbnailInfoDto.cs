// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalThumbnailInfoDto(
    string WorkType,
    long WorkId,
    IReadOnlyList<PixevalThumbnailImageDto> Thumbnails)
{
    private static readonly IReadOnlyList<string> _Sizes = ["square_medium", "medium", "large", "not_cropped"];

    public static PixevalThumbnailInfoDto FromWork(WorkBase work)
    {
        var workType = work is Novel ? "novel" : "illust";
        var thumbnails = _Sizes
            .Select(size =>
            {
                var url = GetThumbnailUrl(work, size);
                return new PixevalThumbnailImageDto(
                    size,
                    url,
                    GetThumbnailResourceUri(workType, work.Id, size),
                    PixevalMcpHelpers.GetImageMimeType(url));
            })
            .ToArray();
        return new(workType, work.Id, thumbnails);
    }

    public static string GetThumbnailUrl(WorkBase work, string size) =>
        NormalizeThumbnailSize(size) switch
        {
            "square_medium" => work.ThumbnailUrls.SquareMedium,
            "medium" => work.ThumbnailUrls.Medium,
            "large" => work.ThumbnailUrls.Large,
            "not_cropped" => work.ThumbnailUrls.NotCropped,
            _ => throw new PixevalMcpException(
                "size must be 'square_medium', 'medium', 'large', or 'not_cropped'.")
        };

    public static string GetThumbnailResourceUri(string workType, long workId, string size) =>
        $"pixeval://{workType}/{workId}/thumbnail/{NormalizeThumbnailSize(size)}";

    private static string NormalizeThumbnailSize(string size) =>
        size.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant() switch
        {
            "square" or "squaremedium" => "square_medium",
            "notcropped" or "original_preview" or "originalpreview" => "not_cropped",
            var value => value
        };
}