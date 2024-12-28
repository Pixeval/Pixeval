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
using Pixeval.AppManagement;
using Pixeval.Controls.Windowing;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

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
    public static async Task<Result<Stream>> DownloadMemoryStreamAsync(
        this HttpClient httpClient,
        string url,
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 4096,
        CancellationToken cancellationToken = default)
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
        IProgress<double>? progress = null,
        long startPosition = 0,
        int bufferSize = 1 << 15,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (0 > startPosition)
                return new ArgumentOutOfRangeException(nameof(startPosition), "Too small");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (startPosition != 0)
                request.Headers.Range = new(startPosition, null);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            long? responseLength = response.Content.Headers.ContentLength;
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    destination.Position = 0;
                    break;
                case HttpStatusCode.PartialContent:
                    destination.Position = response.Content.Headers.ContentRange?.From ?? startPosition;
                    responseLength = response.Content.Headers.ContentRange?.Length ?? response.Content.Headers.ContentLength + destination.Position;
                    break;
                case HttpStatusCode.RequestedRangeNotSatisfiable:
                    if (response.Content.Headers.ContentRange?.Length != startPosition)
                        return new ArgumentOutOfRangeException(nameof(startPosition), "416: RequestedRangeNotSatisfiable");
                    progress?.Report(100);
                    return null;
            }

            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int bytesRead;
                long totalRead = destination.Position;
                DateTime lastReported = DateTime.MinValue;
                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                    totalRead += bytesRead;

                    var now = DateTime.Now;
                    if (now - lastReported > TimeSpan.FromSeconds(1) && responseLength.HasValue)
                    {
                        lastReported = now;
                        double percentage = totalRead / (double)responseLength.Value * 100;
                        progress?.Report(percentage);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            progress?.Report(100);
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
