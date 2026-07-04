// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record ApplicationSettingsGroup
{
    public DateTime LastCheckedUpdate { get; set; } = DateTime.MinValue;

    public double WindowWidth { get; set; } = 800;

    public double WindowHeight { get; set; } = 600;

    public bool IsMaximized { get; set; }

    // TODO: Not Used
    [SettingsEntry(Symbol.Communication, DownloadUpdateAutomaticallyEntryHeader, DownloadUpdateAutomaticallyEntryDescription)]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// "" 表示使用系统默认语言
    /// </summary>
    [SettingsEntry(Symbol.LocalLanguage, AppLanguageEntryHeader, AppLanguageEntryDescription, AppLanguageEntryPlaceholder, "ms-settings:regionlanguage")]
    public string CultureName { get; set; } = "";

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(Symbol.DarkTheme, ThemeEntryHeader, ThemeEntryDescription)]
    public ApplicationTheme Theme { get; set; }

    [SettingsEntry(Symbol.Database, UseFileCacheEntryHeader, UseFileCacheEntryDescription)]
    public bool UseFileCache { get; set; } = true;

    [SettingsEntry(Symbol.TextFont, AppFontFamilyEntryHeader, AppFontFamilyEntryDescription, AppFontFamilyEntryPlaceholder)]
    public ObservableCollection<string> AppFontFamily { get; set; } = [];

    [SettingsEntry(Symbol.Table, HomePageRowsEntryHeader, HomePageRowsEntryDescription)]
    public int HomePageRows { get; set; } = 7;

    [SettingsEntry(Symbol.Table, HomePageColumnsEntryHeader, HomePageColumnsEntryDescription)]
    public int HomePageColumns { get; set; } = 1;

    [SettingsEntry(Symbol.WindowHeaderHorizontal, HideHomePageToolbarEntryHeader, HideHomePageToolbarEntryDescription)]
    public bool HideHomePageToolbar { get; set; }

    [SettingsEntry(Symbol.AppTitle, HideHomePageCardTitleEntryHeader, HideHomePageCardTitleEntryDescription)]
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
