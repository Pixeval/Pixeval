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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using Windows.Storage.Streams;

namespace Pixeval.Download;

public class DownloadManager<TDownloadTask> : IDisposable where TDownloadTask : IDownloadTask
{
    private readonly Channel<TDownloadTask> _downloadTaskChannel;
    private readonly HttpClient _httpClient;
    private readonly ObservableCollection<TDownloadTask> _queuedTasks;
    private readonly SemaphoreSlim _semaphoreSlim;

    private readonly ISet<TDownloadTask> _taskQuerySet;

    // the generic parameter is used to simplify the code, the expression can be embedded directly into the condition of the while loop
    private readonly ReenterableAwaiter<bool> _throttle;
    private int _workingTasks;

    public DownloadManager(int concurrencyDegree, HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _queuedTasks = [];
        _taskQuerySet = new HashSet<TDownloadTask>();
        _throttle = new ReenterableAwaiter<bool>(true, true);
        _downloadTaskChannel = Channel.CreateUnbounded<TDownloadTask>();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        ConcurrencyDegree = concurrencyDegree;

        PollTask();
    }

    public int ConcurrencyDegree { get; set; }

    public IList<TDownloadTask> QueuedTasks => _queuedTasks;

    public void Dispose()
    {
        _httpClient.Dispose();
        _semaphoreSlim.Dispose();
    }

    public void QueueTask(TDownloadTask task)
    {
        // intrinsic download task are not counted
        if (task is not IIntrinsicDownloadTask && _taskQuerySet.Contains(task))
        {
            return;
        }

        _ = _taskQuerySet.Add(task);
        _queuedTasks.Add(task);
        // Start the task only if it is created and is ready-to-run
        if (task.CurrentState == DownloadState.Created)
        {
            SetState(task, DownloadState.Queued);
            QueueDownloadTask(task);
        }
    }

    private async void PollTask()
    {
        while (await _downloadTaskChannel.Reader.WaitToReadAsync())
        {
            while (await _throttle && _downloadTaskChannel.Reader.TryRead(out var task))
            {
                Download(task).Discard();
            }
        }
    }

    public void RemoveTask(TDownloadTask task)
    {
        _ = _taskQuerySet.Remove(task);
        _ = _queuedTasks.Remove(task);
    }

    public void ClearTasks()
    {
        foreach (var task in _queuedTasks.Where(t => t.CurrentState is DownloadState.Running or DownloadState.Paused or DownloadState.Created or DownloadState.Queued))
        {
            task.CancellationHandle.Cancel();
        }

        _queuedTasks.Clear();
        _taskQuerySet.Clear();
    }

    /// <summary>
    /// Tries to redownload a task only if its already queued and not running
    /// </summary>
    public bool TryExecuteInline(TDownloadTask task)
    {
        // Execute the task only if it's already queued and is not running
        if (_queuedTasks.Contains(task) && task.CurrentState is not DownloadState.Running or DownloadState.Created or DownloadState.Queued)
        {
            QueueDownloadTask(task);
            return true;
        }

        return false;
    }

    private void QueueDownloadTask(TDownloadTask task)
    {
        _ = _downloadTaskChannel.Writer.WriteAsync(task);
    }

    private async Task Download(TDownloadTask task)
    {
        IncrementCounter();

        var args = new DownloadStartingEventArgs();
        task.DownloadStarting(args);
        if (await args.DeferralAwaiter)
        {
            await DownloadInternal(task);
        }

        await DecrementCounterAsync();
    }

    private void IncrementCounter()
    {
        if (Interlocked.Increment(ref _workingTasks) == ConcurrencyDegree)
        {
            _throttle.Reset();
        }
    }

    private async Task DecrementCounterAsync()
    {
        _ = Interlocked.Decrement(ref _workingTasks);
        await _semaphoreSlim.WaitAsync();
        _throttle.SetResult(true);
        _ = _semaphoreSlim.Release();
    }

    private async Task DownloadInternal(TDownloadTask task)
    {
        SetState(task, DownloadState.Running);
        task.CancellationHandle.Register(() => SetState(task, DownloadState.Cancelled));
        task.CancellationHandle.RegisterPaused(() => SetState(task, DownloadState.Paused));
        task.CancellationHandle.RegisterResumed(() => SetState(task, DownloadState.Running));
        var ras = await _httpClient.DownloadAsIRandomAccessStreamAsync(task.Url, new Progress<int>(percentage => task.ProgressPercentage = percentage), task.CancellationHandle);
        switch (ras)
        {
            case Result<IRandomAccessStream>.Success(var resultStream):
                try
                {
                    if (task is ICustomBehaviorDownloadTask customBehaviorDownloadTask)
                    {
                        customBehaviorDownloadTask.Consume(resultStream);
                    }
                    else
                    {
                        using (resultStream)
                        {
                            await IoHelper.CreateAndWriteToFileAsync(resultStream, task.Destination);
                        }
                    }
                }
                catch (Exception e)
                {
                    Functions.IgnoreException(() => File.Delete(task.Destination));
                    ThreadingHelper.DispatchTask(() => task.ErrorCause = e);
                    SetState(task, DownloadState.Error);
                    task.Completion.SetException(e);
                }

                SetState(task, DownloadState.Completed);
                task.Completion.SetResult();
                break;
            case Result<IRandomAccessStream>.Failure(var exception):
                Functions.IgnoreException(() => File.Delete(task.Destination));
                if (exception is not OperationCanceledException and not null)
                {
                    ThreadingHelper.DispatchTask(() => task.ErrorCause = exception);
                    SetState(task, DownloadState.Error);
                    task.Completion.SetException(exception);
                }

                break;
        }
    }

    private static void SetState(TDownloadTask task, DownloadState state)
    {
        ThreadingHelper.DispatchTask(() => task.CurrentState = state);
    }
}
