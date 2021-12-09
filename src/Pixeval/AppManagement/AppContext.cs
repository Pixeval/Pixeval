#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AppContext.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Messages;
using Pixeval.Options;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.AppManagement;

/// <summary>
///     Provide miscellaneous information about the app
/// </summary>
public static class AppContext
{
    public const string AppIdentifier = "Pixeval";

    public const string AppProtocol = "pixeval";

    public static readonly string DatabaseFilePath = AppKnownFolders.Local.Resolve("PixevalData.db");

    private const string SessionContainerKey = "Session";

    private const string ConfigurationContainerKey = "Config";

    public const string AppLogoNoCaptionUri = "ms-appx:///Assets/Images/logo-no-caption.png";

    public static readonly AppVersion AppVersion = new(IterationStage.Alpha, 0, 1, 0, 1);

    private static readonly ApplicationDataContainer SessionContainer;

    private static readonly ApplicationDataContainer ConfigurationContainer;

    private static SoftwareBitmapSource? _imageNotAvailable;

    private static IRandomAccessStream? _imageNotAvailableStream;

    private static SoftwareBitmapSource? _pixivNoProfile;

    private static IRandomAccessStream? _pixivNoProfileStream;

    static AppContext()
    {
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
    }

    public static async Task<SoftwareBitmapSource> GetNotAvailableImageAsync()
    {
        return _imageNotAvailable ??= await (await GetNotAvailableImageStreamAsync()).GetSoftwareBitmapSourceAsync(true);
    }

    public static async Task<IRandomAccessStream> GetNotAvailableImageStreamAsync()
    {
        return _imageNotAvailableStream ??= await GetAssetStreamAsync("Images/image-not-available.png");
    }

    public static async Task<SoftwareBitmapSource> GetPixivNoProfileImageAsync()
    {
        return _pixivNoProfile ??= await (await GetPixivNoProfileImageStreamAsync()).GetSoftwareBitmapSourceAsync(true);
    }

    public static async Task<IRandomAccessStream> GetPixivNoProfileImageStreamAsync()
    {
        return _pixivNoProfileStream ??= await GetAssetStreamAsync("Images/pixiv_no_profile.png");
    }

    /// <summary>
    ///     Copy and extract the login proxy zip to a local folder if:
    ///     1. The local file's checksum doesn't match with the one in the Assets folder(Assets/Binary/Pixeval.LoginProxy.zip)
    ///     2. The local file doesn't exist
    /// </summary>
    /// <returns>A task completes when the copy and extraction operation completes</returns>
    public static async Task CopyLoginProxyIfRequiredAsync()
    {
        var assetFile = await GetAssetBytesAsync("Binary/Pixeval.LoginProxy.zip");
        var assetChecksum = await SHA256.Create().HashAsync(assetFile);
        if (await AppKnownFolders.LoginProxy.TryGetFileRelativeToSelfAsync("checksum.sha256") is { } checksum)
        {
            if (await checksum.ReadStringAsync() != assetChecksum)
            {
                await CopyLoginProxyZipFileAndExtractInternalAsync(assetFile, assetChecksum);
            }

            WeakReferenceMessenger.Default.Send(new ScanningLoginProxyMessage());
            return;
        }

        await CopyLoginProxyZipFileAndExtractInternalAsync(assetFile, assetChecksum);
        WeakReferenceMessenger.Default.Send(new ScanningLoginProxyMessage());
    }

    public static async Task WriteLogoIcoIfNotExist()
    {
        const string iconName = "logo44x44.ico";
        if (await AppKnownFolders.Local.TryGetFileRelativeToSelfAsync(iconName) is null)
        {
            var bytes = await GetAssetBytesAsync($"Images/{iconName}");
            await (await AppKnownFolders.Local.CreateFileAsync(iconName)).WriteBytesAsync(bytes);
        }
    }

    public static async Task<string> GetIconAbsolutePath()
    {
        const string iconName = "logo44x44.ico";
        return (await AppKnownFolders.Local.GetFileAsync(iconName)).Path;
    }

