#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DownloadManager.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Utilities;

namespace Pixeval.Download
{
    public class DownloadManager<TDownloadTask> : IDisposable where TDownloadTask : IDownloadTask
    {
        // the generic parameter is used to simplify the code, the expression can be embedded directly into the condition of the while loop
        private readonly ReenterableAwaiter<bool> _throttle;
        private readonly HttpClient _httpClient;
        private readonly ObservableCollection<TDownloadTask> _queuedTasks;
        private readonly ISet<TDownloadTask> _taskQuerySet;
        private readonly Channel<TDownloadTask> _downloadTaskChannel;
        private readonly SemaphoreSlim _semaphoreSlim;
        private int _workingTasks;

        public DownloadManager(int concurrencyDegree, HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _queuedTasks = new ObservableCollection<TDownloadTask>();
            _taskQuerySet = new HashSet<TDownloadTask>();
            _throttle = new ReenterableAwaiter<bool>(true, true);
            _downloadTaskChannel = Channel.CreateUnbounded<TDownloadTask>();
            _semaphoreSlim = new SemaphoreSlim(1, 1);
            ConcurrencyDegree = concurrencyDegree;

            PollTask();
        }

        public int ConcurrencyDegree { get; set; }

        public IList<TDownloadTask> QueuedTasks => _queuedTasks;

        public void QueueTask(TDownloadTask task)
        {
            if (_taskQuerySet.Contains(task))
            {
                return;
            }

            _taskQuerySet.Add(task);
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
            _taskQuerySet.Remove(task);
            _queuedTasks.Remove(task);
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
            _downloadTaskChannel.Writer.WriteAsync(task);
        }

        private async Task Download(TDownloadTask task)
        {
            await Task.Yield();
            IncrementCounter();

            var args = new DownloadStartingEventArgs();
            task.DownloadStarting(args);
            if (await args.DeferralAwaiter) // decide whether to continue depends on the Cancelled property
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
            Interlocked.Decrement(ref _workingTasks);
            await _semaphoreSlim.WaitAsync();
            _throttle.SetResult(true);
            _semaphoreSlim.Release();
        }

        private async Task DownloadInternal(TDownloadTask task)
        {
            SetState(task, DownloadState.Running);
            task.CancellationHandle.Register(() => SetState(task, DownloadState.Cancelled));
            task.CancellationHandle.RegisterPaused(() => SetState(task, DownloadState.Paused));
            task.CancellationHandle.RegisterResumed(() => SetState(task, DownloadState.Running));
            var ras = await _httpClient.DownloadAsIRandomAccessStreamAsync(
                task.Url,
                new Progress<int>(percentage => task.ProgressPercentage = percentage),
                task.CancellationHandle);
            switch (ras)
            {
                case Result<IRandomAccessStream>.Success (var resultStream):
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
                                IOHelper.CreateParentDirectories(task.Destination);
                                await using var stream = File.Open(task.Destination, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                                resultStream.Seek(0);
                                await resultStream.AsStreamForRead().CopyToAsync(stream);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Functions.IgnoreException(() => File.Delete(task.Destination));
                        App.AppViewModel.DispatchTask(() => task.ErrorCause = e);
                        SetState(task, DownloadState.Error);
                    }

                    SetState(task, DownloadState.Completed);
                    break;
                case Result<IRandomAccessStream>.Failure (var exception):
                    Functions.IgnoreException(() => File.Delete(task.Destination));
                    if (exception is not OperationCanceledException)
                    {
                        App.AppViewModel.DispatchTask(() => task.ErrorCause = exception);
                        SetState(task, DownloadState.Error);
                    }
                    break;
            }
        }

        private static void SetState(TDownloadTask task, DownloadState state)
        {
            App.AppViewModel.DispatchTask(() => task.CurrentState = state);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _semaphoreSlim.Dispose();
        }
    }
}