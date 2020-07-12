#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;

namespace Pixeval.Core
{
    // ReSharper disable once InconsistentNaming
    public static class PixivIO
    {
        public static async Task<byte[]> GetBytes(string url)
        {
            var client = HttpClientFactory.PixivImage();

            byte[] res;
            try
            {
                res = await client.GetByteArrayAsync(url);
            }
            catch
            {
                return null;
            }

            return res;
        }

        public static async Task<BitmapImage> FromUrl(string url)
        {
            return await FromByteArray(await GetBytes(url));
        }

        public static async Task<BitmapImage> FromByteArray(byte[] bArr)
        {
            if (bArr == null || bArr.Length == 0) return null;
            await using var memoryStream = new MemoryStream(bArr);
            return InternalIO.CreateBitmapImageFromStream(memoryStream);
        }

        public static async Task<string> GetResizedBase64UriOfImageFromUrl(string url, string type = null)
        {
            return
                $"data:image/{type ?? url[(url.LastIndexOf('.') + 1)..]};base64,{Convert.ToBase64String(await GetBytes(url))}";
        }

        public static async Task<MemoryStream> Download(string url, IProgress<double> progress,
                                                        CancellationToken cancellationToken = default)
        {
            using var response =
                await HttpClientFactory.GetResponseHeader(
                    HttpClientFactory.PixivImage().Apply(_ => _.Timeout = TimeSpan.FromSeconds(30)), url);

            var contentLength = response.Content.Headers.ContentLength;
            if (!contentLength.HasValue) return new MemoryStream(await GetBytes(url));

            response.EnsureSuccessStatusCode();

            long bytesRead, totalRead = 0L;
            var byteBuffer = ArrayPool<byte>.Shared.Rent(4096);

            var memoryStream = new MemoryStream();
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            while ((bytesRead = await contentStream.ReadAsync(byteBuffer, 0, byteBuffer.Length, cancellationToken)) !=
                0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                await memoryStream.WriteAsync(byteBuffer, 0, (int) bytesRead, cancellationToken);
                progress.Report(totalRead / (double) contentLength);
            }

            cancellationToken.ThrowIfCancellationRequested();
            ArrayPool<byte>.Shared.Return(byteBuffer, true);

            return memoryStream;
        }
    }
}