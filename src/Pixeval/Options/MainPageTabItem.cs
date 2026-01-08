// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Pages;

namespace Pixeval.Options;

/// <summary>
/// We require a strict matching between the value of the enum member and the order of the
/// <see cref="NavigationViewItem" />in <see cref="MainPage" />
/// </summary>
[LocalizationMetadata(typeof(MainPageResources))]
public enum MainPageTabItem
{
    [LocalizedResource(nameof(MainPageResources.RecommendationsTabContent))]
    Recommendation = 0,

    [LocalizedResource(nameof(MainPageResources.RankingsTabContent))]
    Ranking = 1,

    [LocalizedResource(nameof(MainPageResources.BookmarksTabContent))]
    Bookmark = 2,

    [LocalizedResource(nameof(MainPageResources.FollowingsTabContent))]
    Follow = 3,

    [LocalizedResource(nameof(MainPageResources.SpotlightsTabContent))]
    Spotlight = 4,

    [LocalizedResource(nameof(MainPageResources.RecommendUsersTabContent))]
    RecommendUser = 5,

    [LocalizedResource(nameof(MainPageResources.RecentPostsTabContent))]
    RecentPost = 6,

    [LocalizedResource(nameof(MainPageResources.NewWorksTabContent))]
    NewWork = 7
}
