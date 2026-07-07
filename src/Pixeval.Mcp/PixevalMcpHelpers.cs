// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Mcp;

internal static class PixevalMcpHelpers
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    public static int ClampLimit(int limit) => int.Clamp(limit, 1, MaxLimit);

    public static SimpleWorkType ToSimpleWorkType(WorkType workType) =>
        workType is WorkType.Novel ? SimpleWorkType.Novel : SimpleWorkType.IllustrationAndManga;

    public static async Task<IReadOnlyList<PixevalWorkDto>> TakeWorksAsync<T>(
        IAsyncEnumerable<T> works,
        int limit,
        CancellationToken cancellationToken)
        where T : IWorkEntry
    {
        var models = await TakeWorkModelsAsync(works, limit, cancellationToken).ConfigureAwait(false);
        return [.. models.Select(PixevalWorkDto.FromWork)];
    }

    public static async Task<IReadOnlyList<WorkBase>> TakeWorkModelsAsync<T>(
        IAsyncEnumerable<T> works,
        int limit,
        CancellationToken cancellationToken)
        where T : IWorkEntry
    {
        var result = new List<WorkBase>(ClampLimit(limit));
        await foreach (var work in works.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (work is not WorkBase workBase)
                throw new PixevalMcpException("Pixiv returned a work shape that Pixeval MCP cannot serialize yet.");

            result.Add(workBase);
            if (result.Count >= ClampLimit(limit))
                break;
        }

        return result;
    }

    public static PixevalWorkListDto CreateWorkListDto(
        IPixevalMcpRuntime runtime,
        IReadOnlyList<WorkBase> works,
        string? workFilter)
    {
        if (string.IsNullOrWhiteSpace(workFilter))
        {
            var unfiltered = works.Select(PixevalWorkDto.FromWork).ToArray();
            return new(unfiltered.Length, unfiltered);
        }

        var analysis = runtime.AnalyzeWorkFilter(workFilter, -1);
        if (!analysis.IsSuccess)
            return new(0, [], analysis);

        var filtered = runtime.FilterWorks(works, workFilter).Select(PixevalWorkDto.FromWork).ToArray();
        return new(filtered.Length, filtered, analysis);
    }

    public static async Task<IReadOnlyList<PixevalUserDto>> TakeUsersAsync(
        IAsyncEnumerable<User> users,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = new List<PixevalUserDto>(ClampLimit(limit));
        await foreach (var user in users.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result.Add(PixevalUserDto.FromUser(user));
            if (result.Count >= ClampLimit(limit))
                break;
        }

        return result;
    }

    public static async Task<IReadOnlyList<PixevalCommentDto>> TakeCommentsAsync(
        IAsyncEnumerable<Comment> comments,
        int limit,
        long? currentUserId,
        CancellationToken cancellationToken)
    {
        var result = new List<PixevalCommentDto>(ClampLimit(limit));
        await foreach (var comment in comments.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result.Add(PixevalCommentDto.FromComment(comment, currentUserId));
            if (result.Count >= ClampLimit(limit))
                break;
        }

        return result;
    }

    public static string GetImageMimeType(string url)
    {
        var path = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.AbsolutePath : url;
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    public static IReadOnlyList<string> GetOriginalImagePages(Illustration illustration) =>
        illustration.PageCount <= 1
            ? [illustration.OriginalSingleUrl ?? illustration.MetaSinglePage.OriginalImageUrl ?? ""]
            : [.. illustration.MetaPages.Select(static t => t.OriginalUrl)];

    public static IllustrationSearchArguments CreateIllustrationSearchArguments(
        string query,
        SearchIllustrationTagMatchOption match,
        WorkSortOption sort,
        bool includeAi) =>
        new(query)
        {
            MatchOption = match,
            SortOption = sort,
            AiType = includeAi
        };

    public static NovelSearchArguments CreateNovelSearchArguments(
        string query,
        SearchNovelTagMatchOption match,
        WorkSortOption sort,
        bool includeAi) =>
        new(query)
        {
            MatchOption = match,
            SortOption = sort,
            AiType = includeAi
        };
}
