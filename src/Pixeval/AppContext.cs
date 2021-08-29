using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using CommunityToolkit.WinUI.Helpers;
using Mako.Global.Enum;
using Mako.Preference;
using Mako.Util;
using Pixeval.Events;
using Pixeval.Util;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;

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
            // Remarks:
            // Keys in the RoamingSettings will be synced through the devices of the same user
            // For more detailed information see https://docs.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data
            if (!ApplicationData.Current.RoamingSettings.Containers.ContainsKey(ConfigurationContainerKey))
            {
                ApplicationData.Current.RoamingSettings.CreateContainer(ConfigurationContainerKey, ApplicationDataCreateDisposition.Always);
            }

            SessionContainer = ApplicationData.Current.LocalSettings.Containers[SessionContainerKey];
            ConfigurationContainer = ApplicationData.Current.RoamingSettings.Containers[ConfigurationContainerKey];

            // Save the context information when application closing
            EventChannel.Default.Subscribe<ApplicationExitingEvent>(SaveContext);
        }

        public const string AppIdentifier = "Pixeval";

        public const string AppProtocol = "pixeval";

        private const string SessionContainerKey = "Session";

        private const string ConfigurationContainerKey = "Config";

        public const string AppLogoNoCaptionUri = "ms-appx:///Assets/Images/logo-no-caption.png";

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
            var assetChecksum = await SHA256.Create().HashAsync(assetFile);
            if (await TryGetFolderRelativeToLocalFolderAsync(AppLoginProxyFolder) is { } folder
                && await folder.TryGetItemAsync("checksum.sha256") is StorageFile checksum)
            {
                if (await checksum.ReadStringAsync() != assetChecksum)
                {
                    await CopyLoginProxyZipFileAndExtractInternalAsync(assetFile, assetChecksum);
                }

                EventChannel.Default.Publish(new ScanningLoginProxyEvent());
                return;
            }

            await CopyLoginProxyZipFileAndExtractInternalAsync(assetFile, assetChecksum);
            EventChannel.Default.Publish(new ScanningLoginProxyEvent());
        }

        public static Uri GenerateAppLinkToIllustration(string id)
        {
            return new($"{AppProtocol}://illust/{id}");
        }

        public static async Task WriteLogoIcoIfNotExist()
        {
            const string iconName = "logo44x44.ico";
            if (await TryGetFileRelativeToLocalFolderAsync(iconName) is null)
            {
                var bytes = await GetAssetBytesAsync($"Images/{iconName}");
                await (await AppLocalFolder.CreateFileAsync(iconName)).WriteBytesAsync(bytes);
            }
        }

        public static async Task<string> GetIconAbsolutePath()
        {
            const string iconName = "logo44x44.ico";
            return (await AppLocalFolder.GetFileAsync(iconName)).Path;
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

        public static Task<IRandomAccessStreamWithContentType> GetAssetStreamAsync(string relativeToAssetsFolder)
        {
            return GetResourceStreamAsync($"ms-appx:///Assets/{relativeToAssetsFolder}");
        }

        public static async Task<byte[]> GetResourceBytesAsync(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).ReadBytesAsync();
        }

        public static async Task<IRandomAccessStreamWithContentType> GetResourceStreamAsync(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).OpenReadAsync();
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

        public static Task ClearTemporaryDirectory()
        {
            return ApplicationData.Current.TemporaryFolder.ClearDirectoryAsync();
        }

        public static IAsyncOperation<StorageFile> CreateTemporaryFileWithRandomNameAsync(string? extension = null)
        {
            return ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{Guid.NewGuid().ToString()}.{extension ?? string.Empty}", CreationCollisionOption.ReplaceExisting);
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
            if (App.AppSetting is { } appSetting)
            {
                ConfigurationContainer.Values[nameof(AppSetting.Theme)] = appSetting.Theme.CastOrThrow<int>();
                ConfigurationContainer.Values[nameof(AppSetting.ExcludeTags)] = appSetting.ExcludeTags.ToJson();
                ConfigurationContainer.Values[nameof(AppSetting.DisableDomainFronting)] = appSetting.DisableDomainFronting;
                ConfigurationContainer.Values[nameof(AppSetting.DefaultSortOption)] = appSetting.DefaultSortOption.CastOrThrow<int>();
                ConfigurationContainer.Values[nameof(AppSetting.TagMatchOption)] = appSetting.TagMatchOption.CastOrThrow<int>();
                ConfigurationContainer.Values[nameof(AppSetting.TargetFilter)] = appSetting.TargetFilter.CastOrThrow<int>();
                ConfigurationContainer.Values[nameof(AppSetting.PageLimitForKeywordSearch)] = appSetting.PageLimitForKeywordSearch;
                ConfigurationContainer.Values[nameof(AppSetting.SearchStartingFromPageNumber)] = appSetting.SearchStartingFromPageNumber;
                ConfigurationContainer.Values[nameof(AppSetting.PageLimitForSpotlight)] = appSetting.PageLimitForSpotlight;
                ConfigurationContainer.Values[nameof(AppSetting.MirrorHost)] = appSetting.MirrorHost ?? string.Empty;
                ConfigurationContainer.Values[nameof(AppSetting.MaxDownloadTaskConcurrencyLevel)] = appSetting.MaxDownloadTaskConcurrencyLevel;
                ConfigurationContainer.Values[nameof(AppSetting.DisplayTeachingTipWhenGeneratingAppLink)] = appSetting.DisplayTeachingTipWhenGeneratingAppLink;
                ConfigurationContainer.Values[nameof(AppSetting.ItemsNumberLimitForDailyRecommendations)] = appSetting.ItemsNumberLimitForDailyRecommendations;
                ConfigurationContainer.Values[nameof(AppSetting.FiltrateRestrictedContent)] = appSetting.FiltrateRestrictedContent;
                ConfigurationContainer.Values[nameof(AppSetting.UseFileCache)] = appSetting.UseFileCache;
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

        public static AppSetting? LoadConfiguration()
        {
            try
            {
                return new AppSetting(
                    ConfigurationContainer.Values[nameof(AppSetting.Theme)].CastOrThrow<ApplicationTheme>(),
                    (ConfigurationContainer.Values[nameof(AppSetting.ExcludeTags)].CastOrThrow<string>().FromJson<string[]>() ?? Array.Empty<string>()).ToObservableCollection(),
                    ConfigurationContainer.Values[nameof(AppSetting.DisableDomainFronting)].CastOrThrow<bool>(),
                    ConfigurationContainer.Values[nameof(AppSetting.DefaultSortOption)].CastOrThrow<IllustrationSortOption>(),
                    ConfigurationContainer.Values[nameof(AppSetting.TagMatchOption)].CastOrThrow<SearchTagMatchOption>(),
                    ConfigurationContainer.Values[nameof(AppSetting.TargetFilter)].CastOrThrow<TargetFilter>(),
                    ConfigurationContainer.Values[nameof(AppSetting.PageLimitForKeywordSearch)].CastOrThrow<int>(),
                    ConfigurationContainer.Values[nameof(AppSetting.SearchStartingFromPageNumber)].CastOrThrow<int>(),
                    ConfigurationContainer.Values[nameof(AppSetting.PageLimitForSpotlight)].CastOrThrow<int>(),
                    ConfigurationContainer.Values[nameof(AppSetting.MirrorHost)].CastOrThrow<string>(),
                    ConfigurationContainer.Values[nameof(AppSetting.MaxDownloadTaskConcurrencyLevel)].CastOrThrow<int>(),
                    ConfigurationContainer.Values[nameof(AppSetting.DisplayTeachingTipWhenGeneratingAppLink)].CastOrThrow<bool>(),
                    ConfigurationContainer.Values[nameof(AppSetting.ItemsNumberLimitForDailyRecommendations)].CastOrThrow<int>(),
                    ConfigurationContainer.Values[nameof(AppSetting.FiltrateRestrictedContent)].CastOrThrow<bool>(),
                    ConfigurationContainer.Values[nameof(AppSetting.UseFileCache)].CastOrThrow<bool>());
            }
            catch
            {
                return null;
            }
        }
    }
}