    private static async Task CopyLoginProxyZipFileAndExtractInternalAsync(byte[] assetFile, string checksum)
    {
        var loginProxyFolder = AppKnownFolders.LoginProxy;
        await loginProxyFolder.ClearAsync();
        await using var memoryStream = new MemoryStream(assetFile);
        using var zipArchive = new ZipArchive(memoryStream);
        zipArchive.ExtractToDirectory(loginProxyFolder.Self.Path);
        await (await loginProxyFolder.CreateFileAsync("checksum.sha256")).WriteStringAsync(checksum);
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

    public static async Task RestoreHistories()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var downloadHistoryManager = await scope.ServiceProvider.GetRequiredService<Task<DownloadHistoryPersistentManager>>();
        // the HasFlag is not allow in expression tree
        await downloadHistoryManager.DeleteAsync(entry => entry.State == DownloadState.Running ||
                                                          entry.State == DownloadState.Queued ||
                                                          entry.State == DownloadState.Created ||
                                                          entry.State == DownloadState.Paused);
        foreach (var observableDownloadTask in await downloadHistoryManager.EnumerateAsync())
        {
            App.AppViewModel.DownloadManager.QueueTask(observableDownloadTask);
        }
    }

    /// <summary>
    /// Erase all personal data, including session, configuration and image cache
    /// </summary>
    public static async Task ClearDataAsync()
    {
        ApplicationData.Current.RoamingSettings.DeleteContainer(ConfigurationContainerKey);
        ApplicationData.Current.LocalSettings.DeleteContainer(SessionContainerKey);
        await ApplicationData.Current.LocalFolder.ClearDirectoryAsync();
        await AppKnownFolders.Temporary.ClearAsync();
        await AppKnownFolders.SavedWallPaper.ClearAsync();
    }

    public static void SaveContext()
    {
        // Save the current resolution
        (App.AppViewModel.AppSetting.WindowWidth, App.AppViewModel.AppSetting.WindowHeight) = App.AppViewModel.GetAppWindowSizeTuple();
        SaveSession();
        SaveConfiguration();
    }

