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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Attributes;
using Pixeval.CoreApi.Preference;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pixeval.AppManagement;

/// <summary>
///     Provide miscellaneous information about the app
/// </summary>
[LoadSaveConfiguration(typeof(AppSetting), nameof(ConfigurationContainer), CastMethod = "Pixeval.Utilities.Objects.CastOrThrow")]
public static partial class AppContext
{
    public const string AppIdentifier = "Pixeval";

    public const string AppProtocol = "pixeval";

    private const string SessionContainerKey = "Session";

    private const string ConfigurationContainerKey = "Config";

    public const string AppLogoNoCaptionUri = "ms-appx:///Assets/Images/logo-no-caption.png";

    public static readonly string DatabaseFilePath = AppKnownFolders.Local.Resolve("PixevalData.litedb");

    public static readonly string AppVersion = GitVersionInformation.AssemblySemVer;

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

    public static async Task<X509Certificate2> GetFakeServerCertificateAsync()
    {
        return new X509Certificate2(await GetAssetBytesAsync("Certs/pixeval_server_cert.pfx"), "pixeval", X509KeyStorageFlags.UserKeySet);
    }

    public static void RestoreHistories()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var downloadHistoryManager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        // the HasFlag is not allow in expression tree
        downloadHistoryManager.Delete(entry => entry.State == DownloadState.Running ||
                                               entry.State == DownloadState.Queued ||
                                               entry.State == DownloadState.Created ||
                                               entry.State == DownloadState.Paused);
        foreach (var observableDownloadTask in downloadHistoryManager.Enumerate())
        {
            App.AppViewModel.DownloadManager.QueueTask(observableDownloadTask);
        }
    }

    /// <summary>
    ///     Erase all personal data, including session, configuration and image cache
    /// </summary>
    public static Task ClearDataAsync()
    {
        return Functions.IgnoreExceptionAsync(async () =>
        {
            ApplicationData.Current.RoamingSettings.DeleteContainer(ConfigurationContainerKey);
            ApplicationData.Current.LocalSettings.DeleteContainer(SessionContainerKey);
            await ApplicationData.Current.LocalFolder.ClearDirectoryAsync();
            await AppKnownFolders.Temporary.ClearAsync();
            await AppKnownFolders.SavedWallPaper.ClearAsync();
        });
    }

    public static void SaveContext()
    {
        // Save the current resolution
        (App.AppViewModel.AppSetting.WindowWidth, App.AppViewModel.AppSetting.WindowHeight) = App.AppViewModel.GetAppWindowSizeTuple();
        if (!App.AppViewModel.SignOutExit)
        {
            SaveSession();
            SaveConfiguration(App.AppViewModel.AppSetting);
        }
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

}