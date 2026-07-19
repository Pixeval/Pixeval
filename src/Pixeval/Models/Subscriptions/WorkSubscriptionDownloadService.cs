// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
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
    SubscriptionDownloadHistoryPersistentManager subscriptionDownloadHistoryManager,
    HistoryPersistHelper historyPersistHelper,
    IllustrationDownloadTaskFactory illustrationDownloadTaskFactory,
    NovelDownloadTaskFactory novelDownloadTaskFactory,
    FileLogger logger)
{
    private const int DuplicateStopThreshold = 5;

    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public void QueueSyncAll() => _ = SyncAllAsync();

    public void QueueSyncSubscription(WorkSubscriptionEntry subscription) => _ = SyncSubscriptionAsync(subscription);

    public void QueueSyncCurrentSource(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind,
        IFetchEngine<IWorkEntry> engine)
    {
        if (!IsEngineUsable(engine)
            || TryGetSubscription(targetId, subscriptionType, workKind) is not { } subscription)
            return;

        _ = SyncSubscriptionAsync(subscription, engine);
    }

    public WorkSubscriptionEntry? TryGetSubscription(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        subscriptionManager.GetBySubscriptionKey(targetId, subscriptionType, workKind);

    private async Task SyncAllAsync()
    {
        if (!await _syncLock.WaitAsync(0))
            return;

        try
        {
            await foreach (var subscription in subscriptionManager.StreamEntriesAsync().ConfigureAwait(false))
            {
                var knownKeys = new HashSet<SubscriptionDownloadKey>();
                foreach (var engine in CreateEngines(subscription))
                    await SyncSubscriptionCoreAsync(subscription, engine, knownKeys).ConfigureAwait(false);
            }
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
            var knownKeys = new HashSet<SubscriptionDownloadKey>();
            await SyncSubscriptionCoreAsync(subscription, engine, knownKeys);
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

    private async Task SyncSubscriptionAsync(WorkSubscriptionEntry subscription)
    {
        if (!await _syncLock.WaitAsync(0))
            return;

        try
        {
            var knownKeys = new HashSet<SubscriptionDownloadKey>();
            foreach (var engine in CreateEngines(subscription))
                await SyncSubscriptionCoreAsync(subscription, engine, knownKeys).ConfigureAwait(false);
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

    private async Task SyncSubscriptionCoreAsync(
        WorkSubscriptionEntry subscription,
        IFetchEngine<IWorkEntry> engine,
        HashSet<SubscriptionDownloadKey> knownKeys)
    {
        if (!IsEngineUsable(engine))
            return;

        var duplicateCount = 0;

        await foreach (var entry in engine)
        {
            if (!IsEngineUsable(engine))
                return;

            TryUpdateSubscriptionName(subscription, entry);

            var task = await CreateDownloadTaskAsync(entry, subscription);
            if (!IsEngineUsable(engine))
            {
                task.Dispose();
                return;
            }

            if (task.DatabaseEntry is not SubscriptionDownloadHistoryEntry historyEntry)
            {
                task.Dispose();
                throw new InvalidOperationException("A subscription download must use subscription history.");
            }

            var key = new SubscriptionDownloadKey(historyEntry.ArtworkId, historyEntry.Destination);
            if (!knownKeys.Add(key)
                || subscriptionDownloadHistoryManager.ContainsIdentity(
                    historyEntry.WorkSubscriptionId,
                    historyEntry.ArtworkId,
                    historyEntry.Destination))
            {
                task.Dispose();
                if (++duplicateCount >= DuplicateStopThreshold)
                    return;
                continue;
            }

            if (await HasLocalFilesAsync(task))
            {
                task.Dispose();
                if (++duplicateCount >= DuplicateStopThreshold)
                    return;
                continue;
            }

            if (!IsEngineUsable(engine))
            {
                task.Dispose();
                return;
            }

            var isQueued = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!IsEngineUsable(engine))
                    return false;

                historyPersistHelper.DownloadManager.QueueTask(task);
                return true;
            });
            if (!isQueued)
            {
                task.Dispose();
                return;
            }

            duplicateCount = 0;
        }
    }

    private static IEnumerable<IFetchEngine<IWorkEntry>> CreateEngines(WorkSubscriptionEntry subscription)
    {
        var makoClient = App.AppViewModel.MakoClient;
        return subscription.SubscriptionType switch
        {
            WorkSubscriptionType.Bookmarks => CreateBookmarkEngines(),
            WorkSubscriptionType.Posts => CreatePostEngines(),
            WorkSubscriptionType.Series => CreateSeriesEngines(),
            _ => []
        };

        IEnumerable<IFetchEngine<IWorkEntry>> CreateBookmarkEngines()
        {
            var workType = subscription.WorkKind switch
            {
                WorkSubscriptionWorkKind.Illustration or WorkSubscriptionWorkKind.Manga => SimpleWorkType.Illustration,
                WorkSubscriptionWorkKind.Novel => SimpleWorkType.Novel,
                _ => (SimpleWorkType?) null
            };
            if (workType is not { } type)
                yield break;

            yield return makoClient.WorkBookmarks(type, subscription.Id, PrivacyPolicy.Public, null);
            if (subscription.Id == PixevalSettings.MyId)
                yield return makoClient.WorkBookmarks(type, subscription.Id, PrivacyPolicy.Private, null);
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
                yield return makoClient.WorkPosted(type, subscription.Id);
        }

        IEnumerable<IFetchEngine<IWorkEntry>> CreateSeriesEngines()
        {
            var workType = subscription.WorkKind switch
            {
                WorkSubscriptionWorkKind.Illustration or WorkSubscriptionWorkKind.Manga => SimpleWorkType.Illustration,
                WorkSubscriptionWorkKind.Novel => SimpleWorkType.Novel,
                _ => (SimpleWorkType?) null
            };
            if (workType is { } type)
                yield return makoClient.WorkSeries(type, subscription.Id);
        }
    }

    private async Task<IDownloadTaskGroup> CreateDownloadTaskAsync(IArtworkInfo entry, WorkSubscriptionEntry subscription)
    {
        var parserContext = new ParserContext(
            entry,
            subscription);
        var task = entry is Novel
            ? novelDownloadTaskFactory.Create(parserContext, App.AppViewModel.AppSettings.DownloadSettings.DownloadPathMacro, null)
            : await CreateIllustrationDownloadTaskAsync(parserContext);

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
        if (subscription.SubscriptionType is WorkSubscriptionType.Series
            || !string.IsNullOrWhiteSpace(subscription.Name)
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

    private readonly record struct SubscriptionDownloadKey(string ArtworkId, string Destination);
}
