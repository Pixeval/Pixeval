// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Download;
using Pixeval.Mcp;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;

namespace Pixeval.Utilities.McpServer;

public sealed partial class PixevalMcpService
{
    public async Task<PixevalBookmarkTagListDto> BookmarkTagsAsync(
        long? userId,
        SimpleWorkType workType,
        PrivacyPolicy privacy,
        CancellationToken cancellationToken)
    {
        var uid = userId ?? CurrentUser?.Id ??
            throw new PixevalMcpException("Current Pixiv user is not available.");
        var tags = await MakoHelper.GetBookmarkTagsAsync(uid, workType, privacy)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        var result = tags
            .Select(tag => PixevalBookmarkTagDto.FromBookmarkTag(tag, tag is AllBookmarkTag))
            .ToArray();
        return new(uid, workType.ToString(), privacy.ToString(), result.Length, result);
    }

    public async Task<PixevalBookmarkResultDto> SetBookmarkAsync(
        SimpleWorkType workType,
        long id,
        bool bookmarked,
        PrivacyPolicy privacy,
        IReadOnlyList<string>? tags,
        CancellationToken cancellationToken)
    {
        var normalizedTags = tags?
            .Where(static tag => !string.IsNullOrWhiteSpace(tag))
            .Select(static tag => tag.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var privately = privacy is PrivacyPolicy.Private;

        if (workType is SimpleWorkType.Novel)
        {
            var novel = await MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);
            var actual = await MakoHelper.SetNovelBookmarkAsync(novel, bookmarked, privately, normalizedTags)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            return new(
                actual == bookmarked,
                GetBookmarkMessage(actual, bookmarked),
                actual,
                PixevalWorkDto.FromNovel(novel));
        }

        var illustration = await MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        var isBookmarked = await MakoHelper.SetIllustrationBookmarkAsync(
                illustration,
                bookmarked,
                privately,
                normalizedTags)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        return new(
            isBookmarked == bookmarked,
            GetBookmarkMessage(isBookmarked, bookmarked),
            isBookmarked,
            PixevalWorkDto.FromIllustration(illustration));
    }

    public async Task<PixevalWatchLaterResultDto> SetWatchLaterAsync(
        SimpleWorkType workType,
        long id,
        bool watchLater,
        CancellationToken cancellationToken)
    {
        var work = await LoadWorkAsync(workType, id, cancellationToken).ConfigureAwait(false);
        if (work is not IArtworkInfo artwork)
            throw new PixevalMcpException("Pixiv returned a work shape that Pixeval cannot add to watch later.");

        var changed = RunOnUiThread(() => watchLater
            ? ViewModel.HistoryPersistHelper.AddWatchLater(artwork)
            : ViewModel.HistoryPersistHelper.RemoveWatchLater(artwork));
        var actual = RunOnUiThread(() => ViewModel.HistoryPersistHelper.ContainsWatchLater(artwork));
        return new(
            actual == watchLater,
            changed
                ? watchLater ? "Work added to Pixeval watch later." : "Work removed from Pixeval watch later."
                : watchLater
                    ? "Work was already in Pixeval watch later."
                    : "Work was not in Pixeval watch later.",
            actual,
            PixevalWorkDto.FromWork(work));
    }

    public async Task<PixevalFollowUserResultDto> FollowUserAsync(
        long userId,
        bool followed,
        PrivacyPolicy privacy,
        CancellationToken cancellationToken)
    {
        var user = await MakoClient.GetUserFromIdAsync(userId).WaitAsync(cancellationToken).ConfigureAwait(false);
        var success = await (followed
                ? MakoClient.PostFollowUserAsync(userId, privacy)
                : MakoClient.RemoveFollowUserAsync(userId))
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        if (success)
            user.UserEntity.IsFollowed = followed;
        var actual = user.UserEntity.IsFollowed;
        return new(
            actual == followed,
            actual == followed
                ? followed ? "User followed." : "User unfollowed."
                : "Pixiv did not apply the requested follow state.",
            actual,
            PixevalUserDto.FromSingleUserResponse(user));
    }

