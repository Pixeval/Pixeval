// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Globalization;
using System.IO;
using Velopack.Locators;

namespace Pixeval.AppManagement;

public static class StoreDataMigration
{
    private const string StorePackageFamilyName = "PokerKo.4454907E5DDB5_0wpjzgvbyjvyr";
    private const string MigrationMarkerName = ".microsoft-store-migration-v1.complete";

    private static readonly string[] _SettingsFiles =
    [
        "settings.yaml",
        "login_context.yaml",
        "home_page_cards.yaml",
        "navigation_menu.yaml",
        "Pixeval5.0.0.sqlite"
    ];

    public static bool IsVelopackInstallation
    {
        get
        {
            if (!OperatingSystem.IsWindows()
                && !OperatingSystem.IsMacOS()
                && !OperatingSystem.IsLinux())
            {
                return false;
            }

            if (!VelopackLocator.IsCurrentSet)
                return false;

            try
            {
                return VelopackLocator.Current.CurrentlyInstalledVersion is not null;
            }
            catch
            {
                return false;
            }
        }
    }

    public static void TryMigrateFromMicrosoftStore()
    {
        if (!OperatingSystem.IsWindows() || !IsVelopackInstallation)
            return;

        var sourceRoot = GetStoreApplicationFolderPath();
        if (!Directory.Exists(sourceRoot))
            return;

        var targetRoot = GetClassicApplicationFolderPath();
        var markerPath = Path.Combine(targetRoot, MigrationMarkerName);
        if (File.Exists(markerPath))
            return;

        var migrationSucceeded = true;
        try
        {
            Directory.CreateDirectory(targetRoot);
            var sourceSettings = Path.Combine(sourceRoot, "Settings");
            var targetSettings = Path.Combine(targetRoot, "Settings");
            Directory.CreateDirectory(targetSettings);

            foreach (var fileName in _SettingsFiles)
            {
                if (!TryCopyIfMissing(
                        Path.Combine(sourceSettings, fileName),
                        Path.Combine(targetSettings, fileName)))
                {
                    migrationSucceeded = false;
                }
            }

            if (!TryCopyDirectoryIfPresent(
                    Path.Combine(sourceRoot, "Extensions"),
                    Path.Combine(targetRoot, "Extensions")))
            {
                migrationSucceeded = false;
            }

            if (migrationSucceeded)
            {
                File.WriteAllText(
                    markerPath,
                    DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            }
        }
        catch (Exception exception)
        {
            migrationSucceeded = false;
            TryWriteMigrationLog(targetRoot, exception);
        }

        if (!migrationSucceeded)
            TryWriteMigrationLog(targetRoot, new IOException("Microsoft Store data migration was incomplete."));
    }

    private static string GetClassicApplicationFolderPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppInfo.AppIdentifier);

    private static string GetStoreApplicationFolderPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Packages",
        StorePackageFamilyName,
        "LocalCache",
        "Local",
        AppInfo.AppIdentifier);

    private static bool TryCopyIfMissing(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath) || File.Exists(targetPath))
            return true;

        try
        {
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
                Directory.CreateDirectory(targetDirectory);
            File.Copy(sourcePath, targetPath);
            return true;
        }
        catch (Exception exception)
        {
            TryWriteMigrationLog(Path.GetDirectoryName(targetPath) ?? AppContext.BaseDirectory, exception);
            return false;
        }
    }

    private static bool TryCopyDirectoryIfPresent(string sourcePath, string targetPath)
    {
        if (!Directory.Exists(sourcePath))
            return true;

        var success = true;
        foreach (var sourceFile in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
            if (!TryCopyIfMissing(sourceFile, Path.Combine(targetPath, relativePath)))
                success = false;
        }

        return success;
    }

    private static void TryWriteMigrationLog(string targetRoot, Exception exception)
    {
        try
        {
            var logDirectory = Path.Combine(targetRoot, "Logs");
            Directory.CreateDirectory(logDirectory);
            File.AppendAllText(
                Path.Combine(logDirectory, "migration.log"),
                $"{DateTimeOffset.UtcNow:O} {exception}\r\n");
        }
        catch
        {
            // Migration must never prevent the application from starting.
        }
    }
}
