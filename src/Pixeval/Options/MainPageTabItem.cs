using Microsoft.UI.Xaml.Controls;
using Pixeval.Misc;
using Pixeval.Pages;

namespace Pixeval.Options
{
    /// <summary>
    /// We require a strict matching between the value of the enum member and the order of the <see cref="NavigationViewItem"/>
    /// in <see cref="MainPage"/>
    /// </summary>
    public enum MainPageTabItem
    {
        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabDailyRecommendations))]
        DailyRecommendation = 0,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabRanking))]
        Ranking = 1,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabBookmark))]
        Bookmark = 2,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabFollow))]
        Follow = 3,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabSpotlight))]
        Spotlight = 4,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabFeed))]
        Feed = 5,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabUpdate))]
        Update = 6,

        [LocalizedResource(typeof(MiscResources), nameof(MiscResources.MainPageTabReverseSearch))]
        ReverseSearch = 7
    }
}