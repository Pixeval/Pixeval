// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pixeval.CoreApi.Net;
using Pixeval.Download.Models;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download;

public partial class DownloadManager : IDisposable
{
    /// <summary>
    /// 正在排队的任务队列
    /// </summary>
    private readonly Channel<DownloadToken> _downloadTaskChannel = Channel.CreateUnbounded<DownloadToken>();

    /// <summary>
    /// 使用<see cref="MakoApiKind.ImageApi"/>的<see cref="HttpClient"/>
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 指示是否可以进行新的下载任务，要求<see cref="_workingTasks"/>小于<see cref="ConcurrencyDegree"/>
    /// </summary>
    /// <remarks>
    /// the generic parameter is used to simplify the code, the expression can be embedded directly into the condition of the while loop
    /// </remarks>
    private readonly ReenterableAwaiter<bool> _throttle = new(true, true);

    /// <summary>
    /// 正在下载中的数量
    /// </summary>
    private int _workingTasks;

    /// <summary>
    /// 与<see cref="QueuedTasks"/>内容一样，但是用于快速查询
    /// </summary>
    private readonly Dictionary<string, IDownloadTaskGroup> _taskQuerySet = [];

    /// <summary>
    /// 所有的下载任务
    /// </summary>
    public ObservableCollection<IDownloadTaskGroup> QueuedTasks { get; } = [];

    /// <summary>
    /// 同时下载的任务数量
    /// </summary>
    public int ConcurrencyDegree { get; set; }

    public DownloadManager(HttpClient httpClient, int concurrencyDegree)
    {
        _httpClient = httpClient;
        ConcurrencyDegree = concurrencyDegree;

        PollTask();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _downloadTaskChannel.Writer.Complete();
        _throttle.Dispose();
    }

    /// <summary>
    /// 保证不重复的前提下，将任务加入下载队列前端
    /// </summary>
    /// <remarks>
    /// intrinsic download task are not counted
    /// </remarks>
    public void QueueTask(IDownloadTaskGroup taskGroup)
    {
        if (_taskQuerySet.TryGetValue(taskGroup.Destination, out var v) && v == taskGroup)
            return;
        _taskQuerySet[taskGroup.Destination] = taskGroup;

        if (v is not null)
            _ = QueuedTasks.Remove(v);
        QueuedTasks.Insert(0, taskGroup);
        taskGroup.SubscribeProgress(_downloadTaskChannel.Writer);
        _ = _downloadTaskChannel.Writer.TryWrite(taskGroup.GetToken());
    }

    /// <summary>
    /// 当队列有排队且未到达最大同时下载数时，开始下载任务
    /// </summary>
    private async void PollTask()
    {
        while (await _downloadTaskChannel.Reader.WaitToReadAsync())
            while (await _throttle && _downloadTaskChannel.Reader.TryRead(out var taskToken) && taskToken is { Token.IsCancellationRequested: false, Task: var taskGroup })
            {
                await taskGroup.InitializeTaskGroupAsync();

                foreach (var subTask in taskGroup)
                {
                    // 需要判断是否处于Queued状态才能运行
                    if (subTask.CurrentState is DownloadState.Queued)
                    {
                        await DownloadAsync(subTask);
                        _ = await _throttle;
                    }
                }
            }
    }

    /// <summary>
    /// 清除指定任务
    /// </summary>
    /// <param name="taskGroup"></param>
    public void RemoveTask(IDownloadTaskGroup taskGroup)
    {
        taskGroup.Cancel();
        _ = _taskQuerySet.Remove(taskGroup.Destination);
        _ = QueuedTasks.Remove(taskGroup);
    }

    /// <summary>
    /// 清除所有任务
    /// </summary>
    public void ClearTasks()
    {
        foreach (var task in QueuedTasks)
            task.Cancel();
        QueuedTasks.Clear();
        _taskQuerySet.Clear();
    }

    /// <summary>
    /// Attempts to re-download a task only if its already queued and not running
    /// </summary>
    /// <remarks>
    /// Execute the task only if it's already queued
    /// </remarks>
    public bool TryExecuteTaskGroupInline(IDownloadTaskGroup taskGroup)
    {
        if (QueuedTasks.Contains(taskGroup) && taskGroup.CurrentState is DownloadState.Queued)
        {
            taskGroup.SubscribeProgress(_downloadTaskChannel.Writer);
            _ = _downloadTaskChannel.Writer.TryWrite(taskGroup.GetToken());
            return true;
        }

        return false;
    }

    private async Task DownloadAsync(ImageDownloadTask task)
    {
        await IncrementCounterAsync();
        _ = Download();

        return;
        async Task Download()
        {
            try
            {
                await task.StartAsync(_httpClient);
            }
            catch
            {
                // ignored
            }
            await DecrementCounterAsync();
        }
    }

    private async Task IncrementCounterAsync()
    {
        if (Interlocked.Increment(ref _workingTasks) == ConcurrencyDegree)
            await _throttle.ResetAsync();
    }

    private async Task DecrementCounterAsync()
    {
        _ = Interlocked.Decrement(ref _workingTasks);
        await _throttle.SetResultAsync(true);
    }
}
