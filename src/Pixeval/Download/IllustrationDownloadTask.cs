using System;
using Pixeval.Util;

namespace Pixeval.Download
{
    public class IllustrationDownloadTask : IDownloadTask
    {
        public IllustrationDownloadTask(string url, string dest)
        {
            Url = url;
            Destination = dest;
            CancellationHandle = new CancellationHandle();
            CurrentState = DownloadState.Created;
        }

        public event Action<double>? ProgressChanged;

        public string Url { get; }

        public string Destination { get; }

        public CancellationHandle CancellationHandle { get; set; }

        public DownloadState CurrentState { get; set; }

        public Exception? ErrorCause { get; set; }

        public void OnProgressChanged(double percentage)
        {
            ProgressChanged?.Invoke(percentage);
        }
    }
}