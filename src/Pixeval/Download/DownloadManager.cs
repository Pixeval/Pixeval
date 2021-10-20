using System;
using System.Collections.Generic;
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
        private readonly HttpClient _httpClient;
        private readonly TaskScheduler _downloadTaskScheduler;
        private readonly Queue<IDownloadTask> _queuedTasks;

        public int ConcurrencyDegree { get; set; }

        public IEnumerable<IDownloadTask> QueuedTasks => _queuedTasks;

        public DownloadManager(int concurrencyDegree, HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            ConcurrencyDegree = concurrencyDegree;
            _queuedTasks = new Queue<IDownloadTask>();
            _downloadTaskScheduler = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, ConcurrencyDegree).ConcurrentScheduler;
        }

        public void QueueTask(IDownloadTask task)
        {
            _queuedTasks.Enqueue(task);
            task.CurrentState = DownloadState.Queued;
            CreateDownloadTask(task);
        }

        public bool TryExecuteInlineAsync(IDownloadTask task)
        {
            if (_queuedTasks.Contains(task))
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
                new Progress<double>(task.OnProgressChanged),
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