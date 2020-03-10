using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public class PixevalDownloadTask
    {
        public async Task<MemoryStream> Execute(string url, IProgress<double> progress, CancellationTokenSource cancellationTokenSource = default)
        {
            using var response = await HttpClientFactory.GetResponseHeader(HttpClientFactory.PixivImage().Apply(_ => _.Timeout = TimeSpan.FromSeconds(30)), url);

            var contentLength = response.Content.Headers.ContentLength;
            if (!contentLength.HasValue)
            {
                throw new InvalidOperationException("cannot retrieve the content length of the request uri");
            }

            response.EnsureSuccessStatusCode();

            long bytesRead, totalRead = 0L;
            var byteBuffer = ArrayPool<byte>.Shared.Rent(4096);

            var memoryStream = new MemoryStream();
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            while ((bytesRead = await contentStream.ReadAsync(byteBuffer, 0, byteBuffer.Length)) != 0)
            {
                cancellationTokenSource?.Token.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                await memoryStream.WriteAsync(byteBuffer, 0, (int) bytesRead);
                progress.Report(totalRead / (double) contentLength);
            }
            ArrayPool<byte>.Shared.Return(byteBuffer, true);

            return memoryStream;
        }

        public static readonly PixevalDownloadTask Instance = new PixevalDownloadTask();
    }
}