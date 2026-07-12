// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using Mako;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Pixeval.Mcp.Dtos;
using static Pixeval.Mcp.PixevalMcpHelpers;

namespace Pixeval.Mcp;

[McpServerToolType]
internal sealed class PixevalMcpReadTools(IPixevalMcpRuntime runtime, PixevalMcpCursorStore cursorStore)
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
    [Description("Returns existing Pixeval help documents for AI use.")]
    public CallToolResult Help(
        [Description("Optional help topic.")] PixevalHelpTopic? topic = null) =>
        Execute(nameof(Help), () => PixevalMcpResult.Success(runtime.Help(topic)));

    [McpServerTool(Name = "more", Title = "Continue Pixeval MCP cursor", ReadOnly = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalCursorPageDto))]
    [Description(
        "Continues a previous Pixeval MCP list result. Pass nextCursor from list tools; original query arguments are kept by Pixeval.")]
    public Task<CallToolResult> MoreAsync(
        [Description("Cursor token returned as nextCursor by a previous list call.")]
        string cursor,
        [Description("Maximum number of items to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(MoreAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.MoreAsync(runtime, cursor, count, cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "download_macro", Title = "Get Pixeval download macro",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalDownloadMacroSettingsDto))]
    [Description(
        "Returns Pixeval's current download path macro, parser diagnostics, and available macro definitions. Use help for the existing syntax document.")]
    public CallToolResult DownloadMacro() =>
        Execute(nameof(DownloadMacro), () => PixevalMcpResult.Success(runtime.DownloadMacro()));

    [McpServerTool(Name = "analyze_download_macro", Title = "Analyze Pixeval download macro",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalDownloadMacroAnalysisDto))]
    [Description(
        "Parses a Pixeval download path macro and returns diagnostics and highlight spans. Use help for the existing syntax document.")]
    public CallToolResult AnalyzeDownloadMacro(
        [Description("Download path macro text. Must not be empty.")]
        string text) =>
        Execute(nameof(AnalyzeDownloadMacro), () =>
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new PixevalMcpException("Download macro text is required.");

            return PixevalMcpResult.Success(runtime.AnalyzeDownloadMacro(text));
        });

    [McpServerTool(Name = "analyze_work_filter", Title = "Analyze Pixeval work filter",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkFilterAnalysisDto))]
    [Description(
        "Parses a Pixeval work filter expression and returns diagnostics and completions. Use help for the existing syntax document.")]
    public CallToolResult AnalyzeWorkFilter(
        [Description("Pixeval work filter expression.")]
        string? text = null,
        [Description("Caret position for completion. -1 means the end of text.")]
        int caretPosition = -1) =>
        Execute(nameof(AnalyzeWorkFilter), () =>
            PixevalMcpResult.Success(runtime.AnalyzeWorkFilter(text, caretPosition)));

    [McpServerTool(Name = "history", Title = "Get Pixeval history",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalHistoryListDto))]
    [Description("Reads Pixeval local history as JSON.")]
    public Task<CallToolResult> HistoryAsync(
        [Description("History type.")] PixevalHistoryType type,
        [Description("Number of items to skip. Negative values are treated as 0.")]
        int skip = 0,
        [Description("Maximum number of items to return. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description("Optional case-insensitive keyword filter.")]
        string? keyword = null,
        [Description(
            "Optional Pixeval work filter for work-like history entries. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(HistoryAsync), async () =>
            PixevalMcpResult.Success(await runtime.HistoryAsync(
                    type,
                    skip,
                    count,
                    keyword,
                    workFilter,
                    cancellationToken)
                .ConfigureAwait(false)));

    [McpServerTool(Name = "extensions", Title = "Get Pixeval extensions",
        ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalExtensionListDto))]
    [Description(
        "Returns loaded Pixeval extension hosts, extension kinds, and setting schemas without setting values. Use help for the existing extension document.")]
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
        int count = DefaultCount,
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
                    count,
                    index,
                    minSimilarity,
                    loadPixivWorks,
                    cancellationToken)
                .ConfigureAwait(false)));

    [McpServerTool(Name = "search_illustrations", Title = "Search Pixiv illustrations", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Searches Pixiv illustrations and manga using Pixeval's current login, network settings, and content filter. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> SearchIllustrationsAsync(
        [Description("Search keyword or tag text.")]
        string query,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description("Pixiv search match mode.")]
        SearchIllustrationTagMatchOption match = SearchIllustrationTagMatchOption.Keyword,
        [Description("Sort mode. Popular sort requires Pixiv Premium.")]
        WorkSortOption sort = WorkSortOption.PublishDateDescending,
        [Description("Whether Pixiv AI-generated works should be included.")]
        bool includeAi = true,
        [Description("Pixiv illustration search content type.")]
        SearchIllustrationContentType contentType = SearchIllustrationContentType.IllustrationAndMangaAndUgoira,
        [Description("Pixiv illustration aspect-ratio filter.")]
        SearchIllustrationRatioPattern ratioPattern = SearchIllustrationRatioPattern.All,
        [Description("Optional start date in yyyy-MM-dd form.")]
        string? startDate = null,
        [Description("Optional end date in yyyy-MM-dd form.")]
        string? endDate = null,
        [Description("Whether Pixiv should merge plain keyword results.")]
        bool mergePlainKeywordResults = true,
        [Description("Whether Pixiv should include translated tag results.")]
        bool includeTranslatedTagResults = true,
        [Description("Whether Pixiv should include potential violation works.")]
        bool includePotentialViolationWorks = false,
        [Description("Minimum image width.")] int? widthMin = null,
        [Description("Maximum image width.")] int? widthMax = null,
        [Description("Minimum image height.")] int? heightMin = null,
        [Description("Maximum image height.")] int? heightMax = null,
        [Description("Optional Pixiv illustration creation tool filter.")]
        string? tool = null,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SearchIllustrationsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var filter = AnalyzeListWorkFilter(workFilter);
            if (filter is { IsSuccess: false })
                return PixevalMcpResult.Success(new PixevalWorkListDto(0, [], filter));

            var args = CreateIllustrationSearchArguments(
                query,
                match,
                sort,
                includeAi,
                contentType,
                ratioPattern,
                startDate,
                endDate,
                mergePlainKeywordResults,
                includeTranslatedTagResults,
                includePotentialViolationWorks,
                widthMin,
                widthMax,
                heightMin,
                heightMax,
                tool);
            return PixevalMcpResult.Success(await cursorStore.CreateWorkCursorAsync(
                    runtime.MakoClient.IllustrationSearch(args),
                    runtime,
                    count,
                    workFilter,
                    filter,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "search_novels", Title = "Search Pixiv novels", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Searches Pixiv novels using Pixeval's current login, network settings, and content filter. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> SearchNovelsAsync(
        [Description("Search keyword or tag text.")]
        string query,
        [Description("Maximum number of novels to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description("Pixiv search match mode.")]
        SearchNovelTagMatchOption match = SearchNovelTagMatchOption.Keyword,
        [Description("Sort mode. Popular sort requires Pixiv Premium.")]
        WorkSortOption sort = WorkSortOption.PublishDateDescending,
        [Description("Whether Pixiv AI-generated works should be included.")]
        bool includeAi = true,
        [Description("Optional Pixiv novel language code.")]
        string? language = null,
        [Description("Novel content-length filter mode.")]
        SearchNovelContentLengthOption contentLengthOption = SearchNovelContentLengthOption.None,
        [Description("Minimum novel content length.")]
        int? contentLengthMin = null,
        [Description("Maximum novel content length.")]
        int? contentLengthMax = null,
        [Description("Whether to include original novels only.")]
        bool originalOnly = false,
        [Description("Optional Pixiv genre id. Only meaningful when originalOnly is true.")]
        int? genreId = null,
        [Description("Whether to include replaceable novels only.")]
        bool replaceableOnly = false,
        [Description("Optional start date in yyyy-MM-dd form.")]
        string? startDate = null,
        [Description("Optional end date in yyyy-MM-dd form.")]
        string? endDate = null,
        [Description("Whether Pixiv should merge plain keyword results.")]
        bool mergePlainKeywordResults = true,
        [Description("Whether Pixiv should include translated tag results.")]
        bool includeTranslatedTagResults = true,
        [Description("Whether Pixiv should include potential violation works.")]
        bool includePotentialViolationWorks = false,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SearchNovelsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var filter = AnalyzeListWorkFilter(workFilter);
            if (filter is { IsSuccess: false })
                return PixevalMcpResult.Success(new PixevalWorkListDto(0, [], filter));

            var args = CreateNovelSearchArguments(
                query,
                match,
                sort,
                includeAi,
                language,
                contentLengthOption,
                contentLengthMin,
                contentLengthMax,
                originalOnly,
                genreId,
                replaceableOnly,
                startDate,
                endDate,
                mergePlainKeywordResults,
                includeTranslatedTagResults,
                includePotentialViolationWorks);
            return PixevalMcpResult.Success(await cursorStore.CreateWorkCursorAsync(
                    runtime.MakoClient.NovelSearch(args),
                    runtime,
                    count,
                    workFilter,
                    filter,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "recommended_works", Title = "Get Pixiv recommended works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets Pixiv recommended works for illustrations, manga, or novels. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> RecommendedWorksAsync(
        [Description("Work type.")] WorkType workType = WorkType.Illustration,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description("Whether Pixiv ranking works should be included.")]
        bool includeRankingWorks = true,
        [Description("Whether Pixiv privacy policy entries should be included.")]
        bool includePrivacyPolicy = true,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteWorkCursorAsync(
            nameof(RecommendedWorksAsync),
            () => runtime.MakoClient.WorkRecommended(
                workType,
                includeRankingWorks,
                includePrivacyPolicy),
            count,
            workFilter,
            cancellationToken);

    [McpServerTool(Name = "new_works", Title = "Get newest Pixiv works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets newest Pixiv illustrations, manga, or novels. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> NewWorksAsync(
        [Description("Work type.")] WorkType workType = WorkType.Illustration,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description("Optional maximum Pixiv work id for continuing from an older server-side position.")]
        uint? maxWorkId = null,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteWorkCursorAsync(
            nameof(NewWorksAsync),
            () => runtime.MakoClient.WorkNew(workType, maxWorkId),
            count,
            workFilter,
            cancellationToken);

    [McpServerTool(Name = "following_works", Title = "Get following Pixiv works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets new works from followed users for illustrations/manga or novels. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> FollowingWorksAsync(
        [Description("Work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Follow privacy. Private only works for the logged-in account.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteWorkCursorAsync(
            nameof(FollowingWorksAsync),
            () => runtime.MakoClient.WorkFollowing(workType, privacy),
            count,
            workFilter,
            cancellationToken);

    [McpServerTool(Name = "posts", Title = "Get Pixiv user posts", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets works posted by a Pixiv user. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> PostsAsync(
        [Description("Pixiv user id.")] long userId,
        [Description("Work type.")] WorkType workType = WorkType.Illustration,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteWorkCursorAsync(
            nameof(PostsAsync),
            () => runtime.MakoClient.WorkPosted(userId, workType),
            count,
            workFilter,
            cancellationToken);

    [McpServerTool(Name = "my_pixiv_works", Title = "Get Pixiv MyPixiv works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets works from MyPixiv users for illustrations/manga or novels. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> MyPixivWorksAsync(
        [Description("Work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteWorkCursorAsync(
            nameof(MyPixivWorksAsync),
            () => runtime.MakoClient.WorkMyPixiv(workType),
            count,
            workFilter,
            cancellationToken);

    [McpServerTool(Name = "related_works", Title = "Get Pixiv related works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets works related to a Pixiv illustration/manga or novel. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> RelatedWorksAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv illustration/manga or novel id.")]
        long id,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteWorkCursorAsync(
            nameof(RelatedWorksAsync),
            () => runtime.MakoClient.WorkRelated(id, workType),
            count,
            workFilter,
            cancellationToken);

    [McpServerTool(Name = "series_watchlist", Title = "Get Pixiv series watchlist", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalSeriesListDto))]
    [Description(
        "Gets the logged-in account's Pixiv manga or novel series watchlist. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> SeriesWatchlistAsync(
        [Description("Series work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Maximum number of series to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SeriesWatchlistAsync), async () =>
        {
            runtime.EnsureLoggedIn();

            return PixevalMcpResult.Success(await cursorStore.CreateSeriesCursorAsync(
                    runtime.MakoClient.WorkSeriesWatchlist(workType),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "works", Title = "Get Pixiv works", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description("Gets metadata for multiple Pixiv illustrations/manga or novels by id.")]
    public Task<CallToolResult> WorksAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work ids. At most 100 ids are loaded.")]
        IReadOnlyList<long> ids,
        [Description(
            "Optional Pixeval work filter expression applied to the loaded works. Use help for syntax.")]
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

            var users = new List<PixevalUserDto>(int.Min(ids.Count, MaxCount));
            foreach (var id in ids.Distinct().Take(MaxCount))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var user = await runtime.GetUserAsync(id, cancellationToken).ConfigureAwait(false);
                users.Add(PixevalUserDto.FromSingleUserResponse(user));
            }

            return PixevalMcpResult.Success(new PixevalUserListDto(users.Count, users));
        });

    [McpServerTool(Name = "related_users", Title = "Get Pixiv related users", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Gets Pixiv users related to a seed user. Pixiv returns a fixed-size result set, so this tool does not use a cursor.")]
    public Task<CallToolResult> RelatedUsersAsync(
        [Description("Seed Pixiv user id.")] long userId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(RelatedUsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var users = await runtime.MakoClient.RelatedUserAsync(userId)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            runtime.CacheUsers(users);
            var result = users.Select(PixevalUserDto.FromUser).ToArray();
            return PixevalMcpResult.Success(new PixevalUserListDto(result.Length, result));
        });

    [McpServerTool(Name = "thumbnails", Title = "Get Pixiv work thumbnails",
        ReadOnly = true, OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalThumbnailInfoDto))]
    [Description("Returns Pixeval MCP resource URIs for thumbnail binary resources of an illustration/manga or novel.")]
    public Task<CallToolResult> ThumbnailsAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work id.")] long id,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(ThumbnailsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var work = await runtime.GetWorkAsync(workType, id, cancellationToken).ConfigureAwait(false);
            return PixevalMcpResult.Success(PixevalThumbnailInfoDto.FromWork(work));
        });

    [McpServerTool(Name = "ranking", Title = "Get Pixiv ranking", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets Pixiv ranking works for illustrations/manga or novels. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> RankingAsync(
        [Description("Work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Pixiv ranking option.")] RankOption rankOption = RankOption.Day,
        [Description("Ranking date in yyyy-MM-dd form. Empty uses Pixiv's latest available ranking day.")]
        string? date = null,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(RankingAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var filter = AnalyzeListWorkFilter(workFilter);
            if (filter is { IsSuccess: false })
                return PixevalMcpResult.Success(new PixevalWorkListDto(0, [], filter));

            var rankingDate = string.IsNullOrWhiteSpace(date)
                ? MakoClient.RankingMaxDateTime
                : new DateTimeOffset(DateTime.Parse(date));
            return PixevalMcpResult.Success(await cursorStore.CreateWorkCursorAsync(
                    runtime.MakoClient.WorkRanking(
                        workType,
                        rankOption,
                        rankingDate),
                    runtime,
                    count,
                    workFilter,
                    filter,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "bookmarks", Title = "Get Pixiv bookmarks", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkListDto))]
    [Description(
        "Gets public or own private Pixiv bookmarks for a user. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> BookmarksAsync(
        [Description("Pixiv user id. Empty uses the currently logged-in Pixeval account.")]
        long? userId = null,
        [Description("Work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Bookmark privacy. Private only works for the logged-in account.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        [Description("Optional bookmark tag name.")]
        string? tag = null,
        [Description("Maximum number of works to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        [Description(
            "Optional Pixeval work filter expression applied to the fetched result set. Use help for syntax.")]
        string? workFilter = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(BookmarksAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var filter = AnalyzeListWorkFilter(workFilter);
            if (filter is { IsSuccess: false })
                return PixevalMcpResult.Success(new PixevalWorkListDto(0, [], filter));

            var uid = userId ?? runtime.CurrentUser?.Id ??
                throw new PixevalMcpException("Current Pixiv user is not available.");
            return PixevalMcpResult.Success(await cursorStore.CreateWorkCursorAsync(
                    runtime.MakoClient.WorkBookmarks(
                        uid,
                        workType,
                        privacy,
                        tag),
                    runtime,
                    count,
                    workFilter,
                    filter,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "bookmark_tags", Title = "Get Pixiv bookmark tags", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalBookmarkTagListDto))]
    [Description(
        "Gets actual Pixiv bookmark tag counts for the logged-in account or a public Pixiv user. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> BookmarkTagsAsync(
        [Description("Pixiv user id. Empty uses the currently logged-in Pixeval account.")]
        long? userId = null,
        [Description("Work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Bookmark privacy. Private only works for the logged-in account.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        [Description("Maximum number of tags to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(BookmarkTagsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var uid = userId ?? runtime.CurrentUser?.Id ??
                throw new PixevalMcpException("Current Pixiv user is not available.");
            return PixevalMcpResult.Success(await cursorStore.CreateBookmarkTagCursorAsync(
                    runtime.MakoClient.WorkBookmarkTags(uid, workType, privacy),
                    runtime,
                    uid,
                    workType,
                    privacy,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "trending_tags", Title = "Get Pixiv trending tags", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalTrendingTagListDto))]
    [Description("Gets Pixiv trending tags for illustrations/manga or novels.")]
    public Task<CallToolResult> TrendingTagsAsync(
        [Description("Work type.")] SimpleWorkType workType = SimpleWorkType.Illustration,
        [Description("Maximum number of tags to return. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(TrendingTagsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var tags = await runtime.MakoClient.GetWorkTrendingTagsAsync(workType).WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            var selected = tags.Take(ClampCount(count)).ToArray();
            runtime.CacheWorks(selected.Select(static tag => tag.Illustration));
            var result = selected.Select(PixevalTrendingTagDto.FromTrendingTag).ToArray();
            return PixevalMcpResult.Success(new PixevalTrendingTagListDto(result.Length, result));
        });

    [McpServerTool(Name = "search_users", Title = "Search Pixiv users", ReadOnly = true, OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Searches Pixiv users by keyword. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> SearchUsersAsync(
        [Description("User search keyword.")] string query,
        [Description("Maximum number of users to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SearchUsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateUserCursorAsync(
                    runtime.MakoClient.UserSearch(query),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "recommended_users", Title = "Get Pixiv recommended users", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Gets Pixiv recommended users. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> RecommendedUsersAsync(
        [Description("Maximum number of users to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(RecommendedUsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateUserCursorAsync(
                    runtime.MakoClient.UserRecommended(),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "following_users", Title = "Get Pixiv following users", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Gets followed users for a Pixiv user. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> FollowingUsersAsync(
        [Description("Pixiv user id. Empty uses the currently logged-in Pixeval account.")]
        long? userId = null,
        [Description("Follow privacy. Private only works for the logged-in account.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        [Description("Maximum number of users to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(FollowingUsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            var uid = userId ?? runtime.CurrentUser?.Id ??
                throw new PixevalMcpException("Current Pixiv user is not available.");
            return PixevalMcpResult.Success(await cursorStore.CreateUserCursorAsync(
                    runtime.MakoClient.UserFollowing(uid, privacy),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "followers", Title = "Get Pixiv followers", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Gets users who follow the logged-in Pixiv account. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> FollowersAsync(
        [Description("Maximum number of users to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(FollowersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateUserCursorAsync(
                    runtime.MakoClient.UserFollower(),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "my_pixiv_users", Title = "Get Pixiv MyPixiv users", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalUserListDto))]
    [Description("Gets MyPixiv users for a Pixiv user. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> MyPixivUsersAsync(
        [Description("Pixiv user id.")] long userId,
        [Description("Maximum number of users to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(MyPixivUsersAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateUserCursorAsync(
                    runtime.MakoClient.UserMyPixiv(userId),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "spotlight", Title = "Get Pixivision spotlight articles", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalSpotlightListDto))]
    [Description("Gets Pixivision spotlight articles from Pixiv. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> SpotlightAsync(
        [Description("Maximum number of articles to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SpotlightAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateSpotlightCursorAsync(
                    runtime.MakoClient.Spotlight(),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "comments", Title = "Get Pixiv work comments", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalCommentListDto))]
    [Description(
        "Gets top-level comments for an illustration/manga or novel. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> CommentsAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work id.")] long workId,
        [Description("Maximum number of comments to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(CommentsAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateCommentCursorAsync(
                    runtime.MakoClient.WorkComments(workType, workId),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "comment_replies", Title = "Get Pixiv comment replies", ReadOnly = true,
        OpenWorld = true, UseStructuredContent = true, OutputSchemaType = typeof(PixevalCommentListDto))]
    [Description(
        "Gets replies for a top-level illustration/manga or novel comment. If hasMore is true, call more(nextCursor) to continue.")]
    public Task<CallToolResult> CommentRepliesAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Top-level Pixiv comment id.")]
        long commentId,
        [Description("Maximum number of replies to return in this call. Clamped to 1..100.")]
        int count = DefaultCount,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(CommentRepliesAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            return PixevalMcpResult.Success(await cursorStore.CreateCommentCursorAsync(
                    runtime.MakoClient.WorkCommentReplies(workType, commentId),
                    runtime,
                    count,
                    cancellationToken)
                .ConfigureAwait(false));
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

    private PixevalWorkFilterAnalysisDto? AnalyzeListWorkFilter(string? workFilter) =>
        string.IsNullOrWhiteSpace(workFilter)
            ? null
            : runtime.AnalyzeWorkFilter(workFilter, -1);

    private Task<CallToolResult> ExecuteWorkCursorAsync(
        string toolName,
        Func<IFetchEngine<IWorkEntry>> engineFactory,
        int count,
        string? workFilter,
        CancellationToken cancellationToken) =>
        ExecuteAsync(toolName, async () =>
        {
            runtime.EnsureLoggedIn();
            var filter = AnalyzeListWorkFilter(workFilter);
            if (filter is { IsSuccess: false })
                return PixevalMcpResult.Success(new PixevalWorkListDto(0, [], filter));

            return PixevalMcpResult.Success(await cursorStore.CreateWorkCursorAsync(
                    engineFactory(),
                    runtime,
                    count,
                    workFilter,
                    filter,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    private async Task<IReadOnlyList<WorkBase>> LoadWorksAsync(
        SimpleWorkType workType,
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken)
    {
        var works = new List<WorkBase>(int.Min(ids.Count, MaxCount));
        foreach (var id in ids.Distinct().Take(MaxCount))
        {
            cancellationToken.ThrowIfCancellationRequested();
            works.Add(await runtime.GetWorkAsync(workType, id, cancellationToken).ConfigureAwait(false));
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
