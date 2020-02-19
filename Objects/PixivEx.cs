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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageMagick;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Persisting;

namespace Pixeval.Objects
{
    internal static class PixivEx
    {
        public static async Task<byte[]> FromUrlInternal(string url)
        {
            var client = HttpClientFactory.PixivImage();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");

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

        public static void SaveBitmapImage(this BitmapImage bitmapImage, string path)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using var fs = new FileStream(path, FileMode.Create);
            encoder.Save(fs);
        }

        private static IReadOnlyList<Stream> ReadGifZipEntries(Stream stream)
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

        public static async IAsyncEnumerable<BitmapImage> ReadGifZipBitmapImages(byte[] bArr)
        {
            var lst = await Task.Run(() => ReadGifZipEntries(bArr));

            foreach (var s in lst) yield return await FromStreamAsync(s);
        }

        private static Stream MergeGifStream(IReadOnlyList<Stream> streams, IReadOnlyList<long> delay)
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

        public static async Task<Stream> GetGifStream(string link, IReadOnlyList<long> delay)
        {
            return MergeGifStream(ReadGifZipEntries(await FromUrlInternal(link)), delay);
        }

        public static async Task<BitmapImage> GetGifBitmap(string link, IReadOnlyList<long> delay)
        {
            return FromStream(MergeGifStream(ReadGifZipEntries(await FromUrlInternal(link)), delay));
        }

        public static Task DownloadIllustsInternal(IEnumerable<Illustration> illustrations)
        {
            return DownloadIllusts(illustrations, Settings.Global.DownloadLocation);
        }

        public static Task DownloadIllusts(IEnumerable<Illustration> illustrations, string path)
        {
            return Task.WhenAll(illustrations.Select(illustration => DownloadIllust(illustration, path)));
        }

        public static async Task DownloadIllustInternal(Illustration illustration)
        {
            await DownloadIllust(illustration, Settings.Global.DownloadLocation);
        }

        public static async Task DownloadIllust(Illustration illustration, string rootPath)
        {
            var path = Directory.CreateDirectory(rootPath).FullName;
            if (illustration.IsManga)
            {
                await DownloadManga(illustration, path);
            }
            else if (illustration.IsUgoira)
            {
                await DownloadUgoira(illustration, path);
            }
            else
            {
                var url = illustration.Origin.IsNullOrEmpty() ? illustration.Large : illustration.Origin;
                await File.WriteAllBytesAsync(Path.Combine(path, $"[{Texts.FormatPath(illustration.UserName)}]{illustration.Id}{Texts.GetExtension(url)}"), await FromUrlInternal(url));
            }
        }

        public static async Task DownloadUgoira(Illustration illustration, string rootPath)
        {
            if (!illustration.IsUgoira) throw new InvalidOperationException();

            var metadata = await HttpClientFactory.AppApiService.GetUgoiraMetadata(illustration.Id);
            var url = FormatGifZipUrl(metadata.UgoiraMetadataInfo.ZipUrls.Medium);

            await using var img = (MemoryStream) await GetGifStream(url, metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToList());
            await File.WriteAllBytesAsync(Path.Combine(rootPath, $"[{Texts.FormatPath(illustration.UserName)}]{illustration.Id}.gif"), await Task.Run(() => img.ToArray()));
        }

        public static async Task DownloadManga(Illustration illustration, string rootPath)
        {
            if (!illustration.IsManga) throw new InvalidOperationException();

            var mangaDir = Directory.CreateDirectory(Path.Combine(rootPath, $"[{Texts.FormatPath(illustration.UserName)}]{illustration.Id}")).FullName;
            for (var i = 0; i < illustration.MangaMetadata.Length; i++)
            {
                var url = illustration.MangaMetadata[i].Origin.IsNullOrEmpty() ? illustration.MangaMetadata[i].Large : illustration.MangaMetadata[i].Origin;
                await File.WriteAllBytesAsync(Path.Combine(mangaDir, $"{i}{Texts.GetExtension(url)}"), await FromUrlInternal(url));
            }
        }

        public static async Task DownloadSpotlight(SpotlightArticle article)
        {
            var root = Directory.CreateDirectory(Path.Combine(Settings.Global.DownloadLocation, "Spotlight", article.Title)).FullName;

            var result = await Tasks<string, Illustration>.Of(await PixivClient.Instance.GetArticleWorks(article.Id.ToString()))
                .Mapping(PixivHelper.IllustrationInfo)
                .Construct()
                .WhenAll();

            await Task.WhenAll(result.Select(t => DownloadIllust(t, root)));
        }

        public static string FormatGifZipUrl(string link)
        {
            return !link.EndsWith("ugoira1920x1080.zip") ? Regex.Replace(link, "ugoira(\\d+)x(\\d+).zip", "ugoira1920x1080.zip") : link;
        }

        public static string GetSpotlightCover(SpotlightArticle article)
        {
            var match = Regex.Match(article.Thumbnail, "/(?<illust_id>\\d+)_p\\d+_master1200\\.jpg|png");
            if (match.Success)
            {
                var url = Regex.Replace(article.Thumbnail, "/c/\\d+x\\d+_\\d+/img-master/", "/img-original/").Replace("_master1200", string.Empty);
                return url;
            }

            return article.Thumbnail;
        }
    }
}