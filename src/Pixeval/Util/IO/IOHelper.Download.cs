// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.AppManagement;
using Pixeval.Util.Threading;
using Pixeval.Utilities;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
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
    public static Task<Result<Stream>> DownloadMemoryStreamAsync(
        this HttpClient httpClient,
        string url,
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 4096,
        CancellationToken cancellationToken = default) =>
        DownloadMemoryStreamAsync(httpClient, new Uri(url), progress, startPosition, bufferSize, cancellationToken);

    /// <inheritdoc cref="DownloadStreamAsync"/>
    public static async Task<Result<Stream>> DownloadMemoryStreamAsync(
        this HttpClient httpClient,
        Uri uri,
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 4096,
        CancellationToken cancellationToken = default)
    {
        if (uri.IsFile)
        {
            progress?.Report(100);
            return Result<Stream>.AsSuccess(FileHelper.OpenAsyncRead(uri.OriginalString));
        }
        if (uri.Scheme is "ms-appx")
        {
            progress?.Report(100);
            return Result<Stream>.AsSuccess(FileHelper.OpenAsyncRead(AppInfo.ApplicationUriToPath(uri)));
        }
        var stream = Streams.RentStream();
        var result = await httpClient.DownloadStreamAsync(stream, uri, progress, startPosition, bufferSize, cancellationToken);
        if (result is null)
        {
            stream.Position = 0;
            return Result<Stream>.AsSuccess(stream);
        }
        await stream.DisposeAsync();
        return Result<Stream>.AsFailure(result);
    }

    /// <summary>
    /// 一定是从网络下载<br/>
    /// Attempts to download the content that are located by the <paramref name="uri" /> to a
    /// <see cref="Stream" /> with progress supported
    /// </summary>
    public static async Task<Exception?> DownloadStreamAsync(
        this HttpClient httpClient,
        Stream destination,
        Uri uri,
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 1 << 15,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (0 > startPosition)
                return new ArgumentOutOfRangeException(nameof(startPosition), @"Too small");

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
                        return new ArgumentOutOfRangeException(nameof(startPosition), @"416: RequestedRangeNotSatisfiable");
                    if (progress is not null)
                        await ThreadingHelper.DispatchAsync(() => progress.Report(100));
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
                        var percentage = totalRead / (double) responseLength * 100;
                        await ThreadingHelper.DispatchAsync(() =>
                            progress.Report(percentage)); // percentage, 100 as base
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            if (progress is not null)
                await ThreadingHelper.DispatchAsync(() => progress.Report(100));
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
