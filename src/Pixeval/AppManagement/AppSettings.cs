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
using System.Globalization;
using Windows.Foundation;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
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

    /// <summary>
    /// Disable the domain fronting technology, once disabled, the users
    /// from China mainland are required to have other countermeasures to bypass
    /// GFW
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.DisableDomainFrontingEntryHeader))]
    public bool DisableDomainFronting { get; set; }

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
    public MainPageTabItem DefaultSelectedTabItem { get; set; } = MainPageTabItem.DailyRecommendation;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultDownloadPathMacroEntryHeader))]
    public string DefaultDownloadPathMacro { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create)
        + @"\@{if_spot=@{spot_title}}\@{if_manga=[@{artist_name}] @{illust_title}}\[@{artist_name}] @{illust_id}@{if_manga=p@{manga_index}}@{illust_ext}";

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.UgoiraDownloadFormatEntryHeader))]
    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.IllustrationDownloadFormatEntryHeader))]
    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.OverwriteDownloadedFileEntryHeader))]
    public bool OverwriteDownloadedFile { get; set; }

    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader))]
    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader))]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 2;

    /// <summary>
    /// The application-wide default sort option, any illustration page that supports
    /// different orders will use this as its default value
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchSortOptionEntryHeader))]
    public IllustrationSortOption DefaultSortOption { get; set; } = IllustrationSortOption.DoNotSort;

    /// <summary>
    /// The tag match option for keyword search
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public SearchIllustrationTagMatchOption TagMatchOption { get; set; } = SearchIllustrationTagMatchOption.PartialMatchForTags;

    /// <summary>
    /// Indicates the starting page's number of keyword search
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.SearchStartsFromEntryHeader))]
    public int SearchStartingFromPageNumber { get; set; } = 1;

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
    /// Indicates the maximum page count that are allowed to be retrieved during
    /// keyword search(30 entries per page)
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchPageLimitHeader))]
    public int PageLimitForKeywordSearch { get; set; } = 100;

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.TargetAPIPlatformEntryHeader))]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.ThumbnailDirectionEntryHeader))]
    public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection.Portrait;

    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.ItemsViewLayoutTypeEntryHeader))]
    public ItemsViewLayoutType ItemsViewLayoutType { get; set; } = ItemsViewLayoutType.LinedFlow;

    /// <summary>
    /// Indicates whether the restricted content are permitted to be included
    /// in the searching results, including R-18 and R-18G
    /// </summary>
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.FiltrateRestrictedContentEntryHeader))]
    public bool FiltrateRestrictedContent { get; set; }

    /// <summary>
    /// How many rows to be preloaded in illustration grid
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.PreloadRowsEntryHeader))]
    public int PreLoadRows { get; set; } = 2;

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

    /// <summary>
    /// Indicates how many illustrations will be collected during
    /// the enumeration of the <see cref="MakoClient.RecommendationIllustrations" />
    /// </summary>
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.RecommendationItemLimitEntryHeader))]
    public int ItemsNumberLimitForDailyRecommendations { get; set; } = 500;

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
    public string[] PixivApiNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string[] PixivImageNameResolver { get; set; } =
    [
        "210.140.92.141",
        "210.140.92.142",
        "210.140.92.143"
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
    public string UserName { get; set; } = "";

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public string Password { get; set; } = "";

    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SyntheticSetting]
    public Size WindowSize { get; set; } = WindowHelper.EstimatedWindowSize().ToSize();

    public MakoClientConfiguration ToMakoClientConfiguration()
    {
        return new MakoClientConfiguration(5000, !DisableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
    }
}
