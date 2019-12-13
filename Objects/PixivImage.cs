using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageMagick;
using Pixeval.Data.Web.Delegation;
using Pixeval.Persisting;

namespace Pixeval.Objects
{
    public static class PixivImage
    {
        internal static async Task<byte[]> FromUrlInternal(string url)
        {
            var client = HttpClientFactory.PixivImage();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");

            return await client.GetByteArrayAsync(url);
        }

        public static async Task<BitmapImage> FromUrl(string url)
        {
            return FromByteArray(await FromUrlInternal(url));
        }

        public static BitmapImage FromByteArray(byte[] bArr)
        {
            using var memoryStream = new MemoryStream(bArr);
            return FromStream(memoryStream);
        }

        public static BitmapImage FromStream(Stream stream)
        {
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

        internal static void SaveBitmapImage(this BitmapImage bitmapImage, string path)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using var fs = new FileStream(path, FileMode.Create);
            encoder.Save(fs);
        }

        public static void CacheImage(byte[] imageBArr, string id, int episode = 0)
        {
            CacheImage(FromByteArray(imageBArr), id, episode);
        }

        public static void CacheImage(BitmapImage bitmapImage, string id, int episode = 0)
        {
            Task.Run(() =>
            {
                var path = Path.Combine(PixevalEnvironment.TempFolder, $"{id}_{episode}.png");
                if (!File.Exists(path)) bitmapImage.SaveBitmapImage(path);
            });
        }

        public static async Task<BitmapImage> GetAndCreateOrLoadFromCache(bool usingCache, string url, string id, int episode = 0)
        {
            if (usingCache)
            {
                var path = Path.Combine(PixevalEnvironment.TempFolder, $"{id}_{episode}.png");
                if (File.Exists(path)) 
                {
                    var toCache = FromByteArray(await File.ReadAllBytesAsync(path));
                    CacheImage(toCache, id, episode);
                    return toCache;
                }
            }

            var img = await FromUrl(url);
            return img;
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

        internal static IReadOnlyList<Stream> ReadGifZipEntries(byte[] bArr)
        {
            return ReadGifZipEntries(new MemoryStream(bArr));
        }

        internal static async IAsyncEnumerable<BitmapImage> ReadGifZipBitmapImages(byte[] bArr)
        {
            var lst = await Task.Run(() => ReadGifZipEntries(bArr));

            foreach (var s in lst)
            {
                yield return await FromStreamAsync(s);
            }
        }

        internal static Task<BitmapImage> GetAndCreateOrLoadFromCacheInternal(string url, string id, int episode = 0)
        {
            return GetAndCreateOrLoadFromCache(Settings.Global.CachingThumbnail, url, id, episode);
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
    }
}