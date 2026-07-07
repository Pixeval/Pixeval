// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Mcp;

public interface IPixevalMcpRuntime
{
    string AppVersion { get; }

    string TargetFilter { get; }

    TokenUser? CurrentUser { get; }

    MakoClient MakoClient { get; }

    HttpClient ImageHttpClient { get; }

    int Port { get; }

    bool EnableWriteTools { get; }

    int MaxBinaryResourceMegabytes { get; }

    void EnsureLoggedIn();

    void EnsureWriteToolsEnabled();

    PixevalHelpDto Help(string? topic);

    PixevalDownloadMacroSettingsDto DownloadMacro();

    PixevalDownloadMacroAnalysisDto AnalyzeDownloadMacro(string? text);

    Task<PixevalDownloadMacroPreviewDto> PreviewDownloadMacroAsync(
        string? text,
        WorkType? workType,
        long? id,
        CancellationToken cancellationToken);

    PixevalSetDownloadMacroResultDto SetDownloadMacro(string text);

    PixevalWorkFilterAnalysisDto AnalyzeWorkFilter(string? text, int caretPosition);

    IReadOnlyList<WorkBase> FilterWorks(IReadOnlyList<WorkBase> works, string? text);

    Task<PixevalHistoryListDto> HistoryAsync(
        PixevalHistoryType type,
        int skip,
        int limit,
        string? keyword,
        string? workFilter,
        CancellationToken cancellationToken);

    PixevalExtensionListDto Extensions();

    PixevalSettingsSummaryDto SettingsSummary();

    Task<PixevalNovelContentDto> NovelContentAsync(
        long id,
        bool includeMarkdown,
        CancellationToken cancellationToken);

    Task<PixevalSauceNaoSearchDto> SauceNaoSearchAsync(
        string? imageBase64,
        string? imageUrl,
        long? illustrationId,
        int page,
        int limit,
        string? index,
        double minSimilarity,
        bool loadPixivWorks,
        CancellationToken cancellationToken);

    Task<PixevalBookmarkTagListDto> BookmarkTagsAsync(
        long? userId,
        SimpleWorkType workType,
        PrivacyPolicy privacy,
        CancellationToken cancellationToken);

    Task<PixevalMcpDownloadTaskDto> QueueDownloadAsync(
        SimpleWorkType workType,
        long id,
        CancellationToken cancellationToken);

    IReadOnlyList<PixevalMcpDownloadTaskDto> DownloadTasks();

    Task<PixevalBookmarkResultDto> SetBookmarkAsync(
        SimpleWorkType workType,
        long id,
        bool bookmarked,
        PrivacyPolicy privacy,
        IReadOnlyList<string>? tags,
        CancellationToken cancellationToken);

    Task<PixevalWatchLaterResultDto> SetWatchLaterAsync(
        SimpleWorkType workType,
        long id,
        bool watchLater,
        CancellationToken cancellationToken);

    Task<PixevalFollowUserResultDto> FollowUserAsync(
        long userId,
        bool followed,
        PrivacyPolicy privacy,
        CancellationToken cancellationToken);

    PixevalDownloadTaskControlResultDto ControlDownload(
        int? queueIndex,
        string? destination,
        PixevalDownloadAction action,
        bool deleteLocalFiles);

    Task<PixevalWorkSubscriptionOperationResultDto> AddSubscriptionAsync(
        long userId,
        PixevalWorkSubscriptionType subscriptionType,
        PixevalWorkSubscriptionWorkKind workKind,
        CancellationToken cancellationToken);

    PixevalWorkSubscriptionOperationResultDto RemoveSubscription(
        int? historyEntryId,
        long? userId,
        PixevalWorkSubscriptionType? subscriptionType,
        PixevalWorkSubscriptionWorkKind? workKind);

    PixevalOperationResultDto SyncSubscriptions();

    void LogToolException(string toolName, Exception exception);
}

public sealed class PixevalMcpException(string message) : Exception(message);
