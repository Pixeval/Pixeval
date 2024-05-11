#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppSetting.cs
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Windows.Foundation;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Options;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using WinUI3Utilities.Controls;
using Windows.Globalization;
using static Pixeval.SettingsPageResources;

namespace Pixeval.AppManagement;

[GenerateConstructor, Reset]
public partial record AppSettings : IWindowSettings
{
    public AppSettings()
    {
    }

    [SettingsEntry(IconGlyph.CommunicationsE95A, nameof(DownloadUpdateAutomaticallyEntryHeader), nameof(DownloadUpdateAutomaticallyEntryDescription))]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(IconGlyph.PersonalizeE771, nameof(ThemeEntryHeader), nameof(ThemeEntryDescriptionHyperlinkButtonContent))]
    public ElementTheme Theme { get; set; }

    [SettingsEntry(IconGlyph.ColorE790, nameof(BackdropEntryHeader), null)]
    public BackdropType Backdrop { get; set; } = MicaController.IsSupported() ? BackdropType.MicaAlt : DesktopAcrylicController.IsSupported() ? BackdropType.Acrylic : BackdropType.None;

    [SettingsEntry(IconGlyph.NetworkE968, nameof(EnableDomainFrontingEntryHeader), nameof(EnableDomainFrontingEntryDescription))]
    public bool EnableDomainFronting { get; set; } = true;

    /// <summary>
    /// Indicates whether a <see cref="TeachingTip" /> should be displayed
    /// when user clicks "Generate Link"
    /// </summary>
    [SettingsEntry(IconGlyph.LinkE71B, nameof(GenerateHelpLinkEntryHeader), nameof(GenerateHelpLinkEntryDescription))]
    public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; } = true;

    [SettingsEntry(IconGlyph.FileExplorerEC50, nameof(UseFileCacheEntryHeader), nameof(UseFileCacheEntryDescription))]
    public bool UseFileCache { get; set; }

    [SettingsEntry(IconGlyph.FontE8D2, nameof(AppFontFamilyEntryHeader), nameof(OpenFontSettingsHyperlinkButtonContent))]
    public string AppFontFamilyName { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [SettingsEntry(IconGlyph.CheckMarkE73E, nameof(DefaultSelectedTabEntryHeader), nameof(DefaultSelectedTabEntryDescription))]
    public MainPageTabItem DefaultSelectedTabItem { get; set; }

    [SettingsEntry(IconGlyph.RenameE8AC, nameof(DownloadPathMacroEntryHeader), nameof(DownloadPathMacroEntryDescription))]
    public string DownloadPathMacro { get; set; } = GetSpecialFolder() + @"\@{if_manga=[@{artist_name}] @{title}}\[@{artist_name}] @{id}@{if_manga=p@{manga_index}}@{ext}";

    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    public NovelDownloadFormat NovelDownloadFormat { get; set; }

    [SettingsEntry(IconGlyph.ScanE8FE, nameof(OverwriteDownloadedFileEntryHeader), nameof(OverwriteDownloadedFileEntryDescription))]
    public bool OverwriteDownloadedFile { get; set; }

    [SettingsEntry(IconGlyph.HistoryE81C, nameof(MaximumDownloadHistoryRecordsEntryHeader), nameof(MaximumDownloadHistoryRecordsEntryDescription))]
    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingsEntry(IconGlyph.LightningBoltE945, nameof(MaxDownloadConcurrencyLevelEntryHeader), nameof(MaxDownloadConcurrencyLevelEntryDescription))]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 2;

    [SettingsEntry(IconGlyph.SaveLocalE78C, nameof(DownloadWhenBookmarkedEntryHeader), nameof(DownloadWhenBookmarkedEntryDescription))]
    public bool DownloadWhenBookmarked { get; set; }

    /// <summary>
    /// The application-wide default sort option, any illustration page that supports
    /// different orders will use this as its default value
    /// </summary>
    [SettingsEntry(IconGlyph.SortE8CB, nameof(DefaultSearchSortOptionEntryHeader), nameof(DefaultSearchSortOptionEntryDescription))]
    public WorkSortOption WorkSortOption { get; set; }

    [SettingsEntry(IconGlyph.ViewAllE8A9, nameof(DefaultSearchTagMatchOptionEntryHeader), nameof(DefaultSearchTagMatchOptionEntryDescription))]
    public SimpleWorkType SimpleWorkType { get; set; }

    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    /// <summary>
    /// The illustration tag match option for keyword search
    /// </summary>
    public SearchIllustrationTagMatchOption SearchIllustrationTagMatchOption { get; set; } = SearchIllustrationTagMatchOption.PartialMatchForTags;

    /// <summary>
    /// The novel tag match option for keyword search
    /// </summary>
    public SearchNovelTagMatchOption SearchNovelTagMatchOption { get; set; } = SearchNovelTagMatchOption.PartialMatchForTags;

    [SettingsEntry(IconGlyph.HistoryE81C, nameof(MaximumSearchHistoryRecordsEntryHeader), nameof(MaximumSearchHistoryRecordsEntryDescription))]
    public int MaximumSearchHistoryRecords { get; set; } = 50;

    [SettingsEntry(IconGlyph.EaseOfAccessE776, nameof(SearchDurationEntryHeader), nameof(SearchDurationEntryDescription))]
    public SearchDuration SearchDuration { get; set; } = SearchDuration.Undecided;

    [SettingsEntry(IconGlyph.StopwatchE916, nameof(UsePreciseRangeForSearchEntryHeader), nameof(UsePreciseRangeForSearchEntryDescription))]
    public bool UsePreciseRangeForSearch { get; set; }

    [SettingsEntry(IconGlyph.SearchAndAppsE773, nameof(ReverseSearchApiKeyEntryHeader), nameof(ReverseSearchApiKeyEntryDescriptionHyperlinkButtonContent))]
    public string? ReverseSearchApiKey { get; set; }

    [SettingsEntry(IconGlyph.FilterE71C, nameof(ReverseSearchResultSimilarityThresholdEntryHeader), nameof(ReverseSearchResultSimilarityThresholdEntryDescription))]
    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    [SettingsEntry(IconGlyph.SetHistoryStatus2F739, nameof(MaximumSuggestionBoxSearchHistoryEntryHeader), nameof(MaximumSuggestionBoxSearchHistoryEntryDescription))]
    public int MaximumSuggestionBoxSearchHistory { get; set; } = 10;

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(IconGlyph.CommandPromptE756, nameof(TargetAPIPlatformEntryHeader), nameof(TargetAPIPlatformEntryDescription))]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(IconGlyph.TypeE97C, nameof(ThumbnailDirectionEntryHeader), nameof(ThumbnailDirectionEntryDescription))]
    public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection.Portrait;

    [SettingsEntry(IconGlyph.TilesECA5, nameof(ItemsViewLayoutTypeEntryHeader), nameof(ItemsViewLayoutTypeEntryDescription))]
    public ItemsViewLayoutType ItemsViewLayoutType { get; set; } = ItemsViewLayoutType.LinedFlow;

    [SettingsEntry(IconGlyph.Blocked2ECE4, nameof(BlockedTagsEntryHeader), nameof(BlockedTagsEntryDescription))]
    public HashSet<string> BlockedTags { get; set; } = [];

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(IconGlyph.HardDriveEDA2, nameof(ImageMirrorServerEntryHeader), nameof(ImageMirrorServerEntryDescription))]
    public string? MirrorHost { get; set; } = null;

    [SettingsEntry(IconGlyph.HistoryE81C, nameof(MaximumBrowseHistoryRecordsEntryHeader), nameof(MaximumBrowseHistoryRecordsEntryDescription))]
    public int MaximumBrowseHistoryRecords { get; set; } = 100;

    public DateTimeOffset SearchStartDate { get; set; } = DateTimeOffset.Now - TimeSpan.FromDays(1);

    public DateTimeOffset SearchEndDate { get; set; } = DateTimeOffset.Now;

    [SettingsEntry(IconGlyph.FitPageE9A6, nameof(BrowserOriginalImageEntryHeader), nameof(BrowserOriginalImageEntryDescription))]
    public bool BrowserOriginalImage { get; set; }

    [AttributeIgnore(typeof(ResetAttribute))]
    public DateTimeOffset LastCheckedUpdate { get; set; } = DateTimeOffset.MinValue;

    public string[] PixivWebApiNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivAccountNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivOAuthNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivAppApiNameResolver { get; set; } =
    [
        "210.140.131.199",
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivImageNameResolver { get; set; } =
    [
        "210.140.92.144",
        "210.140.92.141",
        "210.140.92.142",
        "210.140.92.143"
    ];

    public string[] PixivImageNameResolver2 { get; set; } =
    [
        "210.140.92.143",
        "210.140.92.141",
        "210.140.92.142"
    ];

    public string TagsManagerWorkingPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

    public uint NovelFontColorInDarkMode { get; set; } = 0xFFFFFFFF;

    public uint NovelFontColorInLightMode { get; set; } = 0xFF000000;

    public uint NovelBackgroundInDarkMode { get; set; }

    public uint NovelBackgroundInLightMode { get; set; }

    [SettingsEntry(IconGlyph.BrushSizeEDA8, nameof(NovelSettingsFontWeightEntryHeader), nameof(NovelSettingsFontWeightEntryDescription))]
    public FontWeightsOption NovelFontWeight { get; set; } = FontWeightsOption.Normal;

    [SettingsEntry(IconGlyph.FontE8D2, nameof(NovelSettingsFontFamilyEntryHeader), nameof(OpenFontSettingsHyperlinkButtonContent))]
    public string NovelFontFamily { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [SettingsEntry(IconGlyph.FontSizeE8E9, nameof(NovelSettingsFontSizeEntryHeader), nameof(NovelSettingsFontSizeEntryDescription))]
    public int NovelFontSize { get; set; } = 14;

    [SettingsEntry(IconGlyph.ListEA37, nameof(NovelSettingsLineHeightEntryHeader), nameof(NovelSettingsLineHeightEntryDescription))]
    public int NovelLineHeight { get; set; } = 28;

    [SettingsEntry(IconGlyph.PageMarginLandscapeWideF57A, nameof(NovelSettingsMaxWidthEntryHeader), nameof(NovelSettingsMaxWidthEntryDescription))]
    public int NovelMaxWidth { get; set; } = 1000;

    public Size WindowSize { get; set; } = WindowHelper.EstimatedWindowSize().ToSize();

    public bool IsMaximized { get; set; }

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    public WorkType WorkType => SimpleWorkType is SimpleWorkType.IllustAndManga ? WorkType.Illust : WorkType.Novel;

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    public ElementTheme ActualTheme => Theme is ElementTheme.Default
        ? AppHelper.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light
        : Theme;

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    [SettingsEntry(IconGlyph.ColorE790, nameof(NovelSettingsBackgroundEntryHeader), nameof(NovelSettingsBackgroundEntryDescription))]
    public uint NovelBackground
    {
        get => ActualTheme is ElementTheme.Light ? NovelBackgroundInLightMode : NovelBackgroundInDarkMode;
        set
        {
            if (ActualTheme is ElementTheme.Light)
                NovelBackgroundInLightMode = value;
            else
                NovelBackgroundInDarkMode = value;
        }
    }

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    [SettingsEntry(IconGlyph.FontColorE8D3, nameof(NovelSettingsFontColorEntryHeader), nameof(NovelSettingsFontColorEntryDescription))]
    public uint NovelFontColor
    {
        get => ActualTheme is ElementTheme.Light ? NovelFontColorInLightMode : NovelFontColorInDarkMode;
        set
        {
            if (ActualTheme is ElementTheme.Light)
                NovelFontColorInLightMode = value;
            else
                NovelFontColorInDarkMode = value;
        }
    }

    public MakoClientConfiguration ToMakoClientConfiguration()
    {
        var language = ApplicationLanguages.PrimaryLanguageOverride;
        if (string.IsNullOrEmpty(language))
            language = CultureInfo.CurrentUICulture.Name;
        return new MakoClientConfiguration(5000, EnableDomainFronting, MirrorHost, CultureInfo.GetCultureInfo(language));
    }

    private static string GetSpecialFolder()
    {
        var picPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
        var picDirectory = Path.GetDirectoryName(picPath);
        return picDirectory == Path.GetDirectoryName(docPath)
            ? picDirectory +
              @$"\@{{if_illust={Path.GetFileName(picPath)}}}@{{if_novel={Path.GetFileName(docPath)}}}"
            : @$"\@{{if_illust={picPath}}}@{{if_novel={docPath}}}";
    }
}
