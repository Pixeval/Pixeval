// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Pixeval.Models.Home;
using Pixeval.Utilities;
using SharpYaml;

namespace Pixeval.AppManagement;

/// <summary>
/// Provide miscellaneous information about the app
/// </summary>
public static class AppInfo
{
    public const string AppIdentifier = nameof(Pixeval);

    public const string AppProtocol = "pixeval";

    public static string ApplicationFolderPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppIdentifier);

    public static string SettingsFolder { get; } = Path.Combine(ApplicationFolderPath, "Settings");

    public static string CacheFolder { get; } = Path.Combine(ApplicationFolderPath, "Cache");

    public static string LogsFolder { get; } = Path.Combine(ApplicationFolderPath, "Logs");

    public static string TempFolder { get; } = Path.Combine(ApplicationFolderPath, "Temp");

    public static string ExtensionsFolder { get; } = Path.Combine(ApplicationFolderPath, "Extensions");

    private static string AppSettingsPath { get; } = Path.Combine(SettingsFolder, "settings.yaml");

    private static string LoginContextPath { get; } = Path.Combine(SettingsFolder, "login_context.yaml");

    private static string HomePageCardsPath { get; } = Path.Combine(SettingsFolder, "home_page_cards.yaml");

    public static string DatabaseFilePath { get; } = Path.Combine(SettingsFolder, "PixevalData4.3.12.sqlite");

    public static readonly Uri IconApplicationUri = new($"avares://{AppIdentifier}/Assets/logo.ico");

    public static readonly Uri SvgIconApplicationUri = new($"avares://{AppIdentifier}/Assets/logo.svg");

    public static Versioning AppVersion { get; } = new();

    static AppInfo()
    {
        if (Directory.Exists(TempFolder))
            Directory.Delete(TempFolder, true);
        // Ensure directories exist
        _ = Directory.CreateDirectory(ApplicationFolderPath);
        _ = Directory.CreateDirectory(SettingsFolder);
        _ = Directory.CreateDirectory(CacheFolder);
        _ = Directory.CreateDirectory(LogsFolder);
        _ = Directory.CreateDirectory(TempFolder);
        _ = Directory.CreateDirectory(ExtensionsFolder);
    }

    public const string ImageNotAvailablePath = $"avares://{AppIdentifier}/Assets/image-not-available.png";

    public const string PixivNoProfilePath = $"avares://{AppIdentifier}/Assets/pixiv_no_profile.png";

    public static Stream GetImageNotAvailableStream() => AssetLoader.Open(new Uri(ImageNotAvailablePath));

    public static Stream GetPixivNoProfileStream() => AssetLoader.Open(new Uri(PixivNoProfilePath));

    public static async Task<byte[]> GetAssetBytesAsync(string relativeToAssetsFolder)
    {
        await using var stream = GetAssetStream(relativeToAssetsFolder);
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public static Stream GetAssetStream(string relativeToAssetsFolder)
    {
        var uri = new Uri($"avares://{AppIdentifier}/Assets/{relativeToAssetsFolder}");
        return AssetLoader.Open(uri);
    }

    public static async Task<string> GetAssetStringAsync(string relativeToAssetsFolder, Encoding? encoding = null)
    {
        var reader = new StreamReader(GetAssetStream(relativeToAssetsFolder), encoding);
        return await reader.ReadToEndAsync();
    }

    public static void ClearConfig()
    {
        TryDelete(AppSettingsPath);
        TryDelete(HomePageCardsPath);
        TryDelete(NavigationMenuPath);
    }

    public static void ClearLoginContext()
    {
        TryDelete(LoginContextPath);
        }

    public static void SaveWindowContext(Window window)
    {
        var applicationSettings = App.AppViewModel.AppSettings.ApplicationSettings;
        var isMaximized = applicationSettings.IsMaximized = window.WindowState is WindowState.Maximized;
        // 在非最大化状态下保存窗口大小，以便从最大化恢复时使用
        if (!isMaximized)
        {
            applicationSettings.WindowWidth = window.Width;
            applicationSettings.WindowHeight = window.Height;
        }
    }

    public static void SaveContext()
    {
        SaveLoginContext(App.AppViewModel.LoginContext);
        SaveSettings();
    }

    public static AppSettings? LoadAppSettings(FileLogger logger)
    {
        if (!File.Exists(AppSettingsPath))
            return null;

        try
        {
            return YamlSerializer.DeserializeFile(AppSettingsPath, SettingsSerializerContext.Default.AppSettings);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to load settings", e);
            return null;
        }
    }

    public static LoginContext? LoadLoginContext(FileLogger logger)
    {
        if (!File.Exists(LoginContextPath))
            return null;

        try
        {
            return YamlSerializer.DeserializeFile(LoginContextPath, SettingsSerializerContext.Default.LoginContext);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to load settings", e);
            return null;
    }
    }

    public static ObservableCollection<HomePageCardLayout>? LoadHomePageCards(FileLogger logger)
    {
        if (!File.Exists(HomePageCardsPath))
            return null;

        try
        {
            return YamlSerializer.DeserializeFile(HomePageCardsPath, SettingsSerializerContext.Default.HomePageCardsSettings)?.Cards;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to load home page cards", e);
            return null;
        }
    }

    public static void SaveSettings()
    {
        SaveAppSettings(App.AppViewModel.AppSettings);
        SaveHomePageCards(App.AppViewModel.HomePageCards);
    }

    public static void SaveAppSettings(AppSettings? configuration)
    {
        if (configuration is null)
            return;

        YamlSerializer.SerializeToFile(AppSettingsPath, configuration, SettingsSerializerContext.Default.AppSettings);
    }

    public static void SaveLoginContext(LoginContext? configuration)
    {
        if (configuration is null)
            return;

        YamlSerializer.SerializeToFile(LoginContextPath, configuration, SettingsSerializerContext.Default.LoginContext);
    }

    public static void SaveHomePageCards(ObservableCollection<HomePageCardLayout>? cards)
    {
        if (cards is null)
            return;

        YamlSerializer.SerializeToFile(
            HomePageCardsPath,
            new HomePageCardsSettings { Cards = cards },
            SettingsSerializerContext.Default.HomePageCardsSettings);
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignored
        }
    }
}
