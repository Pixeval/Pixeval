using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Util.IO;
using System.Security.Cryptography.X509Certificates;
using Windows.Storage;

namespace Pixeval
{
    internal static class ResourcesHelpers
    {

        public const string AppLogoNoCaptionUri = "ms-appx:///Assets/Images/logo-no-caption.png";

        public static async Task<SoftwareBitmapSource> GetNotAvailableImageAsync()
        {
            return await (await GetNotAvailableImageStreamAsync()).GetSoftwareBitmapSourceAsync(true);
        }

        public static async Task<IRandomAccessStream> GetNotAvailableImageStreamAsync()
        {
            return await GetAssetStreamAsync("Images/image-not-available.png");
        }

        public static async Task<SoftwareBitmapSource> GetPixivNoProfileImageAsync()
        {
            return await (await GetPixivNoProfileImageStreamAsync()).GetSoftwareBitmapSourceAsync(true);
        }

        public static async Task<IRandomAccessStream> GetPixivNoProfileImageStreamAsync()
        {
            return await GetAssetStreamAsync("Images/pixiv_no_profile.png");
        }

        /// <summary>
        ///     Get the byte array of a file in the Assets folder
        /// </summary>
        /// <param name="relativeToAssetsFolder">A path with leading slash(or backslash) removed</param>
        /// <returns></returns>
        public static Task<byte[]> GetAssetBytesAsync(string relativeToAssetsFolder)
        {
            return GetResourceBytesAsync($"ms-appx:///Assets/{relativeToAssetsFolder}");
        }

        public static Task<IRandomAccessStreamWithContentType> GetAssetStreamAsync(string relativeToAssetsFolder)
        {
            return GetResourceStreamAsync($"ms-appx:///Assets/{relativeToAssetsFolder}");
        }

        public static async Task<byte[]> GetResourceBytesAsync(string path)
        {
            return (await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).ReadBytesAsync())!;
        }

        public static async Task<IRandomAccessStreamWithContentType> GetResourceStreamAsync(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).OpenReadAsync();
        }

        public static async Task<X509Certificate2> GetFakeCaRootCertificateAsync()
        {
            return new X509Certificate2(await GetAssetBytesAsync("Certs/pixeval_ca.cer"));
        }

    }
}
