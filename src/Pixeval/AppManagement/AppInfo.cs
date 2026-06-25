// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
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

    public static string DatabaseFilePath { get; } = Path.Combine(SettingsFolder, "PixevalData4.3.12.sqlite");

    public static readonly Uri IconApplicationUri = new($"avares://{AppIdentifier}/Assets/logo.ico");

    public static readonly Uri SvgIconApplicationUri = new($"avares://{AppIdentifier}/Assets/logo.svg");

    public static Versioning AppVersion { get; } = new();

    static AppInfo()
    {
        if (Directory.Exists(TempFolder))
            Directory.Delete(TempFolder, true);
        // Ensure directories exist
        Directory.CreateDirectory(ApplicationFolderPath);
        Directory.CreateDirectory(SettingsFolder);
        Directory.CreateDirectory(CacheFolder);
        Directory.CreateDirectory(LogsFolder);
        Directory.CreateDirectory(TempFolder);
        Directory.CreateDirectory(ExtensionsFolder);
    }

    public const string ImageNotAvailablePath = $"avares://{AppIdentifier}/Assets/image-not-available.png";

    public const string PixivNoProfilePath = $"avares://{AppIdentifier}/Assets/pixiv_no_profile.png";

    public static Stream GetImageNotAvailableStream() => AssetLoader.Open(new Uri(ImageNotAvailablePath));

    public static Stream GetPixivNoProfileStream() => AssetLoader.Open(new Uri(PixivNoProfilePath));

    /// <summary>
    /// Get the byte array of a file in the Assets folder
    /// </summary>
    /// <param name="relativeToAssetsFolder">A path with leading slash(or backslash) removed</param>
    /// <returns></returns>
    public static Task<byte[]> GetAssetBytesAsync(string relativeToAssetsFolder)
    {
        return GetResourceBytesAsync(relativeToAssetsFolder);
    }

    public static Stream GetAssetStream(string relativeToAssetsFolder)
    {
        var uri = new Uri($"avares://{AppIdentifier}/Assets/{relativeToAssetsFolder}");
        return AssetLoader.Open(uri);
    }

    public static async Task<byte[]> GetResourceBytesAsync(string relativeToAssetsFolder)
    {
        var uri = new Uri($"avares://{AppIdentifier}/Assets/{relativeToAssetsFolder}");
        await using var stream = AssetLoader.Open(uri);
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public static void ClearConfig()
    {
        try
        {
            File.Delete(AppSettingsPath);
        }
        catch
        {
            // ignored
        }
    }

    public static void ClearLoginContext()
    {
        try
        {
            File.Delete(LoginContextPath);
        }
        catch
        {
            // ignored
        }
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
        SaveSettings(App.AppViewModel.AppSettings);
    }

    public static AppSettings? LoadConfig(FileLogger logger)
    {
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

    public static void SaveSettings(AppSettings? configuration)
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
}
