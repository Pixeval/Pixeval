using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
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

        public static string AppConfigurationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppIdentifier);

        public static string AppSessionFileName = Path.Combine(AppConfigurationFolder, "Session.json");

        public static string AppConfigurationFileName = Path.Combine(AppConfigurationFolder, "Settings.json");

        public static string AppLoginProxyFolder = Path.Combine(AppConfigurationFolder, "LoginProxy");

        public static async Task CopyLoginProxyIfRequired()
        {
            var assetFile = await GetAssetBytes("Binary/Pixeval.LoginProxy.zip");
            var assetChecksum = await assetFile.HashAsync<SHA256CryptoServiceProvider>();
            if (Directory.Exists(AppLoginProxyFolder))
            {
                var checksumPath = Path.Combine(AppLoginProxyFolder, "checksum.sha256");
                if (File.Exists(checksumPath))
                {
                    var localChecksum = await File.ReadAllTextAsync(checksumPath);
                    if (assetChecksum != localChecksum)
                    {
                        await CopyLoginProxyZipFileAndExtractInternal(assetFile, assetChecksum);
                        return;
                    }
                }
            }
            await CopyLoginProxyZipFileAndExtractInternal(assetFile, assetChecksum);
        }

        private static async Task CopyLoginProxyZipFileAndExtractInternal(byte[] assetFile, string checksum)
        {
            IOHelper.ReinitializeDirectory(AppLoginProxyFolder);
            var zipFilePath = Path.Combine(AppLoginProxyFolder, "Pixeval.LoginProxy.zip");
            await File.WriteAllBytesAsync(zipFilePath, assetFile);
            ZipFile.ExtractToDirectory(zipFilePath, AppLoginProxyFolder);
            File.Delete(zipFilePath);
            await File.WriteAllTextAsync(Path.Combine(AppLoginProxyFolder, "checksum.sha256"), checksum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeToAssetsFolder">Without slash at the beginning</param>
        /// <returns></returns>
        public static Task<byte[]> GetAssetBytes(string relativeToAssetsFolder)
        {
            return GetResourceBytes($"ms-appx:///Assets/{relativeToAssetsFolder}");
        }

        public static async Task<byte[]> GetResourceBytes(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).ReadBytesAsync();
        }

        private static void
    }
}