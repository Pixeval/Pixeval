using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Options;

namespace Pixeval
{
    public record AppSetting
    {
        /// <summary>
        /// The Application Theme
        /// </summary>
        public ApplicationTheme Theme { get; set; }

        /// <summary>
        /// The tags that are not allowed to be included in the search result.
        /// The particular search result will be ignored if any of its tag is
        /// included in <see cref="ExcludeTags"/>
        /// </summary>
        public ObservableCollection<string> ExcludeTags { get; set; }

        /// <summary>
        /// Indicates whether the restricted content are permitted to be included
        /// in the searching results, including R-18 and R-18G
        /// </summary>
        public bool FiltrateRestrictedContent { get; set; }

        /// <summary>
        /// Disable the domain fronting technology, once disabled, the users
        /// from China mainland are required to have other countermeasures to bypass
        /// GFW
        /// </summary>
        public bool DisableDomainFronting { get; set; }

        /// <summary>
        /// The application-wide default sort option, any illustration page that supports
        /// different orders will use this as its default value
        /// </summary>
        public IllustrationSortOption DefaultSortOption { get; set; }

        /// <summary>
        /// The tag match option for keyword search
        /// </summary>
        public SearchTagMatchOption TagMatchOption { get; set; }

        /// <summary>
        /// The target filter that indicates the type of the client
        /// </summary>
        public TargetFilter TargetFilter { get; set; }

        /// <summary>
        /// How many rows to be preloaded in illustration grid
        /// </summary>
        public int PreLoadRows { get; set; }

        /// <summary>
        /// Indicates the maximum page count that are allowed to be retrieved during
        /// keyword search(30 entries per page)
        /// </summary>
        public int PageLimitForKeywordSearch { get; set; }

        /// <summary>
        /// Indicates the starting page's number of keyword search
        /// </summary>
        public int SearchStartingFromPageNumber { get; set; }

        /// <summary>
        /// Indicates the maximum page count that are allowed to be retrieved during
        /// spotlight retrieval(10 entries per page)
        /// </summary>
        public int PageLimitForSpotlight { get; set; }

        /// <summary>
        /// The mirror host for image server, Pixeval will do a simple substitution that
        /// changes the host of the original url(i.pximg.net) to this one.
        /// </summary>
        public string? MirrorHost { get; set; }

        /// <summary>
        /// The max download tasks that are allowed to run concurrently
        /// </summary>
        public int MaxDownloadTaskConcurrencyLevel { get; set; }

        public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; }

        public int ItemsNumberLimitForDailyRecommendations { get; set; }

        public bool UseFileCache { get; set; }

        public int WindowWidth { get; set; }

        public int WindowHeight { get; set; }

        public ThumbnailDirection ThumbnailDirection { get; set; }

        public AppSetting(
            ApplicationTheme theme,
            ObservableCollection<string> excludeTags,
            bool disableDomainFronting,
            IllustrationSortOption defaultSortOption,
            SearchTagMatchOption searchTagMatchOption,
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
            ThumbnailDirection thumbnailDirection)
        {
            Theme = theme;
            ExcludeTags = excludeTags;
            DisableDomainFronting = disableDomainFronting;
            DefaultSortOption = defaultSortOption;
            TagMatchOption = searchTagMatchOption;
            TargetFilter = targetFilter;
            PreLoadRows = preLoadRows;
            PageLimitForKeywordSearch = pageLimitForKeywordSearch;
            SearchStartingFromPageNumber = searchStartingFromPageNumber;
            PageLimitForSpotlight = pageLimitForSpotlight;
            MirrorHost = mirrorHost;
            MaxDownloadTaskConcurrencyLevel = maxDownloadTaskConcurrencyLevel;
            DisplayTeachingTipWhenGeneratingAppLink = displayTeachingTipWhenGeneratingAppLink;
            ItemsNumberLimitForDailyRecommendations = itemsNumberLimitForDailyRecommendations;
            FiltrateRestrictedContent = filtrateRestrictedContent;
            UseFileCache = useFileCache;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            ThumbnailDirection = thumbnailDirection;
        }

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
                ThumbnailDirection.Portrait);
        }

        public MakoClientConfiguration ToMakoClientConfiguration()
        {
            return new(5000, !DisableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
        }
    }
}