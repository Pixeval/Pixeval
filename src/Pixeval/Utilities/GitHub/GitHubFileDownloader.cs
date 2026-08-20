// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Velopack.Sources;

namespace Pixeval.Utilities.GitHub;

/// <summary>
/// Adapts Pixeval's GitHub HTTP client to Velopack's synchronous-looking source API.
/// </summary>
internal sealed class GitHubFileDownloader(Func<HttpClient> clientProvider) : IFileDownloader
{
    public async Task<string> DownloadString(
        string url,
        IDictionary<string, string>? headers,
        double timeout)
    {
        using var timeoutSource = CreateTimeoutSource(timeout, CancellationToken.None);
        using var response = await SendAsync(url, headers, timeoutSource.Token).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(timeoutSource.Token).ConfigureAwait(false);
    }

    public async Task<byte[]> DownloadBytes(
        string url,
        IDictionary<string, string>? headers,
        double timeout)
    {
        using var timeoutSource = CreateTimeoutSource(timeout, CancellationToken.None);
        using var response = await SendAsync(url, headers, timeoutSource.Token).ConfigureAwait(false);
        return await response.Content.ReadAsByteArrayAsync(timeoutSource.Token).ConfigureAwait(false);
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
        using var response = await SendAsync(url, headers, timeoutSource.Token).ConfigureAwait(false);
        var contentLength = response.Content.Headers.ContentLength;
        var directory = Path.GetDirectoryName(targetFile);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var input = await response.Content.ReadAsStreamAsync(timeoutSource.Token).ConfigureAwait(false);
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
            var read = await input.ReadAsync(buffer.AsMemory(), timeoutSource.Token).ConfigureAwait(false);
            if (read is 0)
                break;

            await output.WriteAsync(buffer.AsMemory(0, read), timeoutSource.Token).ConfigureAwait(false);
            totalRead += read;
            if (contentLength is > 0)
                progress?.Invoke((int)Math.Clamp(totalRead * 100L / contentLength.Value, 0L, 100L));
        }

        progress?.Invoke(100);
    }

    private async Task<HttpResponseMessage> SendAsync(
        string url,
        IDictionary<string, string>? headers,
        CancellationToken cancelToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
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

    private static CancellationTokenSource CreateTimeoutSource(double timeout, CancellationToken cancelToken)
    {
        var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        if (timeout is > 0 and < double.PositiveInfinity)
            timeoutSource.CancelAfter(TimeSpan.FromMinutes(timeout));
        return timeoutSource;
    }
}
