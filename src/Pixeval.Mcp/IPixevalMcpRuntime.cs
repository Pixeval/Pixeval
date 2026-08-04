// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Mako.Net.Responses;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Mcp;

public interface IPixevalMcpRuntime
{
    string AppVersion { get; }

    string TargetFilter { get; }

    TokenUser? CurrentUser { get; }

    MakoClient MakoClient { get; }

    HttpClient ImageHttpClient { get; }

    ushort Port { get; }

    bool EnableWriteTools { get; }

    int MaxBinaryResourceMegabytes { get; }

    void EnsureLoggedIn();

    void EnsureWriteToolsEnabled();

    PixevalHelpDto Help(PixevalHelpTopic? topic);

    PixevalDownloadMacroSettingsDto DownloadMacro();

    PixevalDownloadMacroAnalysisDto AnalyzeDownloadMacro(string text);

    PixevalSetDownloadMacroResultDto SetDownloadMacro(string text);

    PixevalWorkFilterAnalysisDto AnalyzeWorkFilter(string? text, int caretPosition);

    IReadOnlyList<WorkBase> FilterWorks(IReadOnlyList<WorkBase> works, string? text);

    Task<PixevalHistoryListDto> HistoryAsync(
        PixevalHistoryType type,
        int skip,
        int count,
        string? keyword,
        string? workFilter,
        CancellationToken token);

    PixevalExtensionListDto Extensions();

    PixevalSettingsSummaryDto SettingsSummary();

    void CacheWorks(IEnumerable<WorkBase> works);

    void CacheUsers(IEnumerable<User> users);

    void CacheUserInfos(IEnumerable<UserBasicInfo> users);

    Task<WorkBase> GetWorkAsync(
        SimpleWorkType workType,
        long id,
        CancellationToken token);

    Task<Illustration> GetIllustrationAsync(
        long id,
        CancellationToken token);

    Task<Novel> GetNovelAsync(
        long id,
        CancellationToken token);

    Task<SingleUserResponse> GetUserAsync(
        long id,
        CancellationToken token);

    Task<UserBasicInfo> GetUserBasicInfoAsync(
        long id,
        CancellationToken token);

    Task<PixevalNovelContentDto> NovelContentAsync(
        long id,
        bool includeMarkdown,
        CancellationToken token);

    Task<PixevalSauceNaoSearchDto> SauceNaoSearchAsync(
        string? imageBase64,
        string? imageUrl,
        long? illustrationId,
        int page,
        int count,
        string? index,
        double minSimilarity,
        bool loadPixivWorks,
        CancellationToken token);

    Task<PixevalMcpDownloadTaskDto> QueueDownloadAsync(
        SimpleWorkType workType,
        long id,
        CancellationToken token);

    IReadOnlyList<PixevalMcpDownloadTaskDto> DownloadTasks();

    Task<PixevalBookmarkResultDto> SetBookmarkAsync(
        SimpleWorkType workType,
        long id,
        bool bookmarked,
        PrivacyPolicy privacy,
        IReadOnlyList<string>? tags,
        CancellationToken token);

    Task<PixevalWatchLaterResultDto> SetWatchLaterAsync(
        SimpleWorkType workType,
        long id,
        bool watchLater,
        CancellationToken token);

    Task<PixevalFollowUserResultDto> FollowUserAsync(
        long userId,
        bool followed,
        PrivacyPolicy privacy,
        CancellationToken token);

    PixevalDownloadTaskControlResultDto ControlDownload(
        int? queueIndex,
        string? destination,
        PixevalDownloadAction action,
        bool deleteLocalFiles);

    Task<PixevalWorkSubscriptionOperationResultDto> AddSubscriptionAsync(
        long targetId,
        PixevalWorkSubscriptionType subscriptionType,
        PixevalWorkSubscriptionWorkKind workKind,
        CancellationToken token);

    Task<PixevalWorkSubscriptionOperationResultDto> RemoveSubscriptionAsync(
        int? historyEntryId,
        long? targetId,
        PixevalWorkSubscriptionType? subscriptionType,
        PixevalWorkSubscriptionWorkKind? workKind,
        CancellationToken token);

    PixevalOperationResultDto SyncSubscriptions();

    void LogToolException(string toolName, Exception exception);
}

public sealed class PixevalMcpException(string message) : Exception(message);
