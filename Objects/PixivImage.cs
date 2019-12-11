using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Pixeval.Caching.Persisting;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Model.Web.Delegation;

namespace Pixeval.Objects
{
    public static class PixivImage
    {
        private static async Task<byte[]> FromUrlInternal(string url)
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
                if (!File.Exists(path))
                {
                    bitmapImage.SaveBitmapImage(path);
                }
            });
        }

        public static async Task<BitmapImage> GetAndCreateOrLoadFromCache(string url, string id, int episode = 0)
        {
            var path = Path.Combine(PixevalEnvironment.TempFolder, $"{id}_{episode}.png");
            if (File.Exists(path))
            {
                return FromByteArray(await File.ReadAllBytesAsync(path));
            }

            var img = await FromUrl(url);
            CacheImage(img, id, episode);

            return img;
        }
    }
}