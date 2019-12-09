using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Pixeval.Caching.Persisting;
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

        public static void CacheImage(byte[] imageBArr, string fileName)
        {
            File.WriteAllBytesAsync(Path.Combine(PixevalEnvironment.TempFolder, fileName), imageBArr);
        }
    }
}