// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using Mako;
using Mako.Global.Enum;
using Mako.Model;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Pixeval.Mcp.Dtos;
using static Pixeval.Mcp.PixevalMcpHelpers;

namespace Pixeval.Mcp;

[McpServerToolType]
internal sealed class PixevalMcpReadTools(IPixevalMcpRuntime runtime)
{
    [McpServerTool(Name = "status", Title = "Pixeval status", ReadOnly = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalStatusDto))]
    [Description("Returns Pixeval MCP server status, active account metadata, and current content filter.")]
    public CallToolResult Status() =>
        Execute(nameof(Status), () => PixevalMcpResult.Success(PixevalStatusDto.FromRuntime(runtime)));

    [McpServerTool(Name = "capabilities", Title = "Get Pixeval MCP capabilities", ReadOnly = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalCapabilitiesDto))]
    [Description("Returns the current MCP capability flags and binary resource limits configured in Pixeval.")]
    public CallToolResult Capabilities() =>
        Execute(nameof(Capabilities), () => PixevalMcpResult.Success(PixevalCapabilitiesDto.FromRuntime(runtime)));

    [McpServerTool(Name = "help", Title = "Get Pixeval MCP help", ReadOnly = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalHelpDto))]
    [Description(
        "Returns existing Pixeval help documents for AI use. Topics: all, mcp, download_macro, work_filter, extensions.")]
    public CallToolResult Help(
        [Description("Help topic: all, mcp, download_macro, work_filter, or extensions.")]
        string? topic = null) =>
        Execute(nameof(Help), () => PixevalMcpResult.Success(runtime.Help(topic)));

    [McpServerTool(Name = "download_macro", Title = "Get Pixeval download macro",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalDownloadMacroSettingsDto))]
    [Description(
        "Returns Pixeval's current download path macro, parser diagnostics, and available macro definitions. Use help(topic: \"download_macro\") for the existing syntax document.")]
    public CallToolResult DownloadMacro() =>
        Execute(nameof(DownloadMacro), () => PixevalMcpResult.Success(runtime.DownloadMacro()));

    [McpServerTool(Name = "analyze_download_macro", Title = "Analyze Pixeval download macro",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalDownloadMacroAnalysisDto))]
    [Description(
        "Parses a Pixeval download path macro and returns diagnostics and highlight spans. Use help(topic: \"download_macro\") for the existing syntax document.")]
    public CallToolResult AnalyzeDownloadMacro(
        [Description("Download path macro text. Empty uses an empty macro, not the current setting.")]
        string? text = null) =>
        Execute(nameof(AnalyzeDownloadMacro), () =>
            PixevalMcpResult.Success(runtime.AnalyzeDownloadMacro(text)));

    [McpServerTool(Name = "preview_download_macro", Title = "Preview Pixeval download macro",
        ReadOnly = true, OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalDownloadMacroPreviewDto))]
    [Description(
        "Previews the paths produced by a Pixeval download macro. Without an id it returns Pixeval's built-in sample previews. Use help(topic: \"download_macro\") for syntax.")]
    public Task<CallToolResult> PreviewDownloadMacroAsync(
        [Description("Download path macro text. Empty uses Pixeval's current configured macro.")]
        string? text = null,
        [Description("Optional work type for a real preview. Use illustration, manga, or novel.")]
        WorkType? workType = null,
        [Description("Optional Pixiv work id for a real preview.")]
        long? id = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(PreviewDownloadMacroAsync), async () =>
            PixevalMcpResult.Success(await runtime.PreviewDownloadMacroAsync(
                    text,
                    workType,
                    id,
                    cancellationToken)
                .ConfigureAwait(false)));

    [McpServerTool(Name = "analyze_work_filter", Title = "Analyze Pixeval work filter",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkFilterAnalysisDto))]
    [Description(
        "Parses a Pixeval work filter expression and returns diagnostics and completions. Use help(topic: \"work_filter\") for the existing syntax document.")]
    public CallToolResult AnalyzeWorkFilter(
        [Description("Pixeval work filter expression.")]
        string? text = null,
        [Description("Caret position for completion. -1 means the end of text.")]
        int caretPosition = -1) =>
        Execute(nameof(AnalyzeWorkFilter), () =>
            PixevalMcpResult.Success(runtime.AnalyzeWorkFilter(text, caretPosition)));

    [McpServerTool(Name = "filter_works", Title = "Filter Pixiv works by Pixeval filter",
        ReadOnly = true, OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Loads Pixiv works by id and filters them with Pixeval's work filter language. Use help(topic: \"work_filter\") for syntax.")]
    public Task<CallToolResult> FilterWorksAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType,
        [Description("Pixiv work ids. At most 100 ids are loaded.")]
        IReadOnlyList<long> ids,
        [Description("Pixeval work filter expression.")]
        string workFilter,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(FilterWorksAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            if (ids.Count is 0)
                throw new PixevalMcpException("At least one work id is required.");

            var works = await LoadWorksAsync(workType, ids, cancellationToken).ConfigureAwait(false);
            return PixevalMcpResult.Success(CreateWorkListDto(runtime, works, workFilter));
        });

    [McpServerTool(Name = "history", Title = "Get Pixeval history",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalHistoryListDto))]
    [Description(
        "Reads Pixeval local history as JSON. Supported types: search, browse, download, watch_later, work_subscription.")]
    public Task<CallToolResult> HistoryAsync(
        [Description("History type. Use search, browse, download, watch_later, or work_subscription.")]
        PixevalHistoryType type,
        [Description("Number of items to skip. Negative values are treated as 0.")]
        int skip = 0,
        [Description("Maximum number of items to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        [Description("Optional case-insensitive keyword filter.")]
        string? keyword = null,
        [Description(
            "Optional Pixeval work filter for browse, download, and watch_later history. Use help(topic: \"work_filter\") for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(HistoryAsync), async () =>
            PixevalMcpResult.Success(await runtime.HistoryAsync(
                    type,
                    skip,
                    limit,
                    keyword,
                    workFilter,
                    cancellationToken)
                .ConfigureAwait(false)));

    [McpServerTool(Name = "extensions", Title = "Get Pixeval extensions",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalExtensionListDto))]
    [Description(
        "Returns loaded Pixeval extension hosts, extension kinds, and setting schemas without setting values. Use help(topic: \"extensions\") for the existing extension document.")]
    public CallToolResult Extensions() =>
        Execute(nameof(Extensions), () => PixevalMcpResult.Success(runtime.Extensions()));

    [McpServerTool(Name = "settings_summary", Title = "Get Pixeval settings summary",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalSettingsSummaryDto))]
    [Description(
        "Returns a sanitized Pixeval settings summary. Secrets such as cookies, API keys, proxy addresses, and extension setting values are not returned.")]
    public CallToolResult SettingsSummary() =>
        Execute(nameof(SettingsSummary), () => PixevalMcpResult.Success(runtime.SettingsSummary()));

    [McpServerTool(Name = "novel_content", Title = "Get Pixiv novel content", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalNovelContentDto))]
    [Description("Gets a Pixiv novel's text content and Pixeval-parsed Markdown without exposing local files.")]
    public Task<CallToolResult> NovelContentAsync(
        [Description("Pixiv novel id.")] long id,
        [Description("Whether to include Pixeval-parsed Markdown in addition to Pixiv's raw novel text.")]
        bool includeMarkdown = true,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(NovelContentAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await runtime.NovelContentAsync(
                    id,
                    includeMarkdown,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "saucenao_search", Title = "Search SauceNAO by image", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalSauceNaoSearchDto))]
    [Description(
        "Runs SauceNAO reverse image search using Pixeval's configured SauceNAO API key. Provide exactly one of imageBase64, imageUrl, or illustrationId.")]
    public Task<CallToolResult> SauceNaoSearchAsync(
        [Description("Base64 image bytes. Data URLs are accepted.")]
        string? imageBase64 = null,
        [Description("Image URL to download and submit to SauceNAO.")]
        string? imageUrl = null,
        [Description("Pixiv illustration id whose original image should be submitted to SauceNAO.")]
        long? illustrationId = null,
        [Description("Zero-based illustration page when illustrationId is used.")]
        int page = 0,
        [Description("Maximum number of SauceNAO results. Clamped to 1..100.")]
        int limit = DefaultLimit,
        [Description("SauceNAO index name or numeric mask. Empty searches all indexes.")]
        string? index = null,
        [Description("Minimum similarity percentage to include.")]
        double minSimilarity = 0,
        [Description("Whether Pixeval should try to load Pixiv work metadata for Pixiv hits.")]
        bool loadPixivWorks = false,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SauceNaoSearchAsync), async () =>
            PixevalMcpResult.Success(await runtime.SauceNaoSearchAsync(
                    imageBase64,
                    imageUrl,
                    illustrationId,
                    page,
                    limit,
                    index,
                    minSimilarity,
                    loadPixivWorks,
                    cancellationToken)
                .ConfigureAwait(false)));

    [McpServerTool(Name = "search_illustrations", Title = "Search Pixiv illustrations", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Searches Pixiv illustrations and manga using Pixeval's current login, network settings, and content filter.")]
    public Task<CallToolResult> SearchIllustrationsAsync(
        [Description("Search keyword or tag text.")]
        string query,
        [Description("Maximum number of works to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        [Description("Match mode. Use keyword, partial_match_for_tags, exact_match_for_tags, or title_and_caption.")]
        SearchIllustrationTagMatchOption match = SearchIllustrationTagMatchOption.Keyword,
        [Description(
            "Sort mode. Use publish_date_descending, publish_date_ascending, or popularity_descending. Popular sort requires Pixiv Premium.")]
        WorkSortOption sort = WorkSortOption.PublishDateDescending,
        [Description("Whether Pixiv AI-generated works should be included.")]
        bool includeAi = true,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help(topic: \"work_filter\") for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SearchIllustrationsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var args = CreateIllustrationSearchArguments(query, match, sort, includeAi);
            var works = await TakeWorkModelsAsync(runtime.MakoClient.IllustrationSearch(args), limit, cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(CreateWorkListDto(runtime, works, workFilter));
        });

    [McpServerTool(Name = "search_novels", Title = "Search Pixiv novels", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description("Searches Pixiv novels using Pixeval's current login, network settings, and content filter.")]
    public Task<CallToolResult> SearchNovelsAsync(
        [Description("Search keyword or tag text.")]
        string query,
        [Description("Maximum number of novels to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        [Description("Match mode. Use keyword, partial_match_for_tags, exact_match_for_tags, or text.")]
        SearchNovelTagMatchOption match = SearchNovelTagMatchOption.Keyword,
        [Description(
            "Sort mode. Use publish_date_descending, publish_date_ascending, or popularity_descending. Popular sort requires Pixiv Premium.")]
        WorkSortOption sort = WorkSortOption.PublishDateDescending,
        [Description("Whether Pixiv AI-generated works should be included.")]
        bool includeAi = true,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help(topic: \"work_filter\") for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SearchNovelsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var args = CreateNovelSearchArguments(query, match, sort, includeAi);
            var works = await TakeWorkModelsAsync(runtime.MakoClient.NovelSearch(args), limit, cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(CreateWorkListDto(runtime, works, workFilter));
        });

    [McpServerTool(Name = "works", Title = "Get Pixiv works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description("Gets metadata for multiple Pixiv illustrations/manga or novels by id.")]
    public Task<CallToolResult> WorksAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType,
        [Description("Pixiv work ids. At most 100 ids are loaded.")]
        IReadOnlyList<long> ids,
        [Description(
            "Optional Pixeval work filter expression applied to the loaded works. Use help(topic: \"work_filter\") for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(WorksAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            if (ids.Count is 0)
                throw new PixevalMcpException("At least one work id is required.");

            var works = await LoadWorksAsync(workType, ids, cancellationToken).ConfigureAwait(false);
            return PixevalMcpResult.Success(CreateWorkListDto(runtime, works, workFilter));
        });

    [McpServerTool(Name = "users", Title = "Get Pixiv users", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Gets public metadata for multiple Pixiv users by user id.")]
    public Task<CallToolResult> UsersAsync(
        [Description("Pixiv user ids. At most 100 ids are loaded.")]
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(UsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            if (ids.Count is 0)
                throw new PixevalMcpException("At least one user id is required.");

            var users = new List<PixevalUserDto>(int.Min(ids.Count, MaxLimit));
            foreach (var id in ids.Distinct().Take(MaxLimit))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var user = await runtime.MakoClient.GetUserFromIdAsync(id).WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
                users.Add(PixevalUserDto.FromSingleUserResponse(user));
            }

            return PixevalMcpResult.Success(new PixevalUserListDto(users.Count, users));
        });

    [McpServerTool(Name = "thumbnails", Title = "Get Pixiv work thumbnails",
        ReadOnly = true, OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalThumbnailInfoDto))]
    [Description("Returns Pixeval MCP resource URIs for thumbnail binary resources of an illustration/manga or novel.")]
    public Task<CallToolResult> ThumbnailsAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType,
        [Description("Pixiv work id.")] long id,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(ThumbnailsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var work = workType is WorkType.Novel
                ? (WorkBase) await runtime.MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken)
                    .ConfigureAwait(false)
                : await runtime.MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            return PixevalMcpResult.Success(PixevalThumbnailInfoDto.FromWork(work));
        });

    [McpServerTool(Name = "ranking", Title = "Get Pixiv ranking", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description("Gets Pixiv ranking works for illustrations/manga or novels.")]
    public Task<CallToolResult> RankingAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType = WorkType.Illustration,
        [Description("Pixiv ranking option. Use values such as day, week, month, day_r18, or week_rookie.")]
        RankOption rankOption = RankOption.Day,
        [Description("Ranking date in yyyy-MM-dd form. Empty uses Pixiv's latest available ranking day.")]
        string? date = null,
        [Description("Maximum number of works to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help(topic: \"work_filter\") for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(RankingAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var rankingDate = string.IsNullOrWhiteSpace(date)
                ? MakoClient.RankingMaxDateTime
                : new DateTimeOffset(DateTime.Parse(date));
            var works = await TakeWorkModelsAsync(runtime.MakoClient.WorkRanking(
                    ToSimpleWorkType(workType),
                    rankOption,
                    rankingDate),
                limit,
                cancellationToken).ConfigureAwait(false);
            return PixevalMcpResult.Success(CreateWorkListDto(runtime, works, workFilter));
        });

    [McpServerTool(Name = "bookmarks", Title = "Get Pixiv bookmarks", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description("Gets public or own private Pixiv bookmarks for a user.")]
    public Task<CallToolResult> BookmarksAsync(
        [Description("Pixiv user id. Empty uses the currently logged-in Pixeval account.")]
        long? userId = null,
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType = WorkType.Illustration,
        [Description("Bookmark privacy. Use public or private. Private only works for the logged-in account.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        [Description("Optional bookmark tag name.")]
        string? tag = null,
        [Description("Maximum number of works to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help(topic: \"work_filter\") for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(BookmarksAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var uid = userId ?? runtime.CurrentUser?.Id ??
                throw new PixevalMcpException("Current Pixiv user is not available.");
            var works = await TakeWorkModelsAsync(runtime.MakoClient.WorkBookmarks(
                    uid,
                    ToSimpleWorkType(workType),
                    privacy,
                    tag),
                limit,
                cancellationToken).ConfigureAwait(false);
            return PixevalMcpResult.Success(CreateWorkListDto(runtime, works, workFilter));
        });

    [McpServerTool(Name = "bookmark_tags", Title = "Get Pixiv bookmark tags", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalBookmarkTagListDto))]
    [Description("Gets bookmark tag counts for the logged-in account or a public Pixiv user.")]
    public Task<CallToolResult> BookmarkTagsAsync(
        [Description("Pixiv user id. Empty uses the currently logged-in Pixeval account.")]
        long? userId = null,
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType = WorkType.Illustration,
        [Description("Bookmark privacy. Use public or private. Private only works for the logged-in account.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(BookmarkTagsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await runtime.BookmarkTagsAsync(
                    userId,
                    ToSimpleWorkType(workType),
                    privacy,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "trending_tags", Title = "Get Pixiv trending tags", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalTrendingTagListDto))]
    [Description("Gets Pixiv trending tags for illustrations/manga or novels.")]
    public Task<CallToolResult> TrendingTagsAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType = WorkType.Illustration,
        [Description("Maximum number of tags to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(TrendingTagsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var tags = workType is WorkType.Novel
                ? await runtime.MakoClient.GetNovelTrendingTagsAsync().WaitAsync(cancellationToken)
                    .ConfigureAwait(false)
                : await runtime.MakoClient.GetIllustrationTrendingTagsAsync().WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            var result = tags.Take(ClampLimit(limit)).Select(PixevalTrendingTagDto.FromTrendingTag).ToArray();
            return PixevalMcpResult.Success(new PixevalTrendingTagListDto(result.Length, result));
        });

    [McpServerTool(Name = "search_users", Title = "Search Pixiv users", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Searches Pixiv users by keyword.")]
    public Task<CallToolResult> SearchUsersAsync(
        [Description("User search keyword.")] string query,
        [Description("Maximum number of users to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SearchUsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var users = await TakeUsersAsync(runtime.MakoClient.UserSearch(query), limit, cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(new PixevalUserListDto(users.Count, users));
        });

    [McpServerTool(Name = "comments", Title = "Get Pixiv work comments", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalCommentListDto))]
    [Description("Gets top-level comments for an illustration/manga or novel.")]
    public Task<CallToolResult> CommentsAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType,
        [Description("Pixiv work id.")] long workId,
        [Description("Maximum number of comments to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(CommentsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var comments = await TakeCommentsAsync(
                    runtime.MakoClient.WorkComments(ToSimpleWorkType(workType), workId),
                    limit,
                    runtime.CurrentUser?.Id,
                    cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(new PixevalCommentListDto(comments.Count, comments));
        });

    [McpServerTool(Name = "comment_replies", Title = "Get Pixiv comment replies", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalCommentListDto))]
    [Description("Gets replies for a top-level illustration/manga or novel comment.")]
    public Task<CallToolResult> CommentRepliesAsync(
        [Description("Work type. Use illustration, manga, or novel.")]
        WorkType workType,
        [Description("Top-level Pixiv comment id.")]
        long commentId,
        [Description("Maximum number of replies to return. Clamped to 1..100.")]
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(CommentRepliesAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var comments = await TakeCommentsAsync(
                    runtime.MakoClient.WorkCommentReplies(ToSimpleWorkType(workType), commentId),
                    limit,
                    runtime.CurrentUser?.Id,
                    cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(new PixevalCommentListDto(comments.Count, comments));
        });

    [McpServerTool(Name = "download_tasks", Title = "Get Pixeval download tasks", ReadOnly = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalMcpDownloadTaskListDto))]
    [Description("Returns current Pixeval download task states.")]
    public CallToolResult DownloadTasks() =>
        Execute(nameof(DownloadTasks), () =>
        {
            var tasks = runtime.DownloadTasks();
            return PixevalMcpResult.Success(new PixevalMcpDownloadTaskListDto(tasks.Count, tasks));
        });

    private async Task<IReadOnlyList<WorkBase>> LoadWorksAsync(
        WorkType workType,
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken)
    {
        var works = new List<WorkBase>(int.Min(ids.Count, MaxLimit));
        foreach (var id in ids.Distinct().Take(MaxLimit))
        {
            cancellationToken.ThrowIfCancellationRequested();
            works.Add(workType is WorkType.Novel
                ? await runtime.MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken)
                    .ConfigureAwait(false)
                : await runtime.MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken)
                    .ConfigureAwait(false));
        }

        return works;
    }

    private CallToolResult Execute(string toolName, Func<CallToolResult> action)
    {
        try
        {
            return action();
        }
        catch (PixevalMcpException e)
        {
            return PixevalMcpResult.Error(e.Message);
        }
        catch (Exception e)
        {
            runtime.LogToolException(toolName, e);
            return PixevalMcpResult.Error("Pixeval MCP tool failed. See Pixeval MCP logs for details.");
        }
    }

    private async Task<CallToolResult> ExecuteAsync(string toolName, Func<Task<CallToolResult>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (PixevalMcpException e)
        {
            return PixevalMcpResult.Error(e.Message);
        }
        catch (OperationCanceledException)
        {
            return PixevalMcpResult.Error("The MCP request was canceled.");
        }
        catch (Exception e)
        {
            runtime.LogToolException(toolName, e);
            return PixevalMcpResult.Error("Pixeval MCP tool failed. See Pixeval MCP logs for details.");
        }
    }
}