    public PixevalDownloadTaskControlResultDto ControlDownload(
        int? queueIndex,
        string? destination,
        PixevalDownloadAction action,
        bool deleteLocalFiles)
    {
        var manager = ViewModel.HistoryPersistHelper.DownloadManager;
        var (task, index) = FindDownloadTask(manager, queueIndex, destination);
        if (task is null)
            return new(false, "Download task was not found.", null);

        return RunOnUiThread(() =>
        {
            switch (action)
            {
                case PixevalDownloadAction.Pause:
                    task.Pause();
                    break;
                case PixevalDownloadAction.Resume:
                    task.Resume();
                    break;
                case PixevalDownloadAction.Reset:
                    task.Reset();
                    break;
                case PixevalDownloadAction.Cancel:
                    task.Cancel();
                    break;
                case PixevalDownloadAction.Remove:
                    if (deleteLocalFiles)
                        task.Delete();
                    manager.RemoveTask(task);
                    return new PixevalDownloadTaskControlResultDto(true, "Download task removed.", null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }

            var currentIndex = manager.QueuedTasks.IndexOf(task);
            return new PixevalDownloadTaskControlResultDto(
                true,
                $"Download task action '{action}' executed.",
                ToDownloadTaskDto(task, currentIndex < 0 ? index : currentIndex));
        });
    }

    public async Task<PixevalWorkSubscriptionOperationResultDto> AddSubscriptionAsync(
        long userId,
        PixevalWorkSubscriptionType subscriptionType,
        PixevalWorkSubscriptionWorkKind workKind,
        CancellationToken cancellationToken)
    {
        var type = ToWorkSubscriptionType(subscriptionType);
        var kind = ToWorkSubscriptionWorkKind(workKind);
        ValidateWorkSubscriptionKind(type, kind);

        var user = (await MakoClient.GetUserFromIdAsync(userId).WaitAsync(cancellationToken).ConfigureAwait(false))
            .UserEntity;
        if (!WorkSubscriptionHelper.TryAddOrUpdate(user, type, kind))
            return new(false, "Work subscription was not added.", null);

        var entry = ViewModel.AppServiceProvider.GetRequiredService<WorkSubscriptionPersistentManager>()
            .GetBySubscriptionKey(userId, type, kind);
        return new(true, "Work subscription added and sync was queued.",
            entry is { } e ? ToWorkSubscriptionDto(e) : null);
    }

    public PixevalWorkSubscriptionOperationResultDto RemoveSubscription(
        int? historyEntryId,
        long? userId,
        PixevalWorkSubscriptionType? subscriptionType,
        PixevalWorkSubscriptionWorkKind? workKind)
    {
        var manager = ViewModel.AppServiceProvider.GetRequiredService<WorkSubscriptionPersistentManager>();
        var entry = historyEntryId is { } id
            ? manager.GetByKey(id)
            : GetBySubscriptionKey();
        if (entry is null)
            return new(false, "Work subscription was not found.", null);

        var dto = ToWorkSubscriptionDto(entry);
        return manager.TryDelete(entry)
            ? new(true, "Work subscription removed.", dto)
            : new(false, "Work subscription was not removed.", dto);

        WorkSubscriptionEntry? GetBySubscriptionKey()
        {
            if (userId is not { } uid
                || subscriptionType is not { } mcpSubscriptionType
                || workKind is not { } mcpWorkKind)
                throw new PixevalMcpException(
                    "Provide historyEntryId, or provide userId, subscriptionType, and workKind.");

            var type = ToWorkSubscriptionType(mcpSubscriptionType);
            var kind = ToWorkSubscriptionWorkKind(mcpWorkKind);
            ValidateWorkSubscriptionKind(type, kind);
            return manager.GetBySubscriptionKey(uid, type, kind);
        }
    }

    public PixevalOperationResultDto SyncSubscriptions()
    {
        ViewModel.QueueWorkSubscriptionSyncAll();
        return new(true, "Pixeval work subscription sync was queued.");
    }

    private async Task<WorkBase> LoadWorkAsync(
        SimpleWorkType workType,
        long id,
        CancellationToken cancellationToken) =>
        workType is SimpleWorkType.Novel
            ? await MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false)
            : await MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);

    private (IDownloadTaskGroupBase? Task, int Index) FindDownloadTask(
        DownloadManager manager,
        int? queueIndex,
        string? destination)
    {
        if (queueIndex is { } index)
        {
            if (index < 0 || index >= manager.QueuedTasks.Count)
                return (null, -1);

            return (manager.QueuedTasks[index], index);
        }

        if (string.IsNullOrWhiteSpace(destination))
            throw new PixevalMcpException("Provide queueIndex or destination.");

        for (var i = 0; i < manager.QueuedTasks.Count; i++)
            if (string.Equals(manager.QueuedTasks[i].Destination, destination, StringComparison.Ordinal))
                return (manager.QueuedTasks[i], i);

        return (null, -1);
    }

    private static string GetBookmarkMessage(bool actual, bool requested) =>
        actual == requested
            ? requested ? "Work bookmarked." : "Work bookmark removed."
            : "Pixiv did not apply the requested bookmark state.";

    private static PixevalWorkSubscriptionDto ToWorkSubscriptionDto(WorkSubscriptionEntry entry) =>
        new(
            entry.HistoryEntryId,
            entry.UserId,
            entry.Name,
            entry.Account,
            entry.AvatarUrl,
            entry.SubscriptionType.ToString(),
            entry.WorkKind.ToString());

    private static SimpleWorkType ToSimpleWorkType(WorkType workType) =>
        workType is WorkType.Novel ? SimpleWorkType.Novel : SimpleWorkType.IllustrationAndManga;

    private static WorkSubscriptionType ToWorkSubscriptionType(PixevalWorkSubscriptionType subscriptionType) =>
        subscriptionType switch
        {
            PixevalWorkSubscriptionType.Bookmarks => WorkSubscriptionType.Bookmarks,
            PixevalWorkSubscriptionType.Posts => WorkSubscriptionType.Posts,
            _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType))
        };

    private static WorkSubscriptionWorkKind ToWorkSubscriptionWorkKind(PixevalWorkSubscriptionWorkKind workKind) =>
        workKind switch
        {
            PixevalWorkSubscriptionWorkKind.IllustrationAndManga => WorkSubscriptionWorkKind.IllustrationAndManga,
            PixevalWorkSubscriptionWorkKind.Illustration => WorkSubscriptionWorkKind.Illustration,
            PixevalWorkSubscriptionWorkKind.Manga => WorkSubscriptionWorkKind.Manga,
            PixevalWorkSubscriptionWorkKind.Novel => WorkSubscriptionWorkKind.Novel,
            _ => throw new ArgumentOutOfRangeException(nameof(workKind))
        };

    private static void ValidateWorkSubscriptionKind(
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind)
    {
        var isValid = subscriptionType switch
        {
            WorkSubscriptionType.Bookmarks => workKind is WorkSubscriptionWorkKind.IllustrationAndManga
                or WorkSubscriptionWorkKind.Novel,
            WorkSubscriptionType.Posts => workKind is WorkSubscriptionWorkKind.Illustration
                or WorkSubscriptionWorkKind.Manga
                or WorkSubscriptionWorkKind.Novel,
            _ => false
        };
        if (!isValid)
            throw new PixevalMcpException(
                "For bookmarks, workKind must be illustration_and_manga or novel. For posts, workKind must be illustration, manga, or novel.");
    }

    private static void RunOnUiThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Invoke(action);
    }

    private static T RunOnUiThread<T>(Func<T> action) =>
        Dispatcher.UIThread.CheckAccess()
            ? action()
            : Dispatcher.UIThread.Invoke(action);
}
