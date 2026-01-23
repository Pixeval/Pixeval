// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

/// <summary>
/// We require a strict matching between the value of the enum member and the order of the
/// <see cref="NavigationViewItem" />in <see cref="MainPage" />
/// </summary>
[LocalizationMetadata]
public enum MainPageTabItem
{
    [LocalizedResource(MainPageResources.RecommendationsTabContent)]
    Recommendation = 0,

    [LocalizedResource(MainPageResources.RankingsTabContent)]
    Ranking = 1,

    [LocalizedResource(MainPageResources.BookmarksTabContent)]
    Bookmark = 2,

    [LocalizedResource(MainPageResources.FollowingsTabContent)]
    Follow = 3,

    [LocalizedResource(MainPageResources.SpotlightsTabContent)]
    Spotlight = 4,

    [LocalizedResource(MainPageResources.RecommendUsersTabContent)]
    RecommendUser = 5,

    [LocalizedResource(MainPageResources.RecentPostsTabContent)]
    RecentPost = 6,

    [LocalizedResource(MainPageResources.NewWorksTabContent)]
    NewWork = 7
}
