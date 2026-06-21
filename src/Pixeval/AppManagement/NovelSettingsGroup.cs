// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using Avalonia.Media;
using FluentIcons.Common;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record NovelSettingsGroup
{
    [JsonIgnore]
    public Func<ApplicationTheme> ActualThemeProvider { get; set; } = static () => ApplicationTheme.Default;

    public uint NovelFontColorInDarkMode { get; set; } = 0xFFFFFFFF;

    public uint NovelFontColorInLightMode { get; set; } = 0xFF000000;

    public uint NovelBackgroundInDarkMode { get; set; }

    public uint NovelBackgroundInLightMode { get; set; }

    [SettingsEntry(Symbol.LineThickness, AppSettingsResources.NovelSettingsFontWeightEntryHeader, AppSettingsResources.NovelSettingsFontWeightEntryDescription, AppSettingsResources.NovelSettingsFontWeightEntryPlaceholder)]
    public FontWeight NovelFontWeight { get; set; } = FontWeight.Normal;

    [SettingsEntry(Symbol.TextFont, AppSettingsResources.NovelSettingsFontFamilyEntryHeader, AppSettingsResources.AppFontFamilyEntryDescription, AppSettingsResources.AppFontFamilyEntryPlaceholder)]
    public ObservableCollection<string> NovelFontFamily { get; set; } = [];

    [SettingsEntry(Symbol.TextFontSize, AppSettingsResources.NovelSettingsFontSizeEntryHeader, AppSettingsResources.NovelSettingsFontSizeEntryDescription)]
    public int NovelFontSize { get; set; } = 14;

    [SettingsEntry(Symbol.TextLineSpacing, AppSettingsResources.NovelSettingsLineHeightEntryHeader, AppSettingsResources.NovelSettingsLineHeightEntryDescription)]
    public int NovelLineHeight { get; set; } = 28;

    [SettingsEntry(Symbol.AutoFitWidth, AppSettingsResources.NovelSettingsMaxWidthEntryHeader, AppSettingsResources.NovelSettingsMaxWidthEntryDescription)]
    public int NovelMaxWidth { get; set; } = 1000;

    [SettingsEntry(Symbol.ColorBackground, AppSettingsResources.NovelSettingsBackgroundEntryHeader, AppSettingsResources.NovelSettingsBackgroundEntryDescription)]
    public uint NovelBackground
    {
        get => ActualTheme is ApplicationTheme.Light ? NovelBackgroundInLightMode : NovelBackgroundInDarkMode;
        set
        {
            if (ActualTheme is ApplicationTheme.Light)
                NovelBackgroundInLightMode = value;
            else
                NovelBackgroundInDarkMode = value;
        }
    }

    [SettingsEntry(Symbol.TextColor, AppSettingsResources.NovelSettingsFontColorEntryHeader, AppSettingsResources.NovelSettingsFontColorEntryDescription)]
    public uint NovelFontColor
    {
        get => ActualTheme is ApplicationTheme.Light ? NovelFontColorInLightMode : NovelFontColorInDarkMode;
        set
        {
            if (ActualTheme is ApplicationTheme.Light)
                NovelFontColorInLightMode = value;
            else
                NovelFontColorInDarkMode = value;
        }
    }

    private ApplicationTheme ActualTheme => ActualThemeProvider();
}
