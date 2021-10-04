using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Options;

namespace Pixeval
{
    public record AppSetting(
        ApplicationTheme Theme,
        ObservableCollection<string> ExcludeTags,
        bool DisableDomainFronting,
        IllustrationSortOption DefaultSortOption,
        SearchTagMatchOption TagMatchOption,
        TargetFilter TargetFilter,
        int PreLoadRows,
        int PageLimitForKeywordSearch,
        int SearchStartingFromPageNumber,
        int PageLimitForSpotlight,
        string? MirrorHost,
        int MaxDownloadTaskConcurrencyLevel,
        bool DisplayTeachingTipWhenGeneratingAppLink,
        int ItemsNumberLimitForDailyRecommendations,
        bool FiltrateRestrictedContent,
        bool UseFileCache,
        int WindowWidth,
        int WindowHeight,
        ThumbnailDirection ThumbnailDirection,
        DateTimeOffset LastCheckedUpdate,
        bool DownloadUpdateAutomatically,
        string AppFontFamilyName)
    {
        /// <summary>
        /// The Application Theme
        /// </summary>
        public ApplicationTheme Theme { get; set; } = Theme;

        /// <summary>
        /// The tags that are not allowed to be included in the search result.
        /// The particular search result will be ignored if any of its tag is
        /// included in <see cref="ExcludeTags"/>
        /// </summary>
        public ObservableCollection<string> ExcludeTags { get; set; } = ExcludeTags;

        /// <summary>
        /// Indicates whether the restricted content are permitted to be included
        /// in the searching results, including R-18 and R-18G
        /// </summary>
        public bool FiltrateRestrictedContent { get; set; } = FiltrateRestrictedContent;

        /// <summary>
        /// Disable the domain fronting technology, once disabled, the users
        /// from China mainland are required to have other countermeasures to bypass
        /// GFW
        /// </summary>
        public bool DisableDomainFronting { get; set; } = DisableDomainFronting;

        /// <summary>
        /// The application-wide default sort option, any illustration page that supports
        /// different orders will use this as its default value
        /// </summary>
        public IllustrationSortOption DefaultSortOption { get; set; } = DefaultSortOption;

        /// <summary>
        /// The tag match option for keyword search
        /// </summary>
        public SearchTagMatchOption TagMatchOption { get; set; } = TagMatchOption;

        /// <summary>
        /// The target filter that indicates the type of the client
        /// </summary>
        public TargetFilter TargetFilter { get; set; } = TargetFilter;

        /// <summary>
        /// How many rows to be preloaded in illustration grid
        /// </summary>
        public int PreLoadRows { get; set; } = PreLoadRows;

        /// <summary>
        /// Indicates the maximum page count that are allowed to be retrieved during
        /// keyword search(30 entries per page)
        /// </summary>
        public int PageLimitForKeywordSearch { get; set; } = PageLimitForKeywordSearch;

        /// <summary>
        /// Indicates the starting page's number of keyword search
        /// </summary>
        public int SearchStartingFromPageNumber { get; set; } = SearchStartingFromPageNumber;

        /// <summary>
        /// Indicates the maximum page count that are allowed to be retrieved during
        /// spotlight retrieval(10 entries per page)
        /// </summary>
        public int PageLimitForSpotlight { get; set; } = PageLimitForSpotlight;

        /// <summary>
        /// The mirror host for image server, Pixeval will do a simple substitution that
        /// changes the host of the original url(i.pximg.net) to this one.
        /// </summary>
        public string? MirrorHost { get; set; } = MirrorHost;

        /// <summary>
        /// The max download tasks that are allowed to run concurrently
        /// </summary>
        public int MaxDownloadTaskConcurrencyLevel { get; set; } = MaxDownloadTaskConcurrencyLevel;

        public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; } = DisplayTeachingTipWhenGeneratingAppLink;

        public int ItemsNumberLimitForDailyRecommendations { get; set; } = ItemsNumberLimitForDailyRecommendations;

        public bool UseFileCache { get; set; } = UseFileCache;

        public int WindowWidth { get; set; } = WindowWidth;

        public int WindowHeight { get; set; } = WindowHeight;

        public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection;

        public DateTimeOffset LastCheckedUpdate { get; set; } = LastCheckedUpdate;

        public bool DownloadUpdateAutomatically { get; set; } = DownloadUpdateAutomatically;

        public string AppFontFamilyName { get; set; } = AppFontFamilyName;

        public static AppSetting CreateDefault()
        {
            var (width, height) = App.PredetermineEstimatedWindowSize();
            return new AppSetting(
                ApplicationTheme.SystemDefault,
                new ObservableCollection<string>(),
                false,
                IllustrationSortOption.DoNotSort,
                SearchTagMatchOption.PartialMatchForTags,
                TargetFilter.ForAndroid,
                2,
                100,
                1,
                50,
                null,
                Environment.ProcessorCount,
                true,
                500,
                false,
                false,
                width,
                height,
                ThumbnailDirection.Portrait,
                DateTimeOffset.MinValue,
                false,
                "Segoe UI");
        }

        public MakoClientConfiguration ToMakoClientConfiguration()
        {
            return new MakoClientConfiguration(5000, !DisableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
        }
    }
}