using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
using Pixeval.Util;

namespace Pixeval
{
    /// <summary>
    /// Provide miscellaneous information about the app
    /// </summary>
    public static class AppContext
    {
        public const string AppIdentifier = "Pixeval";

        public static StorageFolder AppLocalFolder = ApplicationData.Current.LocalFolder;

        public static string AppSessionFileName = "Session.json";

        public static string AppConfigurationFileName = "Setting.json";

        public static string AppLoginProxyFolder = "LoginProxy";

        /// <summary>
        /// Copy and extract the LoginProxy to the application data folder
        /// </summary>
        /// <returns></returns>
        public static async Task CopyLoginProxyIfRequired()
        {
            var assetFile = await GetAssetBytes("Binary/Pixeval.LoginProxy.zip");
            var assetChecksum = await assetFile.HashAsync<SHA256CryptoServiceProvider>();
            if (await TryGetFolderRelativeToLocalFolderAsync(AppLoginProxyFolder) is { } folder
                && await folder.TryGetItemAsync("checksum.sha256") is StorageFile checksum)
            {
                if (await checksum.ReadStringAsync() != assetChecksum)
                {
                    await CopyLoginProxyZipFileAndExtractInternal(assetFile, assetChecksum);
                }
                return;
            }
            await CopyLoginProxyZipFileAndExtractInternal(assetFile, assetChecksum);
        }

        private static async Task CopyLoginProxyZipFileAndExtractInternal(byte[] assetFile, string checksum)
        {
            var loginProxyFolder = await TryGetFolderRelativeToLocalFolderAsync(AppLoginProxyFolder);
            if (loginProxyFolder is { } folder)
            {
                await folder.ClearDirectoryAsync();
            }
            else
            {
                loginProxyFolder = await AppLocalFolder.CreateFolderAsync(AppLoginProxyFolder);
            }
            
            await using var memoryStream = new MemoryStream(assetFile);
            using var zipArchive = new ZipArchive(memoryStream);
            zipArchive.ExtractToDirectory(loginProxyFolder.Path);
            await (await loginProxyFolder.CreateFileAsync("checksum.sha256")).WriteStringAsync(checksum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeToAssetsFolder">With beginning slash removed</param>
        /// <returns></returns>
        public static Task<byte[]> GetAssetBytes(string relativeToAssetsFolder)
        {
            return GetResourceBytes($"ms-appx:///Assets/{relativeToAssetsFolder}");
        }

        public static async Task<byte[]> GetResourceBytes(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).ReadBytesAsync();
        }

        public static IAsyncOperation<IStorageItem?> TryGetItemRelativeToLocalFolderAsync(string pathWithoutSlash)
        {
            return AppLocalFolder.TryGetItemAsync(pathWithoutSlash);
        }

        public static async Task<StorageFile?> TryGetFileRelativeToLocalFolderAsync(string pathWithoutSlash)
        {
            return await AppLocalFolder.TryGetItemAsync(pathWithoutSlash) as StorageFile;
        }

        public static async Task<StorageFolder?> TryGetFolderRelativeToLocalFolderAsync(string pathWithoutSlash)
        {
            return await AppLocalFolder.TryGetItemAsync(pathWithoutSlash) as StorageFolder;
        }
    }
}