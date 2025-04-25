// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Mako.Model;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Util.Threading;
using Pixeval.Utilities;
using WinUI3Utilities;
using Misaki;

namespace Pixeval.Download.Models;

public abstract partial class DownloadTaskGroup(DownloadHistoryEntry entry) : ObservableObject, IDownloadTaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; } = entry;

    public abstract ValueTask InitializeTaskGroupAsync();

    public string Id => DatabaseEntry.Entry.Id;

    protected DownloadTaskGroup(IArtworkInfo entry, string destination, DownloadItemType type) : this(new(destination, type, entry)) => SetNotCreateFromEntry();

    /// <summary>
    /// 将<see cref="IsCreateFromEntry"/>设置为<see langword="false"/>以便启动。<br/>
    /// 具体逻辑查看<see cref="IsCreateFromEntry"/>文档。
    /// </summary>
    protected void SetNotCreateFromEntry()
    {
        if (!IsCreateFromEntry)
            return;
        IsCreateFromEntry = false;
        PropertyChanged += (sender, e) =>
        {
            var g = sender.To<DownloadTaskGroup>();
            if (e.PropertyName is not nameof(CurrentState))
                return;
            // 子任务状态变化时，不一定需要任务组状态变化，也会进入此分支
            OnPropertyChanged(nameof(ActiveCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(ErrorCount));
            if (g.CurrentState is DownloadState.Running or DownloadState.Paused or DownloadState.Pending)
                return;
            g.DatabaseEntry.State = g.CurrentState;
            var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
            manager.Update(g.DatabaseEntry);
        };
        // 外部包裹了Task.Run
        AfterItemDownloadAsync += (_, _) => AllTasksDownloadedAsync();
        AfterAllDownloadAsync += AfterAllDownloadAsyncOverride;
    }

    protected async Task AllTasksDownloadedAsync()
    {
        if (IsAllCompleted && AfterAllDownloadAsync is not null)
        {
            await ThreadingHelper.DispatchAsync(() => IsPending = true);
            try
            {
                await AfterAllDownloadAsync.Invoke(this, CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            await ThreadingHelper.DispatchAsync(() => IsPending = false);
        }
    }

    /// <summary>
    /// 指示是否从<see cref="IWorkEntry"/>，而不是<see cref="EntryViewModel{T}"/>创建。<br/>
    /// 即从数据库加载的、并且没有启动过的任务组为<see langword="true"/>。<br/>
    /// 一旦启动过，此值将被设置为<see langword="false"/>。<br/>
    /// <br/>
    /// 此值相当于IsInitialized的作用，指示启动本下载任务的必要字段、事件订阅等是否已经被初始化。
    /// </summary>
    private bool IsCreateFromEntry { get; set; } = true;

    protected abstract Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default);

    /// <inheritdoc cref="DownloadHistoryEntry.Destination"/>
    public string TokenizedDestination => DatabaseEntry.Destination;

    public int Count => TasksSet.Count;

    public IReadOnlyList<string> Destinations => [.. TasksSet.Select(t => t.Destination)];

    protected IReadOnlyList<ImageDownloadTask> TasksSet => _tasksSet;

    private readonly List<ImageDownloadTask> _tasksSet = [];

    string IDownloadTaskBase.Destination => DatabaseEntry.Destination;

    public abstract string OpenLocalDestination { get; }

    public void TryReset()
    {
        if (CurrentState is not (DownloadState.Completed or DownloadState.Error or DownloadState.Cancelled))
            return;
        IsProcessing = true;
        (CurrentState is DownloadState.Error
            ? TasksSet.Where(t => t.CurrentState is DownloadState.Error)
            : TasksSet)
            .ForEach(t => t.TryReset());
        if (CancellationTokenSource.IsCancellationRequested)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = new();
        }
        DownloadTryReset?.Invoke(this);
        IsProcessing = false;
    }

    public void Pause()
    {
        IsProcessing = true;
        CancellationTokenSource.TryCancel();
        TasksSet.ForEach(t => t.Pause());
        IsProcessing = false;
    }

    public void TryResume()
    {
        IsProcessing = true;
        if (CancellationTokenSource.IsCancellationRequested)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = new();
        }
        TasksSet.ForEach(t => t.TryResume());
        DownloadTryResume?.Invoke(this);
        IsProcessing = false;
    }

    /// <summary>
    /// 因为<see cref="DownloadState.Pending"/>只能变为<see cref="DownloadState.Cancelled"/>而不能变为<see cref="DownloadState.Paused"/>，所以只需要在此使用
    /// </summary>
    public void Cancel()
    {
        if (CurrentState is not (DownloadState.Paused or DownloadState.Pending or DownloadState.Running or DownloadState.Queued))
            return;
        IsProcessing = true;
        TasksSet.ForEach(t => t.Cancel());
        CancellationTokenSource.TryCancel();
        IsProcessing = false;
    }

    public abstract void Delete();

    public DownloadToken GetToken() => new(this, CancellationTokenSource.Token);

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    /// <summary>
    /// 本下载组是否处于<see cref="DownloadState.Pending"/>状态
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentState))]
    public partial bool IsPending { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    public DownloadState CurrentState
    {
        get
        {
            if (IsCreateFromEntry)
                return DatabaseEntry.State;
            var isError = false;
            var isRunning = false;
            var isPaused = false;
            var isCancelled = false;
            var isQueued = false;
            var isPending = false;
            foreach (var task in TasksSet)
            {
                switch (task.CurrentState)
                {
                    case DownloadState.Queued:
                        isQueued = true;
                        break;
                    case DownloadState.Running:
                        isRunning = true;
                        break;
                    case DownloadState.Paused:
                        if (isCancelled)
                            ThrowHelper.ArgumentOutOfRange(CurrentState);
                        isPaused = true;
                        break;
                    case DownloadState.Cancelled:
                        if (isPaused)
                            ThrowHelper.ArgumentOutOfRange(CurrentState);
                        isCancelled = true;
                        break;
                    case DownloadState.Error:
                        isError = true;
                        break;
                    case DownloadState.Pending:
                        isPending = true;
                        break;
                }
            }

            return isError ? DownloadState.Error :
                isCancelled ? DownloadState.Cancelled :
                isPaused ? DownloadState.Paused :
                isRunning ? DownloadState.Running :
                isQueued ? DownloadState.Queued :
                isPending || IsPending ? DownloadState.Pending :
                DownloadState.Completed;
        }
    }

    public event Func<ImageDownloadTask, Task>? ItemDownloadStartedAsync;

    public event Func<ImageDownloadTask, Task>? ItemDownloadStoppedAsync;

    public event Func<ImageDownloadTask, Task>? ItemDownloadErrorAsync;

    public event Func<ImageDownloadTask, CancellationToken, Task>? AfterItemDownloadAsync;

    public event Func<DownloadTaskGroup, CancellationToken, Task>? AfterAllDownloadAsync;

    private event Action<DownloadTaskGroup>? DownloadTryResume;

    private event Action<DownloadTaskGroup>? DownloadTryReset;

    protected void AddToTasksSet(ImageDownloadTask task)
    {
        _tasksSet.Add(task);
        task.AfterDownloadAsync += (x, t) => AfterItemDownloadAsync?.Invoke(x, t) ?? Task.CompletedTask;
        task.DownloadStartedAsync += x => ItemDownloadStartedAsync?.Invoke(x) ?? Task.CompletedTask;
        task.DownloadStoppedAsync += x => ItemDownloadStoppedAsync?.Invoke(x) ?? Task.CompletedTask;
        task.DownloadErrorAsync += x => ItemDownloadErrorAsync?.Invoke(x) ?? Task.CompletedTask;
        task.PropertyChanged += (_, e) =>
        {
            OnPropertyChanged(e.PropertyName switch
            {
                nameof(ImageDownloadTask.ProgressPercentage) => nameof(ProgressPercentage),
                nameof(ImageDownloadTask.CurrentState) => nameof(CurrentState),
                nameof(ImageDownloadTask.ErrorCause) => nameof(ErrorCause),
                _ => ""
            });
        };
    }

    public void SubscribeProgress(ChannelWriter<DownloadToken> writer)
    {
        DownloadTryResume += OnDownloadWrite;
        DownloadTryReset += OnDownloadWrite;

        return;
        void OnDownloadWrite(DownloadTaskGroup o) => writer.TryWrite(o.GetToken());
    }

    public Exception? ErrorCause => TasksSet.FirstOrDefault(t => t.ErrorCause is not null)?.ErrorCause;

    public bool IsAllCompleted => !TasksSet.Any(t => t.CurrentState is not DownloadState.Completed);

    public bool IsAnyError => TasksSet.Any(t => t.CurrentState is DownloadState.Error);

    public int ActiveCount => TasksSet.Count(t => t.CurrentState is DownloadState.Queued or DownloadState.Running or DownloadState.Pending or DownloadState.Paused or DownloadState.Cancelled);

    public int CompletedCount => TasksSet.Count(t => t.CurrentState is DownloadState.Completed);

    public int ErrorCount => TasksSet.Count(t => t.CurrentState is DownloadState.Error);

    public double ProgressPercentage =>
        IsCreateFromEntry
            ? DatabaseEntry.State is DownloadState.Queued
                ? 0
                : 100
            : TasksSet.Count is 0
                ? 100
                : TasksSet.Average(t => t.ProgressPercentage);

    public IEnumerator<ImageDownloadTask> GetEnumerator() => TasksSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var task in TasksSet)
            task.Dispose();
    }
}
