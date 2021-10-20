using System;
using System.IO;
using Windows.Storage.Streams;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.Download
{
    public class AnimatedIllustrationDownloadTask : ICustomBehaviorDownloadTask
    {
        private readonly UgoiraMetadataResponse _metadata;

        public AnimatedIllustrationDownloadTask(string url, UgoiraMetadataResponse metadata, string destination)
        {
            _metadata = metadata;
            Url = url;
            Destination = destination;
            CancellationHandle = new CancellationHandle();
            CurrentState = DownloadState.Created;
        }

        public string Url { get; }

        public string Destination { get; }

        public event Action<double>? ProgressChanged;

        public CancellationHandle CancellationHandle { get; set; }

        public DownloadState CurrentState { get; set; }

        public Exception? ErrorCause { get; set; }

        public void OnProgressChanged(double percentage)
        {
            ProgressChanged?.Invoke(percentage);
        }

        public async void Consume(IRandomAccessStream stream)
        {
            using (stream)
            {
                using var gifStream = await IOHelper.GetGifStreamFromZipStreamAsync(stream.AsStreamForRead(), _metadata);

                IOHelper.CreateParentDirectories(Destination);
                await using var fileStream = File.Open(Destination, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                await gifStream.AsStreamForRead().CopyToAsync(fileStream);
            }
        }
    }
}