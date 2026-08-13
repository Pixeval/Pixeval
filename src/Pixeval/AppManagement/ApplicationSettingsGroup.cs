// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record ApplicationSettingsGroup
{
    public DateTime LastCheckedUpdate { get; set; } = DateTime.MinValue;

    public double WindowWidth { get; set; } = 800;

    public double WindowHeight { get; set; } = 600;

    public bool IsMaximized { get; set; }

    // TODO: Not Used
    [SettingsEntry(Symbol.Communication, AppSettingsResources.DownloadUpdateAutomaticallyEntry.Header, AppSettingsResources.DownloadUpdateAutomaticallyEntry.Description)]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// "" 表示使用系统默认语言
    /// </summary>
    [SettingsEntry(Symbol.LocalLanguage, AppSettingsResources.AppLanguageEntry.Header, AppSettingsResources.AppLanguageEntry.Description, AppSettingsResources.AppLanguageEntry.Placeholder, "ms-settings:regionlanguage")]
    public string CultureName { get; set; } = "";

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(Symbol.DarkTheme, AppSettingsResources.ThemeEntry.Header, AppSettingsResources.ThemeEntry.Description)]
    public ApplicationTheme Theme { get; set; }

    [SettingsEntry(Symbol.Database, AppSettingsResources.UseFileCacheEntry.Header, AppSettingsResources.UseFileCacheEntry.Description)]
    public bool UseFileCache { get; set; } = true;

    [SettingsEntry(Symbol.DatabaseLightning, AppSettingsResources.LimitFileCacheSizeEntry.Header, AppSettingsResources.LimitFileCacheSizeEntry.Description)]
    public bool LimitFileCacheSize { get; set; }

    [SettingsEntry(Symbol.HardDrive, AppSettingsResources.FileCacheSizeLimitInMegabytesEntry.Header,
        AppSettingsResources.FileCacheSizeLimitInMegabytesEntry.Description)]
    public int FileCacheSizeLimitInMegabytes { get; set; } = 2048;

    [SettingsEntry(Symbol.TextFont, AppSettingsResources.AppFontFamilyEntry.Header, AppSettingsResources.AppFontFamilyEntry.Description, AppSettingsResources.AppFontFamilyEntry.Placeholder)]
    public ObservableCollection<string> AppFontFamily { get; set; } = [];

    [SettingsEntry(Symbol.Table, AppSettingsResources.HomePageRowsEntry.Header, AppSettingsResources.HomePageRowsEntry.Description)]
    public int HomePageRows { get; set; } = 7;

    [SettingsEntry(Symbol.Table, AppSettingsResources.HomePageColumnsEntry.Header, AppSettingsResources.HomePageColumnsEntry.Description)]
    public int HomePageColumns { get; set; } = 1;

    [SettingsEntry(Symbol.WindowHeaderHorizontal, AppSettingsResources.HideHomePageToolbarEntry.Header, AppSettingsResources.HideHomePageToolbarEntry.Description)]
    public bool HideHomePageToolbar { get; set; }

    [SettingsEntry(Symbol.AppTitle, AppSettingsResources.HideHomePageCardTitleEntry.Header, AppSettingsResources.HideHomePageCardTitleEntry.Description)]
    public bool HideHomePageCardTitle { get; set; }

    [SettingsEntry(Symbol.Blur, AppSettingsResources.UseMicaEntry.Header, AppSettingsResources.UseMicaEntry.Description)]
    public bool UseMica { get; set; } = true;
}
