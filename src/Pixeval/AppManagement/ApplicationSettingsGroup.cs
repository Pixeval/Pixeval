// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Home;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record ApplicationSettingsGroup
{
    public DateTime LastCheckedUpdate { get; set; } = DateTime.MinValue;

    public double WindowWidth { get; set; } = 800;

    public double WindowHeight { get; set; } = 600;

    public bool IsMaximized { get; set; }

    // TODO: Not Used
    [SettingsEntry(Symbol.Communication, AppSettingsResources.DownloadUpdateAutomaticallyEntryHeader, AppSettingsResources.DownloadUpdateAutomaticallyEntryDescription)]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// "" 表示使用系统默认语言
    /// </summary>
    [SettingsEntry(Symbol.LocalLanguage, AppSettingsResources.AppLanguageEntryHeader, AppSettingsResources.AppLanguageEntryDescription, AppSettingsResources.AppLanguageEntryPlaceholder, "ms-settings:regionlanguage")]
    public string CultureName { get; set; } = "";

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(Symbol.DarkTheme, AppSettingsResources.ThemeEntryHeader, AppSettingsResources.ThemeEntryDescription)]
    public ApplicationTheme Theme { get; set; }

    [SettingsEntry(Symbol.Database, AppSettingsResources.UseFileCacheEntryHeader, AppSettingsResources.UseFileCacheEntryDescription)]
    public bool UseFileCache { get; set; } = true;

    [SettingsEntry(Symbol.TextFont, AppSettingsResources.AppFontFamilyEntryHeader, AppSettingsResources.AppFontFamilyEntryDescription, AppSettingsResources.AppFontFamilyEntryPlaceholder)]
    public ObservableCollection<string> AppFontFamily { get; set; } = [];

    [SettingsEntry(Symbol.Table, AppSettingsResources.HomePageRowsEntryHeader, AppSettingsResources.HomePageRowsEntryDescription)]
    public int HomePageRows { get; set; } = 7;

    [SettingsEntry(Symbol.Table, AppSettingsResources.HomePageColumnsEntryHeader, AppSettingsResources.HomePageColumnsEntryDescription)]
    public int HomePageColumns { get; set; } = 1;

    [SettingsEntry(Symbol.WindowHeaderHorizontal, AppSettingsResources.HideHomePageToolbarEntryHeader, AppSettingsResources.HideHomePageToolbarEntryDescription)]
    public bool HideHomePageToolbar { get; set; }

    [SettingsEntry(Symbol.AppTitle, AppSettingsResources.HideHomePageCardTitleEntryHeader, AppSettingsResources.HideHomePageCardTitleEntryDescription)]
    public bool HideHomePageCardTitle { get; set; }

    public ObservableCollection<HomePageCardLayout> HomePageCards { get; set; } =
    [
        new(new(HomePageCardSourceKind.Spotlight), 0, 0, 1, 2),
        new(new(HomePageCardSourceKind.UserRecommended), 0, 2, 1, 2),
        new(new(HomePageCardSourceKind.WorkRecommended), 0, 4, 1, 3)
        {
            SimpleWorkType = SimpleWorkType.IllustrationAndManga
        }
    ];
}
