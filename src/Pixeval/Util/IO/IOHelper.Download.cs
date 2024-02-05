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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using AppContext = Pixeval.AppManagement.AppContext;

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

    /// <summary>
    /// Attempts to download the content that are located by the <paramref name="url" /> to a
    /// <see cref="Stream" /> with progress supported
    /// </summary>
    /// <remarks>
    /// A <see cref="CancellationHandle" /> is used instead of <see cref="CancellationToken" />, since this function will be called in
    /// such a frequent manner that the default behavior of <see cref="CancellationToken" /> will bring a huge impact on performance
    /// </remarks>
    public static async Task<Result<Stream>> DownloadStreamAsync(
        this HttpClient httpClient,
        string url,
        IProgress<double>? progress = null,
        CancellationHandle? cancellationHandle = null)
    {
        try
        {
            var awaiter = new ReenterableAwaiter<bool>(!cancellationHandle?.IsPaused ?? true, true);
            var uri = new Uri(url);

            if (uri.IsFile)
            {
                progress?.Report(100);
                return Result<Stream>.AsSuccess(File.OpenRead(url));
            }
            if (uri.Scheme is "ms-appx")
            {
                progress?.Report(100);
                return uri.AbsolutePath switch
                {
                    "/Assets/Images/image-not-available.png" => Result<Stream>.AsSuccess(AppContext.GetNotAvailableImageStream()),
                    "/Assets/Images/pixiv_no_profile.png" => Result<Stream>.AsSuccess(AppContext.GetPixivNoProfileImageStream()),
                    _ => Result<Stream>.AsSuccess(File.OpenRead(AppContext.ApplicationUriToPath(uri)))
                };
            }

            cancellationHandle?.RegisterPaused(awaiter.Reset);
            cancellationHandle?.RegisterResumed(() => awaiter.SetResult(true));

            using var response = await httpClient.GetResponseHeader(uri);
            if (response.Content.Headers.ContentLength is not { } responseLength)
                return (await httpClient.DownloadByteArrayAsync(url)).Rewrap(m =>
                    (Stream)_recyclableMemoryStreamManager.GetStream(m.Span));

            _ = response.EnsureSuccessStatusCode();
            if (cancellationHandle?.IsCancelled is true)
                return Result<Stream>.AsFailure(_cancellationMark);

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            // Most cancellation happens when users are panning the ScrollView, where the
            // cancellation occurs while the `await response.Content.ReadAsStreamAsync()` is 
            // running, so we check the state right after the completion of that statement
            if (cancellationHandle?.IsCancelled is true)
                return Result<Stream>.AsFailure(_cancellationMark);

            var resultStream = _recyclableMemoryStreamManager.GetStream();
            int bytesRead, totalRead = 0;
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            while ((bytesRead = await contentStream.ReadAsync(buffer)) is not 0 && await awaiter)
            {
                if (cancellationHandle?.IsCancelled is true)
                {
                    await resultStream.DisposeAsync();
                    return Result<Stream>.AsFailure(_cancellationMark);
                }

                await resultStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
                // reduce the frequency of the invocation of the callback, otherwise it will draws a severe performance impact
                if (totalRead / (double)responseLength * 100 is var percentage)
                {
                    progress?.Report(percentage); // percentage, 100 as base
                }
            }

            ArrayPool<byte>.Shared.Return(buffer, true);
            _ = resultStream.Seek(0, SeekOrigin.Begin);
            progress?.Report(100);
            return Result<Stream>.AsSuccess(resultStream);
        }
        catch (Exception e)
        {
            return Result<Stream>.AsFailure(e);
        }
    }

    private static Task<HttpResponseMessage> GetResponseHeader(this HttpClient client, Uri uri)
    {
        return client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
    }
}
