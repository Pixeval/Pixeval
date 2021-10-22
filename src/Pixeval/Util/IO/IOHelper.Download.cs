#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IOHelper.Download.cs
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
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.IO;
using Pixeval.Interop;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Util.IO
{
    public static partial class IOHelper
    {
        private const int BlockSizeInBytes = 1024; // 1KB

        private const int LargeBufferMultipleInBytes = 1024 * BlockSizeInBytes; // 1MB

        private const int MaxBufferSizeInBytes = 16 * 1024 * BlockSizeInBytes; // 16MB

        private const int MaximumLargeBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

        private const int MaximumSmallBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

        private static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new(
            BlockSizeInBytes,
            LargeBufferMultipleInBytes,
            MaxBufferSizeInBytes,
            MaximumSmallBufferPoolSizeInBytes,
            MaximumLargeBufferPoolSizeInBytes);

        // Remarks:
        // To avoid collecting stack trace, which is quite a time-consuming task
        // and this exception is intended to be used at a massive magnitude
        private static readonly OperationCanceledException CancellationMark = new();

        /// <summary>
        ///     Attempts to download the content that are located by the <paramref name="url" /> argument
        ///     to a <see cref="Memory{T}" /> asynchronously
        /// </summary>
        public static Task<Result<Memory<byte>>> DownloadByteArrayAsync(this HttpClient httpClient, string url)
        {
            return Functions.TryCatchAsync(async () => Result<Memory<byte>>.OfSuccess(await httpClient.GetByteArrayAsync(url)), e => Task.FromResult(Result<Memory<byte>>.OfFailure(e)));
        }

        /// <summary>
        ///     <para>
        ///         Attempts to download the content that are located by the <paramref name="url" /> to a
        ///         <see cref="IRandomAccessStream" /> with
        ///         progress support
        ///     </para>
        ///     <remarks>
        ///         A <see cref="CancellationHandle" /> is used instead of <see cref="CancellationToken" />, since this function
        ///         will be called in
        ///         such a frequent manner that the default behavior of <see cref="CancellationToken" /> will brings a huge impact
        ///         on performance
        ///     </remarks>
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
            try
            {
                using var response = await httpClient.GetResponseHeader(url);
                if (response.Content.Headers.ContentLength is { } responseLength)
                {
                    response.EnsureSuccessStatusCode();
                    if (cancellationHandle?.IsCancelled is true)
                    {
                        return Result<Stream>.OfFailure(CancellationMark);
                    }

                    await using var contentStream = await response.Content.ReadAsStreamAsync();
                    // Remarks:
                    // Most cancellation happens when users are panning the ScrollViewer, where the
                    // cancellation occurs while the `await response.Content.ReadAsStreamAsync()` is 
                    // running, so we check the state right after the completion of that statement
                    if (cancellationHandle?.IsCancelled is true)
                    {
                        return Result<Stream>.OfFailure(CancellationMark);
                    }

                    var resultStream = new MemoryStream();
                    int bytesRead, totalRead = 0;
                    var buffer = ArrayPool<byte>.Shared.Rent(1024);
                    while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
                    {
                        if (cancellationHandle?.IsCancelled is true)
                        {
                            await resultStream.DisposeAsync();
                            return Result<Stream>.OfFailure(CancellationMark);
                        }

                        await resultStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        progress?.Report(totalRead / (double) responseLength * 100); // percentage, 100 as base
                    }

                    resultStream.Seek(0, SeekOrigin.Begin);
                    return Result<Stream>.OfSuccess(resultStream);
                }

                return (await httpClient.DownloadByteArrayAsync(url)).Bind(m => (Stream) RecyclableMemoryStreamManager.GetStream(m.Span));
            }
            catch (Exception e)
            {
                return Result<Stream>.OfFailure(e);
            }
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