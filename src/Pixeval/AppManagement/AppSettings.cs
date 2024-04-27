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
using Windows.Foundation;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor, SettingPoco]
public partial record AppSettings : IWindowSettings
{
    public AppSettings()
    {
    }

    [SettingMetadata(SettingEntryCategory.Version, typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader))]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.ThemeEntryHeader))]
    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.BackdropEntryHeader))]
    public BackdropType Backdrop { get; set; } = MicaController.IsSupported() ? BackdropType.MicaAlt : DesktopAcrylicController.IsSupported() ? BackdropType.Acrylic : BackdropType.None;

    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.EnableDomainFrontingEntryHeader))]
    public bool EnableDomainFronting { get; set; } = true;

    /// <summary>
    /// Indicates whether a <see cref="TeachingTip" /> should be displayed
    /// when user clicks "Generate Link"
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.GenerateHelpLinkEntryHeader))]
    public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; } = true;

    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.UseFileCacheEntryHeader))]
    public bool UseFileCache { get; set; }

    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.AppFontFamilyEntryHeader))]
    public string AppFontFamilyName { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSelectedTabEntryHeader))]
    public MainPageTabItem DefaultSelectedTabItem { get; set; } = MainPageTabItem.Recommendation;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultDownloadPathMacroEntryHeader))]
    public string DefaultDownloadPathMacro { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create)
        + @"\@{if_manga=[@{artist_name}] @{title}}\[@{artist_name}] @{id}@{if_manga=p@{manga_index}}@{ext}";

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.WorkDownloadFormatEntryHeader))]
    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.WorkDownloadFormatEntryHeader))]
    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.WorkDownloadFormatEntryHeader))]
    public NovelDownloadFormat NovelDownloadFormat { get; set; } = NovelDownloadFormat.Pdf;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.OverwriteDownloadedFileEntryHeader))]
    public bool OverwriteDownloadedFile { get; set; }

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader))]
    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader))]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 2;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadWhenBookmarkedEntryHeader))]
    public bool DownloadWhenBookmarked { get; set; }

    /// <summary>
    /// The application-wide default sort option, any illustration page that supports
    /// different orders will use this as its default value
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchSortOptionEntryHeader))]
    public WorkSortOption WorkSortOption { get; set; } = WorkSortOption.DoNotSort;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public SimpleWorkType SimpleWorkType { get; set; } = SimpleWorkType.IllustAndManga;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public RankOption IllustrationRankOption { get; set; } = RankOption.Day;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public RankOption NovelRankOption { get; set; } = RankOption.Day;

    /// <summary>
    /// The illustration tag match option for keyword search
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public SearchIllustrationTagMatchOption SearchIllustrationTagMatchOption { get; set; } = SearchIllustrationTagMatchOption.PartialMatchForTags;

    /// <summary>
    /// The novel tag match option for keyword search
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public SearchNovelTagMatchOption SearchNovelTagMatchOption { get; set; } = SearchNovelTagMatchOption.PartialMatchForTags;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchHistoryRecordsEntryHeader))]
    public int MaximumSearchHistoryRecords { get; set; } = 50;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.SearchDurationEntryHeader))]
    public SearchDuration SearchDuration { get; set; } = SearchDuration.Undecided;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.UsePreciseRangeForSearchEntryHeader))]
    public bool UsePreciseRangeForSearch { get; set; }

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchApiKeyEntryHeader))]
    public string? ReverseSearchApiKey { get; set; }

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryHeader))]
    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryHeader))]
    public int MaximumSuggestionBoxSearchHistory { get; set; } = 10;

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.TargetAPIPlatformEntryHeader))]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.ThumbnailDirectionEntryHeader))]
    public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection.Portrait;

    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.ItemsViewLayoutTypeEntryHeader))]
    public ItemsViewLayoutType ItemsViewLayoutType { get; set; } = ItemsViewLayoutType.LinedFlow;

    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.BlockedTagsEntryHeader))]
    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    public HashSet<string> BlockedTags { get; set; } = [];

    /// <summary>
    /// Indicates the maximum page count that are allowed to be retrieved during
    /// spotlight retrieval(10 entries per page)
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.SpotlightSearchPageLimitEntryHeader))]
    public int PageLimitForSpotlight { get; set; } = 50;

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.ImageMirrorServerEntryHeader))]
    public string? MirrorHost { get; set; } = null;

    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumBrowseHistoryRecordsEntryHeader))]
    public int MaximumBrowseHistoryRecords { get; set; } = 100;

    [SyntheticSetting]
    public DateTimeOffset SearchStartDate { get; set; } = DateTimeOffset.Now - TimeSpan.FromDays(1);

    [SyntheticSetting]
    public DateTimeOffset SearchEndDate { get; set; } = DateTimeOffset.Now;

    [SyntheticSetting]
    public DateTimeOffset LastCheckedUpdate { get; set; } = DateTimeOffset.MinValue;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivWebApiNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivAccountNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivOAuthNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivAppApiNameResolver { get; set; } =
    [
        "210.140.131.199",
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivImageNameResolver { get; set; } =
    [
        "210.140.92.144",
        "210.140.92.141",
        "210.140.92.142",
        "210.140.92.143"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivImageNameResolver2 { get; set; } =
    [
        "210.140.92.143",
        "210.140.92.141",
        "210.140.92.142"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public bool ShowRecommendIllustratorsInIllustratorContentViewer { get; set; } = true;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public bool ShowExternalCommandBarInIllustratorContentViewer { get; set; } = true;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string TagsManagerWorkingPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public uint NovelFontColorInDarkMode { get; set; } = 0xFFFFFFFF;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public uint NovelFontColorInLightMode { get; set; } = 0xFF000000;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public uint NovelBackgroundInDarkMode { get; set; } = 0;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public uint NovelBackgroundInLightMode { get; set; } = 0;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public FontWeightsOption NovelFontWeight { get; set; } = FontWeightsOption.Normal;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string NovelFontFamily { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public double NovelFontSize { get; set; } = 14;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public double NovelLineHeight { get; set; } = 28;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public double NovelMaxWidth { get; set; } = 1000;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public Size WindowSize { get; set; } = WindowHelper.EstimatedWindowSize().ToSize();

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public bool IsMaximized { get; set; } = false;

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>), typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    public WorkType WorkType => SimpleWorkType is SimpleWorkType.IllustAndManga ? WorkType.Illust : WorkType.Novel;

    public MakoClientConfiguration ToMakoClientConfiguration()
    {
        return new MakoClientConfiguration(5000, EnableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
    }
}
