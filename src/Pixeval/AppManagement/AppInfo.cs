// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
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

    private static string NavigationMenuPath { get; } = Path.Combine(SettingsFolder, "navigation_menu.yaml");

    public static string DatabaseFilePath { get; } = Path.Combine(SettingsFolder, "Pixeval5.0.0.sqlite");

    public static readonly Uri IconApplicationUri = new($"avares://{AppIdentifier}/Assets/logo.ico");

    public static readonly Uri SvgIconApplicationUri = new($"avares://{AppIdentifier}/Assets/logo.svg");

    public static Versioning AppVersion { get; } = new();

    static AppInfo()
    {
        _ = FileHelper.TryDeleteDirectory(TempFolder);
        // Ensure directories exist
        _ = FileHelper.TryCreateDirectory(ApplicationFolderPath);
        _ = FileHelper.TryCreateDirectory(SettingsFolder);
        _ = FileHelper.TryCreateDirectory(CacheFolder);
        _ = FileHelper.TryCreateDirectory(LogsFolder);
        _ = FileHelper.TryCreateDirectory(TempFolder);
        _ = FileHelper.TryCreateDirectory(ExtensionsFolder);
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


        return TryLoad(() => YamlSerializer.DeserializeFile(AppSettingsPath, SettingsSerializerContext.Default.AppSettings), logger);
    }

    public static LoginContext? LoadLoginContext(FileLogger logger)
    {
        if (!File.Exists(LoginContextPath))
            return null;

        return TryLoad(() => YamlSerializer.DeserializeFile(LoginContextPath, SettingsSerializerContext.Default.LoginContext), logger);
    }

    public static ObservableCollection<HomePageCardLayout>? LoadHomePageCards(FileLogger logger)
    {
        if (!File.Exists(HomePageCardsPath))
            return null;

        return TryLoad(() => YamlSerializer.DeserializeFile(HomePageCardsPath, SettingsSerializerContext.Default.ObservableCollectionHomePageCardLayout), logger);
    }

    public static string? LoadNavigationMenuYaml(FileLogger logger)
    {
        if (!File.Exists(NavigationMenuPath))
            return null;

        return TryLoad(() => File.ReadAllText(NavigationMenuPath), logger);
    }

    public static void SaveSettings()
    {
        SaveAppSettings(App.AppViewModel.AppSettings);
        SaveHomePageCards(App.AppViewModel.HomePageCards);
        SaveNavigationMenuYaml(App.AppViewModel.NavigationMenuYamlText);
    }

    public static void SaveAppSettings(AppSettings? appSettings)
    {
        if (appSettings is null)
            return;

        _ = TrySave(() =>
            YamlSerializer.SerializeToFile(AppSettingsPath, appSettings, SettingsSerializerContext.Default.AppSettings));
    }

    public static void SaveLoginContext(LoginContext? loginContext)
    {
        if (loginContext is null)
            return;

        _ = TrySave(() =>
            YamlSerializer.SerializeToFile(LoginContextPath, loginContext, SettingsSerializerContext.Default.LoginContext));
    }

    public static void SaveHomePageCards(ObservableCollection<HomePageCardLayout>? cards)
    {
        if (cards is null)
            return;

        _ = TrySave(() =>
            YamlSerializer.SerializeToFile(HomePageCardsPath, cards, SettingsSerializerContext.Default.ObservableCollectionHomePageCardLayout));
    }

    public static void SaveNavigationMenuYaml(string? yaml)
    {
        if (yaml is null)
            return;

        _ = TrySave(() =>
            File.WriteAllText(NavigationMenuPath, yaml.ReplaceLineEndings(Environment.NewLine), Encoding.UTF8));
    }

    private static T? TryLoad<T>(Func<T> load, FileLogger logger, [CallerMemberName] string? callerName = null)
    {
        try
        {
            return load();
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to {callerName}", e);
            return default;
        }
    }

    private static bool TrySave(Action save)
    {
        try
        {
            save();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
