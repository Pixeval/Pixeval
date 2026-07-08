// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.Views;

namespace Pixeval.Models.Subscriptions;

public class WorkSubscriptionDownloadService(
    WorkSubscriptionPersistentManager subscriptionManager,
    HistoryPersistHelper historyPersistHelper,
    IllustrationDownloadTaskFactory illustrationDownloadTaskFactory,
    NovelDownloadTaskFactory novelDownloadTaskFactory,
    FileLogger logger)
{
    private const int DuplicateStopThreshold = 5;

    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public void QueueSyncAll() => _ = SyncAllAsync();

    public void QueueSyncCurrentSource(
        long uid,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind,
        IFetchEngine<IWorkEntry> engine)
    {
        if (!IsEngineUsable(engine)
            || TryGetSubscription(uid, subscriptionType, workKind) is not { } subscription)
            return;

        _ = SyncSubscriptionAsync(subscription, engine);
    }

    public WorkSubscriptionEntry? TryGetSubscription(
        long uid,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        subscriptionManager.GetBySubscriptionKey(uid, subscriptionType, workKind);

    private async Task SyncAllAsync()
    {
        if (!await _syncLock.WaitAsync(0))
            return;

        try
        {
            foreach (var subscription in subscriptionManager.ToArray())
                foreach (var engine in CreateEngines(subscription))
                    await SyncSubscriptionCoreAsync(subscription, engine, CreateKnownKeys());
        }
        catch (Exception e)
        {
            logger.LogError(nameof(WorkSubscriptionDownloadService), e);
        }
        finally
        {
            _ = _syncLock.Release();
        }
    }

    private async Task SyncSubscriptionAsync(
        WorkSubscriptionEntry subscription,
        IFetchEngine<IWorkEntry> engine)
    {
        if (!await _syncLock.WaitAsync(0))
            return;

        var wasCompleted = engine.EngineHandle.IsCompleted;
        try
        {
            await SyncSubscriptionCoreAsync(subscription, engine, CreateKnownKeys());
        }
        catch (Exception e)
        {
            logger.LogError(nameof(WorkSubscriptionDownloadService), e);
        }
        finally
        {
            if (!wasCompleted && !engine.EngineHandle.IsCancelled)
                engine.EngineHandle.IsCompleted = false;
            _ = _syncLock.Release();
        }
    }

    private async Task SyncSubscriptionCoreAsync(
        WorkSubscriptionEntry subscription,
        IFetchEngine<IWorkEntry> engine,
        HashSet<string> knownKeys)
    {
        if (!IsEngineUsable(engine))
            return;

        var duplicateCount = 0;

        await foreach (var entry in engine)
        {
            if (!IsEngineUsable(engine))
                return;

            TryUpdateSubscriptionName(subscription, entry);

            if (!TryGetKey(entry, out var key))
                continue;

            if (knownKeys.Contains(key))
            {
                if (++duplicateCount >= DuplicateStopThreshold)
                    return;
                continue;
            }

            var task = await CreateDownloadTaskAsync(entry, subscription);
            if (!IsEngineUsable(engine))
                return;

            if (await HasLocalFilesAsync(task))
            {
                _ = knownKeys.Add(key);
                if (++duplicateCount >= DuplicateStopThreshold)
                    return;
                continue;
            }
            if (!IsEngineUsable(engine))
                return;

            duplicateCount = 0;
            historyPersistHelper.DownloadManager.QueueTask(task);
            _ = knownKeys.Add(key);
        }
    }

    private static IEnumerable<IFetchEngine<IWorkEntry>> CreateEngines(WorkSubscriptionEntry subscription)
    {
        var makoClient = App.AppViewModel.MakoClient;
        return subscription.SubscriptionType switch
        {
            WorkSubscriptionType.Bookmarks => CreateBookmarkEngines(),
            WorkSubscriptionType.Posts => CreatePostEngines(),
            _ => []
        };

        IEnumerable<IFetchEngine<IWorkEntry>> CreateBookmarkEngines()
        {
            var workType = subscription.WorkKind switch
            {
                WorkSubscriptionWorkKind.Illustration => SimpleWorkType.Illustration,
                WorkSubscriptionWorkKind.Novel => SimpleWorkType.Novel,
                _ => (SimpleWorkType?) null
            };
            if (workType is not { } type)
                yield break;

            yield return makoClient.WorkBookmarks(subscription.UserId, type, PrivacyPolicy.Public, null);
            if (subscription.UserId == PixevalSettings.MyId)
                yield return makoClient.WorkBookmarks(subscription.UserId, type, PrivacyPolicy.Private, null);
        }

        IEnumerable<IFetchEngine<IWorkEntry>> CreatePostEngines()
        {
            var workType = subscription.WorkKind switch
            {
                WorkSubscriptionWorkKind.Illustration => WorkType.Illustration,
                WorkSubscriptionWorkKind.Manga => WorkType.Manga,
                WorkSubscriptionWorkKind.Novel => WorkType.Novel,
                _ => (WorkType?) null
            };
            if (workType is { } type)
                yield return makoClient.WorkPosts(subscription.UserId, type);
        }
    }

    private HashSet<string> CreateKnownKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var task in historyPersistHelper.DownloadManager.QueuedTasks.OfType<IDownloadTaskGroup>())
            if (TryGetKey(task.DatabaseEntry, out var key))
                _ = keys.Add(key);

        return keys;
    }

    private async Task<IDownloadTaskGroup> CreateDownloadTaskAsync(IArtworkInfo entry, WorkSubscriptionEntry subscription)
    {
        var parserContext = new ParserContext(
            entry,
            WorkSubscriptionDownloadContext.FromSubscriptionType(subscription.SubscriptionType));
        var task = entry is Novel
            ? novelDownloadTaskFactory.Create(parserContext, App.AppViewModel.AppSettings.DownloadSettings.DownloadPathMacro, null)
            : await CreateIllustrationDownloadTaskAsync(parserContext);

        task.DatabaseEntry.WorkSubscriptionId = subscription.HistoryEntryId;
        return task;

        async Task<IDownloadTaskGroup> CreateIllustrationDownloadTaskAsync(ParserContext context)
        {
            if (entry is ISingleAnimatedImage
                {
                    ImageType: ImageType.SingleAnimatedImage,
                    MultiImageUris: not null
                } animatedImage)
            {
                await animatedImage.MultiImageUris.TryPreloadListAsync(animatedImage);
            }

            return illustrationDownloadTaskFactory.Create(
                context,
                App.AppViewModel.AppSettings.DownloadSettings.DownloadPathMacro);
        }
    }

    private void TryUpdateSubscriptionName(WorkSubscriptionEntry subscription, IWorkEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(subscription.Name)
            || string.IsNullOrWhiteSpace(entry.User.Name))
            return;

        subscription.Name = entry.User.Name;
        subscriptionManager.Update(subscription);
    }

    private static bool IsEngineUsable(IFetchEngine<IWorkEntry> engine) =>
        engine.EngineHandle is { IsCancelled: false, IsCompleted: false };

    private static async Task<bool> HasLocalFilesAsync(IDownloadTaskGroup task)
    {
        if (task is NovelDownloadTaskGroup)
            return File.Exists(task.OpenLocalDestination);

        await task.InitializeTaskGroupAsync();
        if (File.Exists(task.OpenLocalDestination))
            return true;

        return task.Count is not 0 && task.All(t => File.Exists(t.Destination));
    }

    private static bool TryGetKey(DownloadHistoryEntry entry, out string key)
    {
        key = "";
        if (string.IsNullOrWhiteSpace(entry.SerializeKey)
            || string.IsNullOrWhiteSpace(entry.Entry.Id))
            return false;

        key = $"{entry.SerializeKey}:{entry.Entry.Id}";
        return true;
    }

    private static bool TryGetKey(IArtworkInfo entry, out string key)
    {
        key = "";
        if (entry is not ISerializable { SerializeKey: { } serializeKey }
            || string.IsNullOrWhiteSpace(entry.Id))
            return false;

        key = $"{serializeKey}:{entry.Id}";
        return true;
    }
}
