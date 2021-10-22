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
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download
{
    public class DownloadManager
    {
        private readonly TaskScheduler _downloadTaskScheduler;
        private readonly HttpClient _httpClient;
        private readonly ObservableCollection<IDownloadTask> _queuedTasks;

        public DownloadManager(int concurrencyDegree, HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            ConcurrencyDegree = concurrencyDegree;
            _queuedTasks = new ObservableCollection<IDownloadTask>();
            _downloadTaskScheduler = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, ConcurrencyDegree).ConcurrentScheduler;
        }

        public int ConcurrencyDegree { get; set; }

        public IEnumerable<IDownloadTask> QueuedTasks => _queuedTasks;

        public void QueueTask(IDownloadTask task)
        {
            _queuedTasks.Add(task);
            // Start the task only if it is created and is ready-to-run
            if (task.CurrentState == DownloadState.Created)
            {
                task.CurrentState = DownloadState.Queued;
                CreateDownloadTask(task);
            }
        }

        public bool TryExecuteInlineAsync(IDownloadTask task)
        {
            // Execute the task only if it's already queued and is not running
            if (_queuedTasks.Contains(task) && !task.IsRunning())
            {
                CreateDownloadTask(task);
                return true;
            }

            return false;
        }

        private void CreateDownloadTask(IDownloadTask task)
        {
            Task.Factory.StartNew(() => Download(task), CancellationToken.None, TaskCreationOptions.DenyChildAttach, _downloadTaskScheduler);
        }

        private async Task Download(IDownloadTask task)
        {
            task.CurrentState = DownloadState.Running;
            task.CancellationHandle.Register(() => task.CurrentState = DownloadState.Cancelled);
            var ras = await _httpClient.DownloadAsIRandomAccessStreamAsync(
                task.Url,
                new Progress<double>(d => task.ProgressPercentage = d),
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
                        task.CurrentState = DownloadState.Error;
                        task.ErrorCause = e;
                    }

                    task.CurrentState = DownloadState.Completed;
                    break;
                case Result<IRandomAccessStream>.Failure (var exception):
                    Functions.IgnoreException(() => File.Delete(task.Destination));
                    task.CurrentState = DownloadState.Error;
                    task.ErrorCause = exception;
                    break;
            }
        }
    }
}