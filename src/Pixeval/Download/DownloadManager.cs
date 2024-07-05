#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadManager.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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
using WinUI3Utilities;

namespace Pixeval.Download;

public partial class DownloadManager : IDisposable
{
    /// <summary>
    /// 正在下载的任务队列，数量不超过<see cref="ConcurrencyDegree"/>
    /// </summary>
    private readonly Channel<IDownloadTaskBase> _downloadTaskChannel = Channel.CreateUnbounded<IDownloadTaskBase>();

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
    /// <see cref="_downloadTaskChannel"/>中的数量
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
    public void QueueTask(IDownloadTaskGroup task)
    {
        if (_taskQuerySet.TryGetValue(task.Destination, out var v) && v == task)
            return;
        _taskQuerySet[task.Destination] = task;

        if (v is not null)
            _ = QueuedTasks.Remove(v);
        QueuedTasks.Insert(0, task);
        _ = _downloadTaskChannel.Writer.TryWrite(task);
    }

    /// <summary>
    /// 当队列有排队且未到达最大同时下载数时，开始下载任务
    /// </summary>
    private async void PollTask()
    {
        while (await _downloadTaskChannel.Reader.WaitToReadAsync())
            while (await _throttle && _downloadTaskChannel.Reader.TryRead(out var queuedTask))
                switch (queuedTask)
                {
                    case ImageDownloadTask imageTask:
                        imageTask.DownloadTryResume += t => _downloadTaskChannel.Writer.TryWrite(t);
                        imageTask.DownloadTryReset += t => _downloadTaskChannel.Writer.TryWrite(t);
                        // 子任务入列时一定是处于Queued状态，需要直接运行的
                        _ = DownloadAsync(imageTask);
                        break;
                    case IDownloadTaskGroup taskGroup:
                        await taskGroup.InitializeTaskGroupAsync();
                        foreach (var subTask in taskGroup)
                        {
                            subTask.DownloadTryResume += t => _downloadTaskChannel.Writer.TryWrite(t);
                            subTask.DownloadTryReset += t => _downloadTaskChannel.Writer.TryWrite(t);
                            // 任务组中的任务需要判断是否处于Queued状态才能运行
                            if (subTask.CurrentState is DownloadState.Queued)
                            {
                                _ = DownloadAsync(subTask);
                                _ = await _throttle;
                            }
                        }
                        break;
                }
    }

    /// <summary>
    /// 清除指定任务
    /// </summary>
    /// <param name="task"></param>
    public void RemoveTask(IDownloadTaskGroup task)
    {
        _ = _taskQuerySet.Remove(task.Destination);
        _ = QueuedTasks.Remove(task);
    }

    /// <summary>
    /// 清除所有任务
    /// </summary>
    public void ClearTasks()
    {
        //    foreach (var task in QueuedTasks)
        //        await task.CancelAsync();

        QueuedTasks.Clear();
        _taskQuerySet.Clear();
    }

    /// <summary>
    /// Tries to redownload a task only if its already queued and not running
    /// </summary>
    /// <remarks>
    /// Execute the task only if it's already queued
    /// </remarks>
    public bool TryExecuteTaskGroupInline(IDownloadTaskGroup task)
    {
        if (QueuedTasks.Contains(task) && task.CurrentState is DownloadState.Queued)
        {
            _ = _downloadTaskChannel.Writer.TryWrite(task);
            return true;
        }

        return false;
    }

    private async Task DownloadAsync(ImageDownloadTask task)
    {
        await IncrementCounterAsync();
        await DownloadInternalAsync(task);
        await DecrementCounterAsync();
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

    private async Task DownloadInternalAsync(ImageDownloadTask task)
    {
        try
        {
            await (task.CurrentState switch
            {
                DownloadState.Queued => task.StartAsync(_httpClient),
                DownloadState.Paused => task.ResumeAsync(_httpClient),
                _ => ThrowHelper.ArgumentOutOfRange<DownloadState, Task>(task.CurrentState)
            });
        }
        catch
        {
            // ignored
        }
    }
}
