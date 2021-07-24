using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
using Mako.Preference;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval
{
    /// <summary>
    /// Provide miscellaneous information about the app
    /// </summary>
    public static class AppContext
    {
        static AppContext()
        {
            AppLocalFolder = ApplicationData.Current.LocalFolder;
            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(SessionContainerKey))
            {
                ApplicationData.Current.LocalSettings.CreateContainer(SessionContainerKey, ApplicationDataCreateDisposition.Always);
            }
            // Keys in the RoamingSettings will be synced through the devices of the same user
            // For more detailed information see https://docs.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data
            if (!ApplicationData.Current.RoamingSettings.Containers.ContainsKey(ConfigurationContainerKey))
            {
                ApplicationData.Current.RoamingSettings.CreateContainer(ConfigurationContainerKey, ApplicationDataCreateDisposition.Always);
            }

            SessionContainer = ApplicationData.Current.LocalSettings.Containers[SessionContainerKey];
            ConfigurationContainer = ApplicationData.Current.RoamingSettings.Containers[ConfigurationContainerKey];

            // Save the context information when application closing
            EventChannel.Default.SubscribeOnUIThread<ApplicationExitingEvent>(SaveContext);
        }

        public const string AppIdentifier = "Pixeval";

        private const string SessionContainerKey = "Session";

        private const string ConfigurationContainerKey = "Config";

        public static StorageFolder AppLocalFolder;

        public static ApplicationDataContainer SessionContainer;

        public static ApplicationDataContainer ConfigurationContainer;

        public static string AppLoginProxyFolder = "LoginProxy";

        /// <summary>
        /// Copy and extract the login proxy zip to a local folder if:
        /// 1. The local file's checksum doesn't match with the one in the Assets folder(Assets/Binary/Pixeval.LoginProxy.zip)
        /// 2. The local file doesn't exist
        /// </summary>
        /// <returns>A task completes when the copy and extraction operation completes</returns>
        public static async Task CopyLoginProxyIfRequiredAsync()
        {
            var assetFile = await GetAssetBytesAsync("Binary/Pixeval.LoginProxy.zip");
            var assetChecksum = await assetFile.HashAsync<SHA256CryptoServiceProvider>();
            if (await TryGetFolderRelativeToLocalFolderAsync(AppLoginProxyFolder) is { } folder
                && await folder.TryGetItemAsync("checksum.sha256") is StorageFile checksum)
            {
                if (await checksum.ReadStringAsync() != assetChecksum)
                {
                    await CopyLoginProxyZipFileAndExtractInternalAsync(assetFile, assetChecksum);
                }

                await EventChannel.Default.PublishAsync(new ScanningLoginProxyEvent());
                return;
            }

            await CopyLoginProxyZipFileAndExtractInternalAsync(assetFile, assetChecksum);
            await EventChannel.Default.PublishAsync(new ScanningLoginProxyEvent());
        }

        private static async Task CopyLoginProxyZipFileAndExtractInternalAsync(byte[] assetFile, string checksum)
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
        /// Get the byte array of a file in the Assets folder
        /// </summary>
        /// <param name="relativeToAssetsFolder">A path with leading slash(or backslash) removed</param>
        /// <returns></returns>
        public static Task<byte[]> GetAssetBytesAsync(string relativeToAssetsFolder)
        {
            return GetResourceBytesAsync($"ms-appx:///Assets/{relativeToAssetsFolder}");
        }

        public static async Task<byte[]> GetResourceBytesAsync(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).ReadBytesAsync();
        }

        /// <summary>
        /// Get an item relative to <see cref="AppLocalFolder"/>
        /// </summary>
        /// <param name="pathWithoutSlash">A path with leading slash(or backslash) removed</param>
        /// <returns></returns>
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

        public static Task ClearAppLocalFolderAsync()
        {
            return AppLocalFolder.ClearDirectoryAsync();
        }

        public static async Task<X509Certificate2> GetFakeCaRootCertificateAsync()
        {
            return new(await GetAssetBytesAsync("Certs/pixeval_ca.cer"));
        }

        public static void SaveContext()
        {
            SaveSession();
            SaveConfiguration();
        }

        public static void SaveSession()
        {
            if (App.MakoClient.Session is { } session)
            {
                var values = SessionContainer.Values;
                values[nameof(Session.AccessToken)] = session.AccessToken;
                values[nameof(Session.Account)] = session.Account;
                values[nameof(Session.AvatarUrl)] = session.AvatarUrl;
                values[nameof(Session.Cookie)] = session.Cookie;
                values[nameof(Session.CookieCreation)] = session.CookieCreation;
                values[nameof(Session.ExpireIn)] = session.ExpireIn;
                values[nameof(Session.Id)] = session.Id;
                values[nameof(Session.IsPremium)] = session.IsPremium;
                values[nameof(Session.Name)] = session.Name;
                values[nameof(Session.RefreshToken)] = session.RefreshToken;
            }
        }

        public static void SaveConfiguration()
        {
            if (App.MakoClient.Configuration is { } configuration)
            {
                ConfigurationContainer.Values[nameof(configuration.Bypass)] = configuration.Bypass;
                ConfigurationContainer.Values[nameof(configuration.ConnectionTimeout)] = configuration.ConnectionTimeout;
                ConfigurationContainer.Values[nameof(configuration.MirrorHost)] = configuration.MirrorHost;
            }
        }

        public static Session? LoadSession()
        {
            try
            {
                var values = SessionContainer.Values;
                return new Session
                {
                    AccessToken = values[nameof(Session.AccessToken)].CastOrThrow<string>(),
                    Account = values[nameof(Session.Account)].CastOrThrow<string>(),
                    AvatarUrl = values[nameof(Session.AvatarUrl)].CastOrThrow<string>(),
                    Cookie = values[nameof(Session.Cookie)].CastOrThrow<string>(),
                    CookieCreation = values[nameof(Session.CookieCreation)].CastOrThrow<DateTimeOffset>(),
                    ExpireIn = values[nameof(Session.ExpireIn)].CastOrThrow<DateTimeOffset>(),
                    Id = values[nameof(Session.Id)].CastOrThrow<string>(),
                    IsPremium = values[nameof(Session.IsPremium)].CastOrThrow<bool>(),
                    Name = values[nameof(Session.Name)].CastOrThrow<string>(),
                    RefreshToken = values[nameof(Session.RefreshToken)].CastOrThrow<string>()
                };
            }
            catch
            {
                return null;
            }
        }

        public static MakoClientConfiguration? LoadConfiguration()
        {
            try
            {
                var values = ConfigurationContainer.Values;
                return new MakoClientConfiguration(
                    values[nameof(MakoClientConfiguration.ConnectionTimeout)].CastOrThrow<int>(),
                    values[nameof(MakoClientConfiguration.Bypass)].CastOrThrow<bool>(),
                    values[nameof(MakoClientConfiguration.MirrorHost)].CastOrThrow<string>(),
                    CultureInfo.CurrentUICulture
                );
            }
            catch
            {
                return null;
            }
        }
    }
}