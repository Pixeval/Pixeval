// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Mcp.Dtos;
using Pixeval.ViewModels;

namespace Pixeval.Utilities.McpServer;

public sealed partial class PixevalMcpService
{
    public async Task<PixevalNovelContentDto> NovelContentAsync(
        long id,
        bool includeMarkdown,
        CancellationToken cancellationToken)
    {
        var novelTask = MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken);
        var contentTask = MakoClient.GetNovelContentAsync(id).WaitAsync(cancellationToken);
        await Task.WhenAll(novelTask, contentTask).ConfigureAwait(false);

        var novel = await novelTask.ConfigureAwait(false);
        var content = await contentTask.ConfigureAwait(false);
        return ToNovelContentDto(novel, content, includeMarkdown);
    }

    private static PixevalNovelContentDto ToNovelContentDto(
        Novel novel,
        NovelContent content,
        bool includeMarkdown) =>
        new(
            PixevalWorkDto.FromNovel(novel),
            content.Id,
            content.Title,
            content.SeriesId,
            content.SeriesTitle,
            content.SeriesIsWatched,
            content.UserId,
            content.CoverUrl,
            content.Tags,
            content.Caption,
            content.Date,
            content.AiType.ToString(),
            content.IsOriginal,
            content.Language,
            content.Text.Length,
            content.Text,
            includeMarkdown ? BuildNovelMarkdown(content) : null,
            new(content.Rating.Like, content.Rating.Bookmark, content.Rating.View),
            content.Marker is { } marker ? new PixevalNovelMarkerDto(marker.Page) : null,
            content.SeriesNavigation is { } navigation
                ? new PixevalNovelNavigationPairDto(
                    ToNovelNavigationDto(navigation.PrevNovel),
                    ToNovelNavigationDto(navigation.NextNovel))
                : null,
            [.. content.Images.Select(ToNovelImageDto)],
            [.. content.Illustrations.Select(ToNovelIllustrationDto)],
            [.. content.GlossaryItems.Select(ToNovelGlossaryItemDto)]);

    private static string BuildNovelMarkdown(NovelContent content)
    {
        using var context = new NovelContext(content);
        ((INovelContext<System.IO.Stream>) context).InitImages();
        return context.LoadMdContent().ToString();
    }

    private static PixevalNovelNavigationDto? ToNovelNavigationDto(NovelNavigation? navigation) =>
        navigation is null
            ? null
            : new(
                navigation.Id,
                navigation.Viewable,
                navigation.ContentOrder,
                navigation.Title,
                navigation.CoverUrl,
                navigation.ViewableMessage);

    private static PixevalNovelImageDto ToNovelImageDto(NovelImage image) =>
        new(
            image.NovelImageId,
            image.Sl,
            image.ThumbnailUrl,
            image.OriginalUrl);

    private static PixevalNovelIllustrationDto ToNovelIllustrationDto(NovelIllustration illustration) =>
        new(
            illustration.Id,
            illustration.Page,
            illustration.Visible,
            illustration.AvailableMessage,
            illustration.Illustration.Title,
            illustration.Illustration.Description,
            illustration.ThumbnailUrl,
            illustration.Illustration.Images.Original,
            illustration.WebsiteUri.ToString(),
            illustration.AppUri.ToString(),
            new(
                illustration.User.Id,
                illustration.User.Name,
                illustration.User.Image),
            [.. illustration.Illustration.Tags.Select(static tag => tag.Tag)]);

    private static PixevalNovelGlossaryItemDto ToNovelGlossaryItemDto(NovelReplaceableGlossary item) =>
        new(
            item.Id,
            item.Name,
            item.Overview,
            ToNovelImageDto(item.Cover));
}
