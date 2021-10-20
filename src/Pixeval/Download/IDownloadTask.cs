using System;
using Pixeval.Util;

namespace Pixeval.Download
{
    public interface IDownloadTask
    {
        string Url { get; }

        string Destination { get; }

        event Action<double>? ProgressChanged;

        CancellationHandle CancellationHandle { get; set; }

        DownloadState CurrentState { get; set; }

        Exception? ErrorCause { get; set; }

        void OnProgressChanged(double percentage);
    }

    public static class DownloadTaskHelper
    {
        public static void Reset(this IDownloadTask task)
        {
            task.CancellationHandle.Reset();
            task.ErrorCause = null;
            task.CurrentState = DownloadState.Created;
        }
    }
}