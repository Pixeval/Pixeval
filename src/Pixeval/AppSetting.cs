using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
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
            ResetDefault();
        }

        public AppSetting(ApplicationTheme theme,
            ObservableCollection<string> excludeTags,
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
            MainPageTabItem defaultSelectedTabItem)
        {
            Theme = theme;
            ExcludeTags = excludeTags;
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
        }

        /// <summary>
        /// The Application Theme
        /// </summary>
        [DefaultValue(ApplicationTheme.SystemDefault)]
        public ApplicationTheme Theme { get; set; }

        /// <summary>
        /// The tags that are not allowed to be included in the search result.
        /// The particular search result will be ignored if any of its tag is
        /// included in <see cref="ExcludeTags"/>
        /// </summary>
        [DefaultValue(typeof(ObservableCollection<string>))]
        public ObservableCollection<string> ExcludeTags { get; set; }

        /// <summary>
        /// Indicates whether the restricted content are permitted to be included
        /// in the searching results, including R-18 and R-18G
        /// </summary>
        [DefaultValue(false)]
        public bool FiltrateRestrictedContent { get; set; }

        /// <summary>
        /// Disable the domain fronting technology, once disabled, the users
        /// from China mainland are required to have other countermeasures to bypass
        /// GFW
        /// </summary>
        [DefaultValue(false)]
        public bool DisableDomainFronting { get; set; }

        /// <summary>
        /// The application-wide default sort option, any illustration page that supports
        /// different orders will use this as its default value
        /// </summary>
        [DefaultValue(IllustrationSortOption.DoNotSort)]
        public IllustrationSortOption DefaultSortOption { get; set; }

        /// <summary>
        /// The tag match option for keyword search
        /// </summary>
        [DefaultValue(SearchTagMatchOption.PartialMatchForTags)]
        public SearchTagMatchOption TagMatchOption { get; set; }

        /// <summary>
        /// The target filter that indicates the type of the client
        /// </summary>
        [DefaultValue(TargetFilter.ForAndroid)]
        public TargetFilter TargetFilter { get; set; }

        /// <summary>
        /// How many rows to be preloaded in illustration grid
        /// </summary>
        [DefaultValue(2)]
        public int PreLoadRows { get; set; }

        /// <summary>
        /// Indicates the maximum page count that are allowed to be retrieved during
        /// keyword search(30 entries per page)
        /// </summary>
        [DefaultValue(100)]
        public int PageLimitForKeywordSearch { get; set; }

        /// <summary>
        /// Indicates the starting page's number of keyword search
        /// </summary>
        [DefaultValue(1)]
        public int SearchStartingFromPageNumber { get; set; }

        /// <summary>
        /// Indicates the maximum page count that are allowed to be retrieved during
        /// spotlight retrieval(10 entries per page)
        /// </summary>
        [DefaultValue(50)]
        public int PageLimitForSpotlight { get; set; }

        /// <summary>
        /// The mirror host for image server, Pixeval will do a simple substitution that
        /// changes the host of the original url(i.pximg.net) to this one.
        /// </summary>
        [DefaultValue(null)]
        public string? MirrorHost { get; set; }

        /// <summary>
        /// The max download tasks that are allowed to run concurrently
        /// </summary>
        [DefaultValue(typeof(ProcessorCountDefaultValueProvider))]
        public int MaxDownloadTaskConcurrencyLevel { get; set; }

        [DefaultValue(true)]
        public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; }

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

        [DefaultValue(typeof(DateTimeOffSetDefaultValueProvider))]
        public DateTimeOffset LastCheckedUpdate { get; set; }

        [DefaultValue(false)]
        public bool DownloadUpdateAutomatically { get; set; }

        [DefaultValue("Segoe UI")]
        public string AppFontFamilyName { get; set; }

        [DefaultValue(MainPageTabItem.DailyRecommendation)]
        public MainPageTabItem DefaultSelectedTabItem { get; set; }

        public static AppSetting CreateDefault()
        {
            return new AppSetting();
        }

        private void ResetDefault()
        {
            foreach (var propertyInfo in typeof(AppSetting).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propertyInfo.SetValue(this, propertyInfo.GetDefaultValue());
            }
        }

        public MakoClientConfiguration ToMakoClientConfiguration()
        {
            return new MakoClientConfiguration(5000, !DisableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
        }
    }
}