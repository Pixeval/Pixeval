// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Velopack.Sources;

namespace Pixeval.Utilities.GitHub;

/// <summary>
/// Adapts Pixeval's GitHub HTTP client to Velopack's synchronous-looking source API.
/// </summary>
internal sealed class GitHubFileDownloader(Func<HttpClient> clientProvider) : IFileDownloader
{
    private static readonly TimeSpan[] _RetryDelays =
    [
        TimeSpan.FromMilliseconds(100),
        TimeSpan.FromMilliseconds(500)
    ];

    public async Task<string> DownloadString(
        string url,
        IDictionary<string, string>? headers,
        double timeout)
    {
        using var timeoutSource = CreateTimeoutSource(timeout, CancellationToken.None);
        return await ExecuteWithRetryAsync(
                async token =>
                {
                    using var response = await SendOnceAsync(url, headers, token).ConfigureAwait(false);
                    return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                },
                timeoutSource.Token)
            .ConfigureAwait(false);
    }

    public async Task<byte[]> DownloadBytes(
        string url,
        IDictionary<string, string>? headers,
        double timeout)
    {
        using var timeoutSource = CreateTimeoutSource(timeout, CancellationToken.None);
        return await ExecuteWithRetryAsync(
                async token =>
                {
                    using var response = await SendOnceAsync(url, headers, token).ConfigureAwait(false);
                    return await response.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false);
                },
                timeoutSource.Token)
            .ConfigureAwait(false);
    }

    public async Task DownloadFile(
        string url,
        string targetFile,
        Action<int> progress,
        IDictionary<string, string>? headers,
        double timeout,
        CancellationToken cancelToken)
    {
        using var timeoutSource = CreateTimeoutSource(timeout, cancelToken);
        await ExecuteWithRetryAsync(
                async token =>
                {
                    await DownloadFileOnceAsync(url, targetFile, progress, headers, token).ConfigureAwait(false);
                    return true;
                },
                timeoutSource.Token)
            .ConfigureAwait(false);
    }

    private async Task DownloadFileOnceAsync(
        string url,
        string targetFile,
        Action<int> progress,
        IDictionary<string, string>? headers,
        CancellationToken cancelToken)
    {
        using var response = await SendOnceAsync(url, headers, cancelToken).ConfigureAwait(false);
        var contentLength = response.Content.Headers.ContentLength;
        var directory = Path.GetDirectoryName(targetFile);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var input = await response.Content.ReadAsStreamAsync(cancelToken).ConfigureAwait(false);
        await using var output = new FileStream(
            targetFile,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 128 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        var buffer = new byte[128 * 1024];
        long totalRead = 0;
        progress?.Invoke(0);
        while (true)
        {
            var read = await input.ReadAsync(buffer.AsMemory(), cancelToken).ConfigureAwait(false);
            if (read is 0)
                break;

            await output.WriteAsync(buffer.AsMemory(0, read), cancelToken).ConfigureAwait(false);
            totalRead += read;
            if (contentLength is > 0)
                progress?.Invoke((int)Math.Clamp(totalRead * 100L / contentLength.Value, 0L, 100L));
        }

        progress?.Invoke(100);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(
        string url,
        IDictionary<string, string>? headers,
        CancellationToken cancelToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        // GitHub CDN connections may be reset while they are idle in the pool.
        request.Headers.ConnectionClose = true;
        if (headers is not null)
        {
            foreach (var (name, value) in headers)
            {
                if (request.Headers.TryAddWithoutValidation(name, value))
                    continue;

                if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                    && MediaTypeHeaderValue.TryParse(value, out var contentType))
                {
                    request.Content = new ByteArrayContent([]);
                    request.Content.Headers.ContentType = contentType;
                    continue;
                }

                throw new InvalidOperationException($"Unsupported HTTP request header: {name}");
            }
        }

        var response = await clientProvider().SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancelToken)
            .ConfigureAwait(false);
        try
        {
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static async Task<TResult> ExecuteWithRetryAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        for (var attempt = 0; ; attempt++)
        {
            cancelToken.ThrowIfCancellationRequested();
            try
            {
                return await operation(cancelToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (
                attempt < _RetryDelays.Length &&
                IsRetryable(exception, cancelToken))
            {
                await Task.Delay(_RetryDelays[attempt], cancelToken).ConfigureAwait(false);
            }
        }
    }

    private static bool IsRetryable(Exception exception, CancellationToken cancelToken) =>
        !cancelToken.IsCancellationRequested && exception switch
        {
            HttpRequestException { StatusCode: null } => true,
            HttpRequestException { StatusCode: { } statusCode } when IsRetryableStatusCode(statusCode) => true,
            IOException { InnerException: SocketException } => true,
            SocketException => true,
            TaskCanceledException => true,
            _ => false
        };

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
        || (int) statusCode is >= 500 and <= 599;

    private static CancellationTokenSource CreateTimeoutSource(double timeout, CancellationToken cancelToken)
    {
        var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        if (timeout is > 0 and < double.PositiveInfinity)
            timeoutSource.CancelAfter(TimeSpan.FromMinutes(timeout));
        return timeoutSource;
    }
}
