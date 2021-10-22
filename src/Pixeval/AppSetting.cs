#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AppSetting.cs
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
using Pixeval.CoreApi;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Misc;
using Pixeval.Options;

namespace Pixeval
{
    public record AppSetting
    {
#pragma warning disable CS8618
        public AppSetting()
#pragma warning restore CS8618
        {
            DefaultValueAttributeHelper.Initialize(this);
        }

        // ReSharper disable once UnusedMember.Global
        public AppSetting(
            ApplicationTheme theme,
            bool disableDomainFronting,
            IllustrationSortOption defaultSortOption,
            SearchTagMatchOption tagMatchOption,
            TargetFilter targetFilter,
            int preLoadRows,
            int pageLimitForKeywordSearch,
            int searchStartingFromPageNumber,
            int pageLimitForSpotlight,
            string? mirrorHost,
            int maxDownloadTaskConcurrencyLevel,
            bool displayTeachingTipWhenGeneratingAppLink,
            int itemsNumberLimitForDailyRecommendations,
            bool filtrateRestrictedContent,
            bool useFileCache,
            int windowWidth,
            int windowHeight,
            ThumbnailDirection thumbnailDirection,
            DateTimeOffset lastCheckedUpdate,
            bool downloadUpdateAutomatically,
            string appFontFamilyName,
            MainPageTabItem defaultSelectedTabItem,
            SearchDuration searchDuration,
            bool usePreciseRangeForSearch,
            DateTimeOffset searchStartDate,
            DateTimeOffset searchEndDate,
            string defaultDownloadPathMacro)
        {
            Theme = theme;
            FiltrateRestrictedContent = filtrateRestrictedContent;
            DisableDomainFronting = disableDomainFronting;
            DefaultSortOption = defaultSortOption;
            TagMatchOption = tagMatchOption;
            TargetFilter = targetFilter;
            PreLoadRows = preLoadRows;
            PageLimitForKeywordSearch = pageLimitForKeywordSearch;
            SearchStartingFromPageNumber = searchStartingFromPageNumber;
            PageLimitForSpotlight = pageLimitForSpotlight;
            MirrorHost = mirrorHost;
            MaxDownloadTaskConcurrencyLevel = maxDownloadTaskConcurrencyLevel;
            DisplayTeachingTipWhenGeneratingAppLink = displayTeachingTipWhenGeneratingAppLink;
            ItemsNumberLimitForDailyRecommendations = itemsNumberLimitForDailyRecommendations;
            UseFileCache = useFileCache;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            ThumbnailDirection = thumbnailDirection;
            LastCheckedUpdate = lastCheckedUpdate;
            DownloadUpdateAutomatically = downloadUpdateAutomatically;
            AppFontFamilyName = appFontFamilyName;
            DefaultSelectedTabItem = defaultSelectedTabItem;
            SearchDuration = searchDuration;
            UsePreciseRangeForSearch = usePreciseRangeForSearch;
            SearchStartDate = searchStartDate;
            SearchEndDate = searchEndDate;
            DefaultDownloadPathMacro = defaultDownloadPathMacro;
        }

        /// <summary>
        ///     The Application Theme
        /// </summary>
        [DefaultValue(ApplicationTheme.SystemDefault)]
        public ApplicationTheme Theme { get; set; }

        /// <summary>
        ///     Indicates whether the restricted content are permitted to be included
        ///     in the searching results, including R-18 and R-18G
        /// </summary>
        [DefaultValue(false)]
        public bool FiltrateRestrictedContent { get; set; }

        /// <summary>
        ///     Disable the domain fronting technology, once disabled, the users
        ///     from China mainland are required to have other countermeasures to bypass
        ///     GFW
        /// </summary>
        [DefaultValue(false)]
        public bool DisableDomainFronting { get; set; }

        /// <summary>
        ///     The application-wide default sort option, any illustration page that supports
        ///     different orders will use this as its default value
        /// </summary>
        [DefaultValue(IllustrationSortOption.DoNotSort)]
        public IllustrationSortOption DefaultSortOption { get; set; }

