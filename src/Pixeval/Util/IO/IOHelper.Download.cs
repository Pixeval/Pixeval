#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IOHelper.Download.cs
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Pixeval.AppManagement;
using Pixeval.Controls.Windowing;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    private const int BlockSizeInBytes = 1024; // 1KB

    private const int LargeBufferMultipleInBytes = 1024 * BlockSizeInBytes; // 1MB

    private const int MaxBufferSizeInBytes = 16 * 1024 * BlockSizeInBytes; // 16MB

    private const int MaximumLargeBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

    private const int MaximumSmallBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

    private static readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager =
        new(new RecyclableMemoryStreamManager.Options(
            BlockSizeInBytes,
            LargeBufferMultipleInBytes,
            MaxBufferSizeInBytes,
            MaximumSmallBufferPoolSizeInBytes,
            MaximumLargeBufferPoolSizeInBytes));

    // To avoid collecting stack trace, which is quite a time-consuming task
    // and this exception is intended to be used at a massive magnitude
    private static readonly OperationCanceledException _cancellationMark = new();

    /// <summary>
    /// Attempts to download the content that are located by the <paramref name="url" /> argument
    /// to a <see cref="Memory{T}" /> asynchronously
    /// </summary>
    public static async Task<Result<Memory<byte>>> DownloadByteArrayAsync(this HttpClient httpClient, string url)
    {
        try
        {
            return Result<Memory<byte>>.AsSuccess(await httpClient.GetByteArrayAsync(url));
        }
        catch (Exception e)
        {
            return Result<Memory<byte>>.AsFailure(e);
        }
    }

    /// <inheritdoc cref="DownloadStreamAsync"/>
    public static async Task<Result<Stream>> DownloadMemoryStreamAsync(
        this HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 4096)
    {
        var uri = new Uri(url);

        if (uri.IsFile)
        {
            progress?.Report(100);
            return Result<Stream>.AsSuccess(OpenAsyncRead(url));
        }
        if (uri.Scheme is "ms-appx")
        {
            progress?.Report(100);
            return Result<Stream>.AsSuccess(OpenAsyncRead(AppInfo.ApplicationUriToPath(uri)));
        }
        var stream = _recyclableMemoryStreamManager.GetStream();
        var result = await httpClient.DownloadStreamAsync(stream, uri, cancellationToken, progress, startPosition, bufferSize);
        if (result is null)
        {
            stream.Position = 0;
            return Result<Stream>.AsSuccess(stream);
        }
        await stream.DisposeAsync();
        return Result<Stream>.AsFailure(result);
    }

    /// <summary>
    /// Attempts to download the content that are located by the <paramref name="uri" /> to a
    /// <see cref="Stream" /> with progress supported
    /// </summary>
    /// <remarks>
    /// A <see cref="CancellationHandle" /> is used instead of <see cref="CancellationToken" />, since this function will be called in
    /// such a frequent manner that the default behavior of <see cref="CancellationToken" /> will bring a huge impact on performance
    /// </remarks>
    public static async Task<Exception?> DownloadStreamAsync(
        this HttpClient httpClient,
        Stream destination,
        Uri uri,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 1 << 15)
    {
        try
        {
            if (0 > startPosition)
                return new ArgumentOutOfRangeException(nameof(startPosition), "Too small");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (startPosition is not 0)
                request.Headers.Range = new(startPosition, null);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var responseLength = null as long?;
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    destination.Position = 0;
                    responseLength = response.Content.Headers.ContentLength;
                    break;
                case HttpStatusCode.PartialContent:
                    destination.Position = response.Content.Headers.ContentRange?.From ?? startPosition;
                    responseLength = response.Content.Headers.ContentRange?.Length ?? response.Content.Headers.ContentLength + destination.Position;
                    break;
                case HttpStatusCode.RequestedRangeNotSatisfiable:
                    if (response.Content.Headers.ContentRange?.Length is { } length && length != startPosition)
                        return new ArgumentOutOfRangeException(nameof(startPosition), "416: RequestedRangeNotSatisfiable");
                    if (progress is not null)
                        _ = WindowFactory.RootWindow.DispatcherQueue.TryEnqueue(() => progress.Report(100));
                    return null;
            }

            _ = response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                var bytesRead = 0;
                var totalRead = destination.Position;
                var lastReported = DateTime.MinValue;
                while ((bytesRead = await contentStream.ReadAsync(new(buffer), cancellationToken).ConfigureAwait(false)) is not 0)
                {
                    await destination.WriteAsync(new(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                    totalRead += bytesRead;
                    // reduce the frequency of the invocation of the callback, otherwise it will draw a severe performance impact

                    var now = DateTime.Now;
                    if (now - lastReported > TimeSpan.FromSeconds(0.5) && progress is not null && responseLength is not null)
                    {
                        lastReported = now;
                        var percentage = totalRead / (double)responseLength * 100;
                        _ = WindowFactory.RootWindow.DispatcherQueue.TryEnqueue(() =>
                            progress.Report(percentage)); // percentage, 100 as base
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            if (progress is not null)
                _ = WindowFactory.RootWindow.DispatcherQueue.TryEnqueue(() => progress.Report(100));
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
