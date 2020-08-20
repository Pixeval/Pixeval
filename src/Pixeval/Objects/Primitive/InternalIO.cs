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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace Pixeval.Wpf.Objects.Primitive
{
    // ReSharper disable once InconsistentNaming
    public static class InternalIO
    {
        public static async Task<byte[]> ToBytes(this Stream stream)
        {
            if (stream is MemoryStream ms) return ms.ToArray();

            var mStream = new MemoryStream();
            await stream.CopyToAsync(mStream);
            return mStream.ToArray();
        }

        public static BitmapImage CreateBitmapImageFromStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            if (stream.Length == 0) return null;
            var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.DelayCreation };
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = stream;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        public static async Task<byte[]> ToByteArrayAsync(this BitmapImage bitmapImage)
        {
            if (bitmapImage.StreamSource is { } stream)
            {
                var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0L;
                return ms.ToArray();
            }

            throw new ArgumentException(nameof(bitmapImage.StreamSource));
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

        public static IEnumerable<Stream> ReadGifZipEntries(byte[] bArr)
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
                    AnimationDelay = (int)delay[i]
                };
                mCollection.Add(img);
            }

            var settings = new QuantizeSettings { Colors = 256 };
            mCollection.Quantize(settings);
            mCollection.Optimize();
            mCollection.Write(ms, MagickFormat.Gif);
            return ms;
        }

        public static Stream ToStream(this BitmapImage bitmapImage)
        {
            var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(ms);
            return ms;
        }

        public static async Task Save<TEncoder>(this BitmapSource bitmapImage, string location)
            where TEncoder : BitmapEncoder, new()
        {
            var encoder = new TEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            await using var fs = CreateNewChannel(location);
            encoder.Save(fs);
        }

        public static FileStream CreateNewChannel(string location)
        {
            return new FileStream(location, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }
    }
}
