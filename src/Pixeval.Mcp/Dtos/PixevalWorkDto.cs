// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using Misaki;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalWorkDto(
    long Id,
    string Type,
    string Title,
    string Description,
    PixevalUserDto Author,
    IReadOnlyList<PixevalTagDto> Tags,
    string ThumbnailUrl,
    string ThumbnailResourceUri,
    string WebsiteUrl,
    string PixevalUri,
    string XRestrict,
    bool IsBookmarked,
    bool IsAiGenerated,
    int TotalBookmarks,
    int TotalViews,
    int PageCount,
    int? Width,
    int? Height,
    DateTimeOffset CreateDate,
    string SafeRating,
    string ImageType,
    double? AspectRatio,
    bool IsAnimated)
{
    public static PixevalWorkDto FromWork(WorkBase work) =>
        work switch
        {
            Illustration illustration => FromIllustration(illustration),
            Novel novel => FromNovel(novel),
            _ => throw new ArgumentOutOfRangeException(nameof(work), work.GetType().FullName)
        };

    public static PixevalWorkDto FromIllustration(Illustration illustration) =>
        new(
            illustration.Id,
            illustration.Type.ToString(),
            illustration.Title,
            illustration.Description,
            PixevalUserDto.FromUserInfo(illustration.User),
            [.. illustration.Tags.Select(PixevalTagDto.FromTag)],
            illustration.ThumbnailUrls.Medium,
            PixevalThumbnailInfoDto.GetThumbnailResourceUri("illust", illustration.Id, "medium"),
            illustration.WebsiteUri.ToString(),
            illustration.AppUri.ToString(),
            illustration.XRestrict.ToString(),
            illustration.IsFavorite,
            illustration.IsAiGenerated,
            illustration.TotalFavorite,
            illustration.TotalView,
            illustration.PageCount,
            illustration.Width,
            illustration.Height,
            illustration.CreateDate,
            GetSafeRating(illustration),
            illustration.ImageType.ToString(),
            GetAspectRatio(illustration.Width, illustration.Height),
            illustration.ImageType is Misaki.ImageType.SingleAnimatedImage);

    public static PixevalWorkDto FromNovel(Novel novel) =>
        new(
            novel.Id,
            "Novel",
            novel.Title,
            novel.Description,
            PixevalUserDto.FromUserInfo(novel.User),
            [.. novel.Tags.Select(PixevalTagDto.FromTag)],
            novel.ThumbnailUrls.Medium,
            PixevalThumbnailInfoDto.GetThumbnailResourceUri("novel", novel.Id, "medium"),
            novel.WebsiteUri.ToString(),
            novel.AppUri.ToString(),
            novel.XRestrict.ToString(),
            novel.IsFavorite,
            novel.IsAiGenerated,
            novel.TotalFavorite,
            novel.TotalView,
            novel.PageCount,
            null,
            null,
            novel.CreateDate,
            GetSafeRating(novel),
            novel.ImageType.ToString(),
            null,
            false);

    private static string GetSafeRating(IArtworkInfo artwork) => artwork.SafeRating.ToString();

    private static double? GetAspectRatio(int width, int height) =>
        width > 0 && height > 0 ? (double) width / height : null;
}