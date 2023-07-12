#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/AppSetting.cs
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
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Misc;
using Pixeval.Options;
using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor, SettingPOCO]
public partial record AppSetting
{
#pragma warning disable CS8618
    public AppSetting()
#pragma warning restore CS8618
    {
        DefaultValueAttributeHelper.Initialize(this);
    }

    /// <summary>
    ///     The Application Theme
    /// </summary>
    [DefaultValue(AppTheme.SystemDefault)]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.ThemeEntryHeader))]
    public AppTheme Theme { get; set; }

    /// <summary>
    ///     Indicates whether the restricted content are permitted to be included
    ///     in the searching results, including R-18 and R-18G
    /// </summary>
    [DefaultValue(false)]
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.FiltrateRestrictedContentEntryHeader))]
    public bool FiltrateRestrictedContent { get; set; }

    /// <summary>
    ///     Disable the domain fronting technology, once disabled, the users
    ///     from China mainland are required to have other countermeasures to bypass
    ///     GFW
    /// </summary>
    [DefaultValue(false)]
    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.DisableDomainFrontingEntryHeader))]
    public bool DisableDomainFronting { get; set; }

    /// <summary>
    ///     The application-wide default sort option, any illustration page that supports
    ///     different orders will use this as its default value
    /// </summary>
    [DefaultValue(IllustrationSortOption.DoNotSort)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchSortOptionEntryHeader))]
    public IllustrationSortOption DefaultSortOption { get; set; }

    /// <summary>
    ///     The tag match option for keyword search
    /// </summary>
    [DefaultValue(SearchTagMatchOption.PartialMatchForTags)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    public SearchTagMatchOption TagMatchOption { get; set; }

    /// <summary>
    ///     The target filter that indicates the type of the client
    /// </summary>
    [DefaultValue(TargetFilter.ForAndroid)]
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.TargetAPIPlatformEntryHeader))]
    public TargetFilter TargetFilter { get; set; }

    /// <summary>
    ///     How many rows to be preloaded in illustration grid
    /// </summary>
    [DefaultValue(2)]
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.PreloadRowsEntryHeader))]
    public int PreLoadRows { get; set; }

    /// <summary>
    ///     Indicates the maximum page count that are allowed to be retrieved during
    ///     keyword search(30 entries per page)
    /// </summary>
    [DefaultValue(100)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchPageLimitHeader))]
    public int PageLimitForKeywordSearch { get; set; }

    /// <summary>
    ///     Indicates the starting page's number of keyword search
    /// </summary>
    [DefaultValue(1)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.SearchStartsFromEntryHeader))]
    public int SearchStartingFromPageNumber { get; set; }

    /// <summary>
    ///     Indicates the maximum page count that are allowed to be retrieved during
    ///     spotlight retrieval(10 entries per page)
    /// </summary>
    [DefaultValue(50)]
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.SpotlightSearchPageLimitEntryHeader))]
    public int PageLimitForSpotlight { get; set; }

    /// <summary>
    ///     The mirror host for image server, Pixeval will do a simple substitution that
    ///     changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [DefaultValue(null)]
    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.ImageMirrorServerEntryHeader))]
    public string? MirrorHost { get; set; }

    /// <summary>
    ///     The max download tasks that are allowed to run concurrently
    /// </summary>
    [DefaultValue(typeof(DownloadConcurrencyDefaultValueProvider))]
    [AttributeIgnore(typeof(SettingsViewModelAttribute<>))]
    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader))]
    public int MaxDownloadTaskConcurrencyLevel { get; set; }

    /// <summary>
    ///     Indicates whether a <see cref="TeachingTip" /> should be displayed
    ///     when user clicks "Generate Link"
    /// </summary>
    [DefaultValue(true)]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.GenerateHelpLinkEntryHeader))]
    public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; }

    /// <summary>
    ///     Indicates how many illustrations will be collected during
    ///     the enumeration of the <see cref="MakoClient.Recommendations" />
    /// </summary>
    [DefaultValue(500)]
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.RecommendationItemLimitEntryHeader))]
    public int ItemsNumberLimitForDailyRecommendations { get; set; }

    [DefaultValue(false)]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.UseFileCacheEntryHeader))]
    public bool UseFileCache { get; set; }

    [DefaultValue(typeof(AppWidthDefaultValueProvider))]
    [SyntheticSetting]
    public int WindowWidth { get; set; }

    [DefaultValue(typeof(AppHeightDefaultValueProvider))]
    [SyntheticSetting]
    public int WindowHeight { get; set; }

    [DefaultValue(ThumbnailDirection.Portrait)]
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.ThumbnailDirectionEntryHeader))]
    public ThumbnailDirection ThumbnailDirection { get; set; }

    [DefaultValue(typeof(MinDateTimeOffSetDefaultValueProvider))]
    [SyntheticSetting]
    public DateTimeOffset LastCheckedUpdate { get; set; }

    [DefaultValue(false)]
    [SettingMetadata(SettingEntryCategory.Version, typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader))]
    public bool DownloadUpdateAutomatically { get; set; }

    [DefaultValue("Microsoft YaHei")]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.AppFontFamilyEntryHeader))]
    public string AppFontFamilyName { get; set; }

    [DefaultValue(MainPageTabItem.DailyRecommendation)]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSelectedTabEntryHeader))]
    public MainPageTabItem DefaultSelectedTabItem { get; set; }

    [DefaultValue(SearchDuration.Undecided)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.SearchDurationEntryHeader))]
    public SearchDuration SearchDuration { get; set; }

    [DefaultValue(false)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.UsePreciseRangeForSearchEntryHeader))]
    public bool UsePreciseRangeForSearch { get; set; }

    [DefaultValue(typeof(DecrementedDateTimeOffSetDefaultValueProvider))]
    [SyntheticSetting]
    public DateTimeOffset SearchStartDate { get; set; }

    [DefaultValue(typeof(CurrentDateTimeOffSetDefaultValueProvider))]
    [SyntheticSetting]
    public DateTimeOffset SearchEndDate { get; set; }

    [DefaultValue(typeof(DownloadPathMacroDefaultValueProvider))]
    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultDownloadPathMacroEntryHeader))]
    public string DefaultDownloadPathMacro { get; set; }

    [DefaultValue(false)]
    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.OverwriteDownloadedFileEntryHeader))]
    public bool OverwriteDownloadedFile { get; set; }

    [DefaultValue(100)]
    [SettingMetadata(SettingEntryCategory.Download, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader))]
    public int MaximumDownloadHistoryRecords { get; set; }

    [DefaultValue(50)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchHistoryRecordsEntryHeader))]
    public int MaximumSearchHistoryRecords { get; set; }

    [DefaultValue(100)]
    [SettingMetadata(SettingEntryCategory.Misc, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumBrowseHistoryRecordsEntryHeader))]
    public int MaximumBrowseHistoryRecords { get; set; }

    [DefaultValue(null)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchApiKeyEntryHeader))]
    public string? ReverseSearchApiKey { get; set; }

    [DefaultValue(80)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryHeader))]
    public int ReverseSearchResultSimilarityThreshold { get; set; }

    [DefaultValue(10)]
    [SettingMetadata(SettingEntryCategory.Search, typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryHeader))]
    public int MaximumSuggestionBoxSearchHistory { get; set; }

    [DefaultValue(IllustrationViewOption.RiverFlow)]
    [SettingMetadata(SettingEntryCategory.BrowsingExperience, typeof(SettingsPageResources), nameof(SettingsPageResources.IllustrationViewOptionEntryHeader))]
    public IllustrationViewOption IllustrationViewOption { get; set; }

    [DefaultValue(true)]
    [SyntheticSetting]
    public bool ShowRecommendIllustratorsInIllustratorContentViewer { get; set; }

    [DefaultValue(true)]
    [SyntheticSetting]
    public bool ShowExternalCommandBarInIllustratorContentViewer { get; set; }

    [DefaultValue(AppBackdropType.Mica)]
    [SettingMetadata(SettingEntryCategory.Application, typeof(SettingsPageResources), nameof(SettingsPageResources.BackdropEntryHeader))]
    public AppBackdropType AppBackdrop { get; set; }

    public static AppSetting CreateDefault()
    {
        return new AppSetting();
    }

    public MakoClientConfiguration ToMakoClientConfiguration()
    {
        return new MakoClientConfiguration(5000, !DisableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
    }
}
