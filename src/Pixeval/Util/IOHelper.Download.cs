using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Mako.Util;
using Microsoft.IO;
using Pixeval.Interop;

namespace Pixeval.Util
{
    // ReSharper disable once InconsistentNaming
    public static partial class IOHelper
    {
        private const int BlockSizeInBytes = 1024; // 1KB

        private const int LargeBufferMultipleInBytes = 1024 * BlockSizeInBytes; // 1MB

        private const int MaxBufferSizeInBytes = 16 * 1024 * BlockSizeInBytes; // 16MB

        // We maintain an approximately 2:1 of large to small buffer pool size ratio, because
        // the full-sized images may get up to more than 20MB, and the thumbnails are comparatively
        // far more smaller. Let's assume that the thumbnails have an average size of 512K(this is
        // already a pessimistic estimation), there would be at most 50 thumbnails appear at the same
        // time, so the total size would be 25MB. As for the large images, there would be at most one
        // on the screen at the same time, so 24MB is just more than sufficient
        private const int MaximumLargeBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

        private const int MaximumSmallBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

        private static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new(
            BlockSizeInBytes,
            LargeBufferMultipleInBytes,
            MaxBufferSizeInBytes,
            MaximumSmallBufferPoolSizeInBytes,
            MaximumLargeBufferPoolSizeInBytes);

        public static MemoryStream GetStream(this RecyclableMemoryStreamManager manager, Memory<byte> buffer, [CallerMemberName] string? tag = null)
        {
            return manager.GetStream(tag, buffer);
        }

        public static MemoryStream GetStream(this RecyclableMemoryStreamManager manager, byte[] buffer, [CallerMemberName] string? tag = null)
        {
            return manager.GetStream(tag, buffer);
        }

        /// <summary>
        /// Attempts to download the content that are located by the <paramref name="url"/> argument
        /// to a <see cref="Memory{T}"/> asynchronously
        /// </summary>
        public static Task<Result<Memory<byte>>> DownloadByteArrayAsync(this HttpClient httpClient, string url)
        {
            return Functions.TryCatchAsync(async () => Result<Memory<byte>>.OfSuccess(await httpClient.GetByteArrayAsync(url)), e => Task.FromResult(Result<Memory<byte>>.OfFailure(e)));
        }

        // avoid stack trace collections
        private static readonly OperationCanceledException CancellationMark = new();

        /// <summary>
        /// <para>
        /// Attempts to download the content that are located by the <paramref name="url"/> to a <see cref="IRandomAccessStream"/> with
        /// progress support
        /// </para>
        /// <remarks>
        /// A <see cref="CancellationHandle"/> is used instead of <see cref="CancellationToken"/>, since this function will be called in
        /// such a frequent manner that the default behavior of <see cref="CancellationToken"/> will brings a huge impact on performance
        /// </remarks>
        /// </summary>
        public static async Task<Result<IRandomAccessStream>> DownloadAsIRandomAccessStreamAsync(
            this HttpClient httpClient,
            string url,
            IProgress<double>? progress = null,
            CancellationHandle? cancellationHandle = default)
        {
            return (await httpClient.DownloadAsStreamAsync(url, progress, cancellationHandle)).Bind(stream => stream.AsRandomAccessStream());
        }

        public static async Task<Result<Stream>> DownloadAsStreamAsync(
            this HttpClient httpClient,
            string url,
            IProgress<double>? progress = null,
            CancellationHandle? cancellationHandle = default)
        {
            using var response = await httpClient.GetResponseHeader(url);
            if (response.Content.Headers.ContentLength is { } responseLength)
            {
                response.EnsureSuccessStatusCode();

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                // Most cancellation happens when users are panning the ScrollViewer, where the
                // cancellation occurs while the `await response.Content.ReadAsStreamAsync()` is 
                // running, so we check the state right after the completion of that statement
                if (cancellationHandle?.IsCancelled is true)
                {
                    return Result<Stream>.OfFailure(CancellationMark);
                }

                var resultStream = (RecyclableMemoryStream) RecyclableMemoryStreamManager.GetStream();
                int bytesRead, totalRead = 0;
                while ((bytesRead = await contentStream.ReadAsync(resultStream.GetMemory(1024))) != 0)
                {
                    if (cancellationHandle?.IsCancelled is true)
                    {
                        await resultStream.DisposeAsync();
                        return Result<Stream>.OfFailure(CancellationMark);
                    }

                    resultStream.Advance(bytesRead);
                    totalRead += bytesRead;
                    progress?.Report(totalRead / (double) responseLength * 100); // percentage, 100 as base
                }

                resultStream.Seek(0, SeekOrigin.Begin);
                return Result<Stream>.OfSuccess(resultStream);
            }

            return (await httpClient.DownloadByteArrayAsync(url)).Bind(m => (Stream) RecyclableMemoryStreamManager.GetStream(m));
        }

        public static async Task<Result<IRandomAccessStream>> DownloadAsIRandomAccessStreamAndRevealProgressInTaskBarIconAsync(
            this HttpClient httpClient,
            string url,
            IProgress<double>? progress = null,
            CancellationHandle? cancellationHandle = default)
        {
            UIHelper.SetTaskBarIconProgressState(TaskBarState.Normal);
            var newProgress = new Progress<double>(d =>
            {
                progress?.Report(d);
                UIHelper.SetTaskBarIconProgressValue((ulong) d, 100);
            });
            try
            {
                var result = await httpClient.DownloadAsIRandomAccessStreamAsync(url, newProgress, cancellationHandle);
                UIHelper.SetTaskBarIconProgressState(TaskBarState.NoProgress);
                return result;
            }
            catch
            {
                UIHelper.SetTaskBarIconProgressState(TaskBarState.Error);
                throw;
            }
        }
    }
}