#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppContext.cs
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
using Pixeval.CoreApi.Preference;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Windows.ApplicationModel;
using Microsoft.UI.Windowing;
using Pixeval.CoreApi.Net;
using Pixeval.Util.UI;

namespace Pixeval.AppManagement;

/// <summary>
/// Provide miscellaneous information about the app
/// </summary>
[AppContext<AppSettings>(ConfigKey = "Config", Type = ApplicationDataContainerType.Roaming, MethodName = "Config")]
[AppContext<Session>(ConfigKey = "Session", MethodName = "Session")]
public static partial class AppInfo
{
    public const string AppIdentifier = nameof(Pixeval);

    public const string AppProtocol = "pixeval";

    public const string AppLogoNoCaptionUri = "ms-appx:///Assets/Images/logo-no-caption.png";

    public const string IconApplicationUri = "ms-appx:///Assets/Images/logo44x44.ico";

    public static readonly string AppIconFontFamilyName = AppHelper.IsWindows11 ? "Segoe Fluent Icons" : "Segoe MDL2 Assets";

    public static readonly string DatabaseFilePath = AppKnownFolders.Local.Resolve("PixevalData.litedb");

    public static Versioning AppVersion { get; } = new();

    public static bool CustomizeTitleBarSupported => AppWindowTitleBar.IsCustomizationSupported();

    private static readonly WeakReference<SoftwareBitmapSource?> _imageNotAvailable = new(null);

    private static readonly WeakReference<Stream?> _imageNotAvailableStream = new(null);

    private static readonly WeakReference<SoftwareBitmapSource?> _pixivNoProfile = new(null);

    private static readonly WeakReference<Stream?> _pixivNoProfileStream = new(null);

    private static readonly WeakReference<SoftwareBitmapSource?> _icon = new(null);

    private static readonly WeakReference<Stream?> _iconStream = new(null);

    static AppInfo()
    {
        // Keys in the RoamingSettings will be synced through the devices of the same user
        // For more detailed information see https://docs.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data
        InitializeConfig();
        InitializeSession();
    }

    public static void SetNameResolver(AppSettings appSetting)
    {
        PixivApiNameResolver.IPAddresses = appSetting.PixivApiNameResolver;
        PixivImageNameResolver.IPAddresses = appSetting.PixivImageNameResolver;
    }

    public static string IconAbsolutePath => ApplicationUriToPath(new Uri(IconApplicationUri));

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

    public static async Task<SoftwareBitmapSource> GetNotAvailableImageAsync()
    {
        if (!_imageNotAvailable.TryGetTarget(out var target))
            _imageNotAvailable.SetTarget(target = await GetNotAvailableImageStream().GetSoftwareBitmapSourceAsync(false));
        return target;
    }

    public static Stream GetNotAvailableImageStream()
    {
        if (!_imageNotAvailableStream.TryGetTarget(out var target))
            _imageNotAvailableStream.SetTarget(target = GetAssetStream("Images/image-not-available.png"));
        return target;
    }

    public static async Task<SoftwareBitmapSource> GetPixivNoProfileImageAsync()
    {
        if (!_pixivNoProfile.TryGetTarget(out var target))
            _pixivNoProfile.SetTarget(target = await GetPixivNoProfileImageStream().GetSoftwareBitmapSourceAsync(false));
        return target;
    }

    public static Stream GetPixivNoProfileImageStream()
    {
        if (!_pixivNoProfileStream.TryGetTarget(out var target))
            _pixivNoProfileStream.SetTarget(target = GetAssetStream("Images/pixiv_no_profile.png"));
        return target;
    }

    public static async Task<SoftwareBitmapSource> GetIconImageAsync()
    {
        if (!_icon.TryGetTarget(out var target))
            _icon.SetTarget(target = await GetIconImageStream().GetSoftwareBitmapSourceAsync(false));
        return target;
    }

    public static Stream GetIconImageStream()
    {
        if (!_iconStream.TryGetTarget(out var target))
            _iconStream.SetTarget(target = GetAssetStream("Images/logo44x44.ico"));
        return target;
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
        return new X509Certificate2(await GetAssetBytesAsync("Certs/pixeval_server_cert.pfx"), AppProtocol, X509KeyStorageFlags.UserKeySet);
    }

    public static void RestoreHistories()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var downloadHistoryManager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        // the HasFlag is not allow in expression tree
        _ = downloadHistoryManager.Delete(
            entry => entry.State == DownloadState.Running ||
                     entry.State == DownloadState.Queued ||
                     entry.State == DownloadState.Paused);

        foreach (var observableDownloadTask in downloadHistoryManager.Enumerate())
        {
            App.AppViewModel.DownloadManager.QueueTask(observableDownloadTask);
        }
    }

    public static void ClearSession()
    {
        Functions.IgnoreException(() => ApplicationData.Current.LocalSettings.DeleteContainer(SessionContainerKey));
    }

    public static void SaveContext()
    {
        // Save the current resolution
        App.AppViewModel.AppSettings.WindowSize = WindowFactory.RootWindow.AppWindow.Size.ToSize();
        if (!App.AppViewModel.SignOutExit)
        {
            if (App.AppViewModel.MakoClient != null!)
                SaveSession(App.AppViewModel.MakoClient.Session);
            SaveConfig(App.AppViewModel.AppSettings);
        }
    }
}