    public static void SaveSession()
    {
        if (App.AppViewModel.MakoClient.Session is { } session)
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
        if (App.AppViewModel.AppSetting is { } appSetting)
        {
            ConfigurationContainer.Values[nameof(AppSetting.Theme)] = appSetting.Theme.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.DisableDomainFronting)] = appSetting.DisableDomainFronting;
            ConfigurationContainer.Values[nameof(AppSetting.DefaultSortOption)] = appSetting.DefaultSortOption.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.TagMatchOption)] = appSetting.TagMatchOption.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.TargetFilter)] = appSetting.TargetFilter.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.PreLoadRows)] = appSetting.PreLoadRows;
            ConfigurationContainer.Values[nameof(AppSetting.PageLimitForKeywordSearch)] = appSetting.PageLimitForKeywordSearch;
            ConfigurationContainer.Values[nameof(AppSetting.SearchStartingFromPageNumber)] = appSetting.SearchStartingFromPageNumber;
            ConfigurationContainer.Values[nameof(AppSetting.PageLimitForSpotlight)] = appSetting.PageLimitForSpotlight;
            ConfigurationContainer.Values[nameof(AppSetting.MirrorHost)] = appSetting.MirrorHost ?? string.Empty;
            ConfigurationContainer.Values[nameof(AppSetting.MaxDownloadTaskConcurrencyLevel)] = appSetting.MaxDownloadTaskConcurrencyLevel;
            ConfigurationContainer.Values[nameof(AppSetting.DisplayTeachingTipWhenGeneratingAppLink)] = appSetting.DisplayTeachingTipWhenGeneratingAppLink;
            ConfigurationContainer.Values[nameof(AppSetting.ItemsNumberLimitForDailyRecommendations)] = appSetting.ItemsNumberLimitForDailyRecommendations;
            ConfigurationContainer.Values[nameof(AppSetting.FiltrateRestrictedContent)] = appSetting.FiltrateRestrictedContent;
            ConfigurationContainer.Values[nameof(AppSetting.UseFileCache)] = appSetting.UseFileCache;
            ConfigurationContainer.Values[nameof(AppSetting.WindowWidth)] = appSetting.WindowWidth;
            ConfigurationContainer.Values[nameof(AppSetting.WindowHeight)] = appSetting.WindowHeight;
            ConfigurationContainer.Values[nameof(AppSetting.ThumbnailDirection)] = appSetting.ThumbnailDirection.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.LastCheckedUpdate)] = appSetting.LastCheckedUpdate;
            ConfigurationContainer.Values[nameof(AppSetting.DownloadUpdateAutomatically)] = appSetting.DownloadUpdateAutomatically;
            ConfigurationContainer.Values[nameof(AppSetting.AppFontFamilyName)] = appSetting.AppFontFamilyName;
            ConfigurationContainer.Values[nameof(AppSetting.DefaultSelectedTabItem)] = appSetting.DefaultSelectedTabItem.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.SearchDuration)] = appSetting.SearchDuration.CastOrThrow<int>();
            ConfigurationContainer.Values[nameof(AppSetting.UsePreciseRangeForSearch)] = appSetting.UsePreciseRangeForSearch;
            ConfigurationContainer.Values[nameof(AppSetting.SearchStartDate)] = appSetting.SearchStartDate;
            ConfigurationContainer.Values[nameof(AppSetting.SearchEndDate)] = appSetting.SearchEndDate;
            ConfigurationContainer.Values[nameof(AppSetting.DefaultDownloadPathMacro)] = appSetting.DefaultDownloadPathMacro;
            ConfigurationContainer.Values[nameof(AppSetting.OverwriteDownloadedFile)] = appSetting.OverwriteDownloadedFile;
            ConfigurationContainer.Values[nameof(AppSetting.MaximumDownloadHistoryRecords)] = appSetting.MaximumDownloadHistoryRecords;
            ConfigurationContainer.Values[nameof(AppSetting.MaximumSearchHistoryRecords)] = appSetting.MaximumSearchHistoryRecords;
            ConfigurationContainer.Values[nameof(AppSetting.ReverseSearchApiKey)] = appSetting.ReverseSearchApiKey;
            ConfigurationContainer.Values[nameof(AppSetting.ReverseSearchResultSimilarityThreshold)] = appSetting.ReverseSearchResultSimilarityThreshold;
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

    public static AppSetting? LoadSetting()
    {
        try
        {
            return new AppSetting(
                ConfigurationContainer.Values[nameof(AppSetting.Theme)].CastOrThrow<ApplicationTheme>(),
                ConfigurationContainer.Values[nameof(AppSetting.DisableDomainFronting)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.DefaultSortOption)].CastOrThrow<IllustrationSortOption>(),
                ConfigurationContainer.Values[nameof(AppSetting.TagMatchOption)].CastOrThrow<SearchTagMatchOption>(),
                ConfigurationContainer.Values[nameof(AppSetting.TargetFilter)].CastOrThrow<TargetFilter>(),
                ConfigurationContainer.Values[nameof(AppSetting.PreLoadRows)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.PageLimitForKeywordSearch)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.SearchStartingFromPageNumber)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.PageLimitForSpotlight)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.MirrorHost)].CastOrThrow<string>(),
                ConfigurationContainer.Values[nameof(AppSetting.MaxDownloadTaskConcurrencyLevel)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.DisplayTeachingTipWhenGeneratingAppLink)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.ItemsNumberLimitForDailyRecommendations)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.FiltrateRestrictedContent)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.UseFileCache)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.WindowWidth)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.WindowHeight)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.ThumbnailDirection)].CastOrThrow<ThumbnailDirection>(),
                ConfigurationContainer.Values[nameof(AppSetting.LastCheckedUpdate)].CastOrThrow<DateTimeOffset>(),
                ConfigurationContainer.Values[nameof(AppSetting.DownloadUpdateAutomatically)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.AppFontFamilyName)].CastOrThrow<string>(),
                ConfigurationContainer.Values[nameof(AppSetting.DefaultSelectedTabItem)].CastOrThrow<MainPageTabItem>(),
                ConfigurationContainer.Values[nameof(AppSetting.SearchDuration)].CastOrThrow<SearchDuration>(),
                ConfigurationContainer.Values[nameof(AppSetting.UsePreciseRangeForSearch)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.SearchStartDate)].CastOrThrow<DateTimeOffset>(),
                ConfigurationContainer.Values[nameof(AppSetting.SearchEndDate)].CastOrThrow<DateTimeOffset>(),
                ConfigurationContainer.Values[nameof(AppSetting.DefaultDownloadPathMacro)].CastOrThrow<string>(),
                ConfigurationContainer.Values[nameof(AppSetting.OverwriteDownloadedFile)].CastOrThrow<bool>(),
                ConfigurationContainer.Values[nameof(AppSetting.MaximumDownloadHistoryRecords)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.MaximumSearchHistoryRecords)].CastOrThrow<int>(),
                ConfigurationContainer.Values[nameof(AppSetting.ReverseSearchApiKey)].CastOrThrow<string>(),
                ConfigurationContainer.Values[nameof(AppSetting.ReverseSearchResultSimilarityThreshold)].CastOrThrow<int>());
        }
        catch
        {
            return null;
        }
    }
}