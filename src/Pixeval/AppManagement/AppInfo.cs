#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppInfo.cs
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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Windows.ApplicationModel;
using Microsoft.UI.Windowing;
using Pixeval.CoreApi.Net;
using Pixeval.Util.UI;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.AppManagement;

/// <summary>
/// Provide miscellaneous information about the app
/// </summary>
[AppContext<AppSettings>(ConfigKey = "Config", MethodName = "Config")]
[AppContext<LoginContext>(ConfigKey = "LoginContext", MethodName = "LoginContext")]
[AppContext<AppDebugTrace>(ConfigKey = "DebugTrace", MethodName = "DebugTrace")]
public static partial class AppInfo
{
    public const string AppIdentifier = nameof(Pixeval);

    public const string AppProtocol = "pixeval";

    public const string IconApplicationUri = "ms-appx:///Assets/Images/logo.ico";

    public static readonly string DatabaseFilePath = AppKnownFolders.Local.Resolve("PixevalData4.2.2.litedb");

    public static Versioning AppVersion { get; } = new();

    public static bool CustomizeTitleBarSupported => AppWindowTitleBar.IsCustomizationSupported();

    public static Task<ImageSource> ImageNotAvailable { get; } = GetImageNotAvailableStream().GetBitmapImageAsync(true, url: "Images/image-not-available.png");

    public static Stream GetImageNotAvailableStream() => GetAssetStream("Images/image-not-available.png");

    public static Task<ImageSource> PixivNoProfile { get; } = GetPixivNoProfileStream().GetBitmapImageAsync(true, url: "Images/pixiv_no_profile.png");

    public static Stream GetPixivNoProfileStream() => GetAssetStream("Images/pixiv_no_profile.png");

    public static Task<ImageSource> Icon { get; } = GetAssetStream("Images/logo.ico").GetBitmapImageAsync(true, url: "Images/logo.ico");

    static AppInfo()
    {
        // Keys in the RoamingSettings will be synced through the devices of the same user
        // For more detailed information see https://docs.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data
        InitializeConfig();
        InitializeLoginContext();
        InitializeDebugTrace();
    }

    public static void SetNameResolvers(AppSettings appSetting)
    {
        MakoHttpOptions.SetNameResolver(MakoHttpOptions.AppApiHost, appSetting.PixivAppApiNameResolver);
        MakoHttpOptions.SetNameResolver(MakoHttpOptions.ImageHost, appSetting.PixivImageNameResolver);
        MakoHttpOptions.SetNameResolver(MakoHttpOptions.ImageHost2, appSetting.PixivImageNameResolver2);
        MakoHttpOptions.SetNameResolver(MakoHttpOptions.OAuthHost, appSetting.PixivOAuthNameResolver);
        MakoHttpOptions.SetNameResolver(MakoHttpOptions.AccountHost, appSetting.PixivAccountNameResolver);
        MakoHttpOptions.SetNameResolver(MakoHttpOptions.WebApiHost, appSetting.PixivWebApiNameResolver);
    }

    public static string IconAbsolutePath => ApplicationUriToPath(new Uri(IconApplicationUri));

    public static Uri NavigationIconUri(string name) => new Uri($"ms-appx:///Assets/Images/Icons/{name}.png");

    public static string ApplicationUriToPath(Uri uri)
    {
        if (uri.Scheme is not "ms-appx")
        {
            // ms-appdata is handled by the caller.
            ThrowHelper.InvalidOperation("Uri is not using the ms-appx scheme");
        }

        var path = Uri.UnescapeDataString(uri.PathAndQuery).TrimStart('/');

        return Path.Combine(Package.Current.InstalledPath, uri.Host, path);
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

    public static Stream GetAssetStream(string relativeToAssetsFolder)
    {
        return File.OpenRead(ApplicationUriToPath(new Uri($"ms-appx:///Assets/{relativeToAssetsFolder}")));
    }

    public static async Task<byte[]> GetResourceBytesAsync(string path)
    {
        return await File.ReadAllBytesAsync(ApplicationUriToPath(new Uri(path)));
    }

    public static async Task<X509Certificate2> GetFakeCaRootCertificateAsync()
    {
        return new X509Certificate2(await GetAssetBytesAsync("Certs/pixeval_ca.cer"));
    }

    public static async Task<X509Certificate2> GetFakeServerCertificateAsync()
    {
        return new X509Certificate2(await GetAssetBytesAsync("Certs/pixeval_server_cert.zip"), AppProtocol, X509KeyStorageFlags.UserKeySet);
    }

    public static void RestoreHistories()
    {
        var downloadHistoryPersistentManager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var browseHistoryPersistentManager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();

        foreach (var downloadTaskGroup in downloadHistoryPersistentManager.Enumerate())
            App.AppViewModel.DownloadManager.QueueTask(downloadTaskGroup);

        foreach (var browseHistoryEntry in browseHistoryPersistentManager.Enumerate())
            browseHistoryPersistentManager.ObservableEntries.Insert(0, browseHistoryEntry);
    }

    public static void ClearConfig()
    {
        Functions.IgnoreException(() => ApplicationData.Current.RoamingSettings.DeleteContainer(ConfigContainerKey));
    }

    public static void ClearLoginContext()
    {
        Functions.IgnoreException(() => ApplicationData.Current.LocalSettings.DeleteContainer(LoginContextContainerKey));
    }

    public static void SaveContext()
    {
        // Save the current resolution
        if (WindowFactory.RootWindow.AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized })
            App.AppViewModel.AppSettings.IsMaximized = true;
        else
        {
            App.AppViewModel.AppSettings.IsMaximized = false;
            App.AppViewModel.AppSettings.WindowSize = WindowFactory.RootWindow.AppWindow.Size.ToSize();
        }
        SaveLoginContext(App.AppViewModel.LoginContext);
        SaveConfig(App.AppViewModel.AppSettings);
    }

    public static void SaveContextWhenExit()
    {
        SaveDebugTrace();
        SaveContext();
        App.AppViewModel.Dispose();
    }

    public static void SaveDebugTrace()
    {
        App.AppViewModel.AppDebugTrace.ExitedSuccessfully = true;
        SaveDebugTrace(App.AppViewModel.AppDebugTrace);
    }
}
