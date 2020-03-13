// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageMagick;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects;

namespace Pixeval.Core
{
    // ReSharper disable once InconsistentNaming
    internal static class PixivIO
    {
        public static async Task<byte[]> FromUrlInternal(string url)
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
            return FromByteArray(await FromUrlInternal(url));
        }

        public static BitmapImage FromByteArray(byte[] bArr)
        {
            if (bArr == null || bArr.Length == 0) return null;
            using var memoryStream = new MemoryStream(bArr);
            return FromStream(memoryStream);
        }

        public static BitmapImage FromStream(Stream stream)
        {
            if (stream.Length == 0) return null;
            var bmp = new BitmapImage {CreateOptions = BitmapCreateOptions.DelayCreation};
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = stream;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        public static Task<BitmapImage> FromStreamAsync(Stream stream)
        {
            return Task.Run(() => FromStream(stream));
        }

        public static IReadOnlyList<Stream> ReadGifZipEntries(Stream stream)
        {
            var dis = new List<Stream>();
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);

            foreach (var zipArchiveEntry in zipArchive.Entries)
            {
                using var aStream = zipArchiveEntry.Open();

                var ms = new MemoryStream();
                aStream.CopyTo(ms);
                ms.Position = 0L;
                dis.Add(ms);
            }

            return dis;
        }

        public static IReadOnlyList<Stream> ReadGifZipEntries(byte[] bArr)
        {
            return ReadGifZipEntries(new MemoryStream(bArr));
        }

        public static Stream MergeGifStream(IReadOnlyList<Stream> streams, IReadOnlyList<long> delay)
        {
            var ms = new MemoryStream();
            using var mCollection = new MagickImageCollection();

            for (var i = 0; i < streams.Count; i++)
            {
                var iStream = streams[i];

                var img = new MagickImage(iStream)
                {
                    AnimationDelay = (int) delay[i]
                };
                mCollection.Add(img);
            }

            var settings = new QuantizeSettings {Colors = 256};
            mCollection.Quantize(settings);
            mCollection.Optimize();
            mCollection.Write(ms, MagickFormat.Gif);
            return ms;
        }

        public static async Task<MemoryStream> Download(string url, IProgress<double> progress, CancellationTokenSource cancellationTokenSource = default)
        {
            using var response = await HttpClientFactory.GetResponseHeader(HttpClientFactory.PixivImage().Apply(_ => _.Timeout = TimeSpan.FromSeconds(30)), url);

            var contentLength = response.Content.Headers.ContentLength;
            if (!contentLength.HasValue) throw new InvalidOperationException("cannot retrieve the content length of the request uri");

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
    }
}