        /// <summary>
        ///     The tag match option for keyword search
        /// </summary>
        [DefaultValue(SearchTagMatchOption.PartialMatchForTags)]
        public SearchTagMatchOption TagMatchOption { get; set; }

        /// <summary>
        ///     The target filter that indicates the type of the client
        /// </summary>
        [DefaultValue(TargetFilter.ForAndroid)]
        public TargetFilter TargetFilter { get; set; }

        /// <summary>
        ///     How many rows to be preloaded in illustration grid
        /// </summary>
        [DefaultValue(2)]
        public int PreLoadRows { get; set; }

        /// <summary>
        ///     Indicates the maximum page count that are allowed to be retrieved during
        ///     keyword search(30 entries per page)
        /// </summary>
        [DefaultValue(100)]
        public int PageLimitForKeywordSearch { get; set; }

        /// <summary>
        ///     Indicates the starting page's number of keyword search
        /// </summary>
        [DefaultValue(1)]
        public int SearchStartingFromPageNumber { get; set; }

        /// <summary>
        ///     Indicates the maximum page count that are allowed to be retrieved during
        ///     spotlight retrieval(10 entries per page)
        /// </summary>
        [DefaultValue(50)]
        public int PageLimitForSpotlight { get; set; }

        /// <summary>
        ///     The mirror host for image server, Pixeval will do a simple substitution that
        ///     changes the host of the original url(i.pximg.net) to this one.
        /// </summary>
        [DefaultValue(null)]
        public string? MirrorHost { get; set; }

        /// <summary>
        ///     The max download tasks that are allowed to run concurrently
        /// </summary>
        [DefaultValue(typeof(ProcessorCountDefaultValueProvider))]
        public int MaxDownloadTaskConcurrencyLevel { get; set; }

        /// <summary>
        ///     Indicates whether a <see cref="TeachingTip" /> should be displayed
        ///     when user clicks "Generate Link"
        /// </summary>
        [DefaultValue(true)]
        public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; }

        /// <summary>
        ///     Indicates how many illustrations will be collected during
        ///     the enumeration of the <see cref="MakoClient.Recommendations" />
        /// </summary>
        [DefaultValue(500)]
        public int ItemsNumberLimitForDailyRecommendations { get; set; }

        [DefaultValue(false)]
        public bool UseFileCache { get; set; }

        [DefaultValue(typeof(AppWidthDefaultValueProvider))]
        public int WindowWidth { get; set; }

        [DefaultValue(typeof(AppHeightDefaultValueProvider))]
        public int WindowHeight { get; set; }

        [DefaultValue(ThumbnailDirection.Portrait)]
        public ThumbnailDirection ThumbnailDirection { get; set; }

        [DefaultValue(typeof(MinDateTimeOffSetDefaultValueProvider))]
        public DateTimeOffset LastCheckedUpdate { get; set; }

        [DefaultValue(false)]
        public bool DownloadUpdateAutomatically { get; set; }

        [DefaultValue("Segoe UI")]
        public string AppFontFamilyName { get; set; }

        [DefaultValue(MainPageTabItem.DailyRecommendation)]
        public MainPageTabItem DefaultSelectedTabItem { get; set; }

        [DefaultValue(SearchDuration.Undecided)]
        public SearchDuration SearchDuration { get; set; }

        [DefaultValue(false)]
        public bool UsePreciseRangeForSearch { get; set; }

        [DefaultValue(typeof(DecrementedDateTimeOffSetDefaultValueProvider))]
        public DateTimeOffset SearchStartDate { get; set; }

        [DefaultValue(typeof(CurrentDateTimeOffSetDefaultValueProvider))]
        public DateTimeOffset SearchEndDate { get; set; }

        [DefaultValue(typeof(DownloadPathMacroDefaultValueProvider))]
        public string DefaultDownloadPathMacro { get; set; }

        public static AppSetting CreateDefault()
        {
            return new AppSetting();
        }

        public MakoClientConfiguration ToMakoClientConfiguration()
        {
            return new MakoClientConfiguration(5000, !DisableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
        }
    }
}