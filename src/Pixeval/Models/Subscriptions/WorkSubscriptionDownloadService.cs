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

public sealed class WorkSubscriptionDownloadService(
    WorkSubscriptionPersistentManager subscriptionManager,
    SubscriptionDownloadHistoryPersistentManager subscriptionDownloadHistoryManager,
    HistoryPersistHelper historyPersistHelper,
    IllustrationDownloadTaskFactory illustrationDownloadTaskFactory,
    NovelDownloadTaskFactory novelDownloadTaskFactory,
    FileLogger logger) : IWorkSubscriptionService, IAsyncDisposable
{
    private const int DuplicateStopThreshold = 5;

    private readonly Lock _syncGate = new();
    private readonly SemaphoreSlim _subscriptionMutationGate = new(1, 1);
    private readonly WorkSubscriptionSyncRequestQueue _pendingSyncRequests = new();
    private readonly HashSet<int> _removedSubscriptionIds = [];
    private CancellationTokenSource? _activeSubscriptionCancellationTokenSource;
    private CancellationTokenSource? _syncCancellationTokenSource;
    private WorkSubscriptionFetchState? _currentFetchState;
    private int? _activeSubscriptionId;
    private WorkSubscriptionSyncRequest? _activeSyncRequest;
    private Task _syncTask = Task.CompletedTask;
    private bool _acceptsSyncRequests = true;
    private bool _isDisposed;

    public WorkSubscriptionFetchState? CurrentFetchState
    {
        get
        {
            lock (_syncGate)
                return _currentFetchState;
        }
    }

    public event EventHandler<WorkSubscriptionFetchState>? FetchStateChanged;

    public event EventHandler<WorkSubscriptionEntry>? SubscriptionUpdated;

    public event EventHandler<int>? SubscriptionRemoved;

    public bool IsSyncInProgress
    {
        get
        {
            lock (_syncGate)
                return !_syncTask.IsCompleted;
        }
    }

    public void QueueSyncAll() => QueueSyncRequest(WorkSubscriptionSyncRequest.All.Instance);

    public void QueueSyncSubscription(WorkSubscriptionEntry subscription) =>
        QueueSyncRequest(new WorkSubscriptionSyncRequest.Subscription(subscription, RefreshMetadata: true));

    public void QueueInitialSync(
        WorkSubscriptionEntry subscription,
        IFetchEngine<IWorkEntry>? sourceEngine = null) =>
        QueueSyncRequest(new WorkSubscriptionSyncRequest.Subscription(subscription, sourceEngine));

    public void QueueSyncCurrentSource(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind,
        IFetchEngine<IWorkEntry> engine)
    {
        if (!IsEngineUsable(engine)
            || TryGetSubscription(targetId, subscriptionType, workKind) is not { } subscription)
            return;

        QueueSyncRequest(new WorkSubscriptionSyncRequest.Subscription(
            subscription,
            engine,
            UsesSharedSourceEngine: true));
    }

    public WorkSubscriptionEntry? TryGetSubscription(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        subscriptionManager.GetBySubscriptionKey(targetId, subscriptionType, workKind);

    public async Task<WorkSubscriptionEntry?> TryRemoveAsync(int historyEntryId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(historyEntryId);
        WorkSubscriptionEntry? subscription;
        lock (_syncGate)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            subscription = subscriptionManager.GetByKey(historyEntryId);
            if (subscription is null || !_removedSubscriptionIds.Add(historyEntryId))
                return null;

            _pendingSyncRequests.RemoveSubscription(historyEntryId);
            if (_activeSubscriptionId == historyEntryId)
                _activeSubscriptionCancellationTokenSource?.Cancel();
        }

        var wasDeleted = false;
        await _subscriptionMutationGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (subscriptionManager.GetByKey(historyEntryId) is not { } persistedSubscription
                || !subscriptionManager.TryDelete(persistedSubscription))
                return null;

            subscription = persistedSubscription;
            wasDeleted = true;
            await historyPersistHelper.RemoveWorkSubscriptionDownloadsAsync(historyEntryId)
                .ConfigureAwait(false);
            return subscription;
        }
        finally
        {
            _ = _subscriptionMutationGate.Release();
            if (wasDeleted)
                NotifySubscriptionRemoved(historyEntryId);
            else
                lock (_syncGate)
                    _ = _removedSubscriptionIds.Remove(historyEntryId);
        }
    }

    public async Task CancelAndWaitAsync()
    {
        CancellationTokenSource? cancellationTokenSource;
        Task syncTask;
        lock (_syncGate)
        {
            _acceptsSyncRequests = false;
            _pendingSyncRequests.Clear();
            cancellationTokenSource = _syncCancellationTokenSource;
            syncTask = _syncTask;
        }

        cancellationTokenSource?.Cancel();
        await syncTask.ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        lock (_syncGate)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
        }

        await CancelAndWaitAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void QueueSyncRequest(WorkSubscriptionSyncRequest request)
    {
        SyncWorker? worker = null;
        lock (_syncGate)
        {
            if (_isDisposed
                || !_acceptsSyncRequests
                || (request is WorkSubscriptionSyncRequest.Subscription
                {
                    Entry.HistoryEntryId: var subscriptionId
                } && _removedSubscriptionIds.Contains(subscriptionId))
                || !_pendingSyncRequests.TryEnqueue(request, _activeSyncRequest))
                return;

            if (_syncTask.IsCompleted)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _syncCancellationTokenSource = cancellationTokenSource;
                _syncTask = completionSource.Task;
                worker = new(cancellationTokenSource, completionSource);
            }
        }

        if (worker is { } syncWorker)
            _ = RunSyncQueueAsync(syncWorker);
    }

    private async Task RunSyncQueueAsync(SyncWorker worker)
    {
        try
        {
            while (TryTakeNextRequest(worker, out var request))
            {
                try
                {
                    await ExecuteSyncRequestAsync(request, worker.CancellationTokenSource.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    logger.LogError(nameof(WorkSubscriptionDownloadService), exception);
                }
                finally
                {
                    lock (_syncGate)
                        if (ReferenceEquals(_activeSyncRequest, request))
                            _activeSyncRequest = null;
                }
            }
        }
        finally
        {
            lock (_syncGate)
                CompleteWorker(worker);
            worker.CancellationTokenSource.Dispose();
        }
    }

    private bool TryTakeNextRequest(
        SyncWorker worker,
        out WorkSubscriptionSyncRequest request)
    {
        lock (_syncGate)
        {
            if (worker.CancellationTokenSource.IsCancellationRequested
                || !_pendingSyncRequests.TryDequeue(out var nextRequest))
            {
                CompleteWorker(worker);
                request = null!;
                return false;
            }

            request = _activeSyncRequest = nextRequest;
            return true;
        }
    }

    private void CompleteWorker(SyncWorker worker)
    {
        if (!ReferenceEquals(_syncCancellationTokenSource, worker.CancellationTokenSource))
        {
            _ = worker.CompletionSource.TrySetResult();
            return;
        }

        _activeSyncRequest = null;
        _syncCancellationTokenSource = null;
        _ = worker.CompletionSource.TrySetResult();
    }

    private Task ExecuteSyncRequestAsync(WorkSubscriptionSyncRequest request, CancellationToken token) =>
        request switch
        {
            WorkSubscriptionSyncRequest.All => SyncAllAsync(token),
            WorkSubscriptionSyncRequest.Subscription subscriptionRequest =>
                SyncQueuedSubscriptionAsync(subscriptionRequest, token),
            _ => throw new ArgumentOutOfRangeException(nameof(request))
        };

    private Task SyncQueuedSubscriptionAsync(
        WorkSubscriptionSyncRequest.Subscription request,
        CancellationToken token)
    {
        if (subscriptionManager.GetByKey(request.Entry.HistoryEntryId) is not { } subscription)
            return Task.CompletedTask;

        return request.SourceEngine is { } engine && IsEngineUsable(engine)
            ? SyncSubscriptionAsync(
                subscription,
                [engine],
                request.UsesSharedSourceEngine,
                request.RefreshMetadata,
                token)
            : SyncSubscriptionAsync(
                subscription,
                CreateEngines(subscription),
                false,
                request.RefreshMetadata,
                token);
    }

    private async Task SyncAllAsync(CancellationToken token)
    {
        await foreach (var subscription in subscriptionManager
                           .StreamEntriesAsync(token: token)
                           .ConfigureAwait(false))
        {
            try
            {
                await SyncSubscriptionAsync(subscription, CreateEngines(subscription), false, false, token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when 
                (!token.IsCancellationRequested
                 && IsSubscriptionRemoved(subscription.HistoryEntryId))
            {
            }
        }
    }

    private async Task SyncSubscriptionAsync(
        WorkSubscriptionEntry subscription,
        IEnumerable<IFetchEngine<IWorkEntry>> engines,
        bool restoreEngineCompletion,
        bool refreshMetadata,
        CancellationToken token)
    {
        using var subscriptionCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (!TryBeginSubscriptionSync(subscription.HistoryEntryId, subscriptionCancellationTokenSource))
            return;

        token = subscriptionCancellationTokenSource.Token;
        var stagedTasks = new List<IDownloadTaskGroup>();
        var knownKeys = new HashSet<SubscriptionDownloadKey>();
        var fetchedCount = 0;
        IWorkEntry? subscriptionMetadataSource = null;
        SetFetchState(subscription.HistoryEntryId, true, fetchedCount);
        try
        {
            if (refreshMetadata
                && await RefreshSubscriptionMetadataAsync(subscription, token).ConfigureAwait(false) is { } metadataEngine)
            {
                engines = [metadataEngine];
                restoreEngineCompletion = false;
            }

            foreach (var engine in engines)
            {
                token.ThrowIfCancellationRequested();
                if (!IsEngineUsable(engine))
                    continue;

                var wasCompleted = engine.EngineHandle.IsCompleted;
                try
                {
                    var engineMetadataSource = await StageSubscriptionDownloadsAsync(
                            subscription,
                            engine,
                            knownKeys,
                            stagedTasks,
                            () => SetFetchState(subscription.HistoryEntryId, true, ++fetchedCount),
                            !restoreEngineCompletion,
                            token)
                        .ConfigureAwait(false);
                    subscriptionMetadataSource ??= engineMetadataSource;
                }
                finally
                {
                    if (restoreEngineCompletion
                        && !wasCompleted
                        && !engine.EngineHandle.IsCancelled)
                        engine.EngineHandle.IsCompleted = false;
                }
            }

            await _subscriptionMutationGate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (IsSubscriptionRemoved(subscription.HistoryEntryId)
                    || subscriptionManager.GetByKey(subscription.HistoryEntryId) is null)
                    return;

                if (stagedTasks.Count is not 0)
                {
                    var committedTasks = stagedTasks.ToArray();
                    stagedTasks.Clear();
                    await historyPersistHelper.QueueSubscriptionDownloadBatchAsync(committedTasks).ConfigureAwait(false);
                }

                if (subscriptionMetadataSource is not null)
                    TryUpdateSubscriptionName(subscription, subscriptionMetadataSource);
            }
            finally
            {
                _ = _subscriptionMutationGate.Release();
            }
        }
        finally
        {
            SetFetchState(subscription.HistoryEntryId, false, fetchedCount);
            foreach (var stagedTask in stagedTasks)
                stagedTask.Dispose();
            EndSubscriptionSync(subscription.HistoryEntryId, subscriptionCancellationTokenSource);
        }
    }

    private async Task<IWorkEntry?> StageSubscriptionDownloadsAsync(
        WorkSubscriptionEntry subscription,
        IFetchEngine<IWorkEntry> engine,
        HashSet<SubscriptionDownloadKey> knownKeys,
        List<IDownloadTaskGroup> stagedTasks,
        Action reportEntryFetched,
        bool cancelEngineOnCancellation,
        CancellationToken token)
    {
        var duplicateCount = 0;
        IWorkEntry? firstEntry = null;

        await foreach (var entry in FetchEngineRetryHelper
                           .StreamAsync(
                               engine,
                               cancelEngineOnCancellation: cancelEngineOnCancellation,
                               token: token)
                           .ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            reportEntryFetched();
            firstEntry ??= entry;

            var task = await CreateDownloadTaskAsync(entry, subscription).ConfigureAwait(false);
            if (token.IsCancellationRequested || engine.EngineHandle.IsCancelled)
            {
                task.Dispose();
                token.ThrowIfCancellationRequested();
                throw new OperationCanceledException("The fetch engine was cancelled.");
            }

            if (task.DatabaseEntry is not SubscriptionDownloadHistoryEntry historyEntry)
            {
                task.Dispose();
                throw new InvalidOperationException("A subscription download must use subscription history.");
            }

            var key = new SubscriptionDownloadKey(historyEntry.ArtworkId, historyEntry.Destination);
            if (!knownKeys.Add(key))
            {
                task.Dispose();
                continue;
            }

            if (subscriptionDownloadHistoryManager.ContainsIdentity(
                    historyEntry.WorkSubscriptionId,
                    historyEntry.ArtworkId,
                    historyEntry.Destination)
                || await HasLocalFilesAsync(task).ConfigureAwait(false))
            {
                task.Dispose();
                if (++duplicateCount >= DuplicateStopThreshold)
                    return firstEntry;
                continue;
            }

            if (token.IsCancellationRequested || engine.EngineHandle.IsCancelled)
            {
                task.Dispose();
                token.ThrowIfCancellationRequested();
                throw new OperationCanceledException("The fetch engine was cancelled.");
            }

            stagedTasks.Add(task);
            duplicateCount = 0;
        }

        return firstEntry;
    }

    private void SetFetchState(int workSubscriptionId, bool isFetching, int fetchedCount)
    {
        var state = new WorkSubscriptionFetchState(workSubscriptionId, isFetching, fetchedCount);
        lock (_syncGate)
            _currentFetchState = isFetching ? state : null;
        try
        {
            FetchStateChanged?.Invoke(this, state);
        }
        catch (Exception exception)
        {
            logger.LogError(nameof(SetFetchState), exception);
        }
    }

    private bool TryBeginSubscriptionSync(
        int workSubscriptionId,
        CancellationTokenSource cancellationTokenSource)
    {
        lock (_syncGate)
        {
            if (_removedSubscriptionIds.Contains(workSubscriptionId))
                return false;

            _activeSubscriptionId = workSubscriptionId;
            _activeSubscriptionCancellationTokenSource = cancellationTokenSource;
            return true;
        }
    }

    private void EndSubscriptionSync(
        int workSubscriptionId,
        CancellationTokenSource cancellationTokenSource)
    {
        lock (_syncGate)
        {
            if (_activeSubscriptionId == workSubscriptionId
                && ReferenceEquals(_activeSubscriptionCancellationTokenSource, cancellationTokenSource))
            {
                _activeSubscriptionId = null;
                _activeSubscriptionCancellationTokenSource = null;
            }
        }
    }

    private bool IsSubscriptionRemoved(int workSubscriptionId)
    {
        lock (_syncGate)
            return _removedSubscriptionIds.Contains(workSubscriptionId);
    }

    private void NotifySubscriptionRemoved(int workSubscriptionId)
    {
        try
        {
            SubscriptionRemoved?.Invoke(this, workSubscriptionId);
        }
        catch (Exception exception)
        {
            logger.LogError(nameof(NotifySubscriptionRemoved), exception);
        }
    }

    private void NotifySubscriptionUpdated(WorkSubscriptionEntry subscription)
    {
        try
        {
            SubscriptionUpdated?.Invoke(this, subscription);
        }
        catch (Exception exception)
        {
            logger.LogError(nameof(NotifySubscriptionUpdated), exception);
        }
    }

    private async Task<IFetchEngine<IWorkEntry>?> RefreshSubscriptionMetadataAsync(
        WorkSubscriptionEntry subscription,
        CancellationToken token)
    {
        var makoClient = App.AppViewModel.MakoClient;
        IFetchEngine<IWorkEntry>? seriesEngine = null;
        switch (subscription.SubscriptionType)
        {
            case WorkSubscriptionType.Bookmarks:
            case WorkSubscriptionType.Posts:
                var userResponse = await FetchEngineRetryHelper.ExecuteAsync(
                        t => makoClient.GetUserFromIdAsync(subscription.Id, t),
                        token: token)
                    .ConfigureAwait(false);
                subscription.UpdateUserMetadata(userResponse.UserEntity);
                break;
            case WorkSubscriptionType.Series:
                var simpleWorkType = subscription.WorkKind is WorkSubscriptionWorkKind.Novel
                    ? SimpleWorkType.Novel
                    : SimpleWorkType.Illustration;
                var series = await FetchEngineRetryHelper.ExecuteAsync(
                        t => makoClient.GetWorkSeriesAsync(
                            simpleWorkType,
                            subscription.Id,
                            t),
                        token: token)
                    .ConfigureAwait(false);
                subscription.UpdateSeriesMetadata(series.Detail, series.First);
                seriesEngine = series.Engine;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(subscription.SubscriptionType));
        }

        subscriptionManager.Update(subscription);
        NotifySubscriptionUpdated(subscription);
        return seriesEngine;
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
        NotifySubscriptionUpdated(subscription);
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

    private sealed record SyncWorker(
        CancellationTokenSource CancellationTokenSource,
        TaskCompletionSource CompletionSource);

    private readonly record struct SubscriptionDownloadKey(string ArtworkId, string Destination);
}
