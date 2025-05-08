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
    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.RecommendationsTabContent))]
    Recommendation = 0,

    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.RankingsTabContent))]
    Ranking = 1,

    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.BookmarksTabContent))]
    Bookmark = 2,

    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.FollowingsTabContent))]
    Follow = 3,

    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.RecommendUsersTabContent))]
    RecommendUser = 5,

    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.RecentPostsTabContent))]
    RecentPost = 6,

    [LocalizedResource(typeof(MainPageResources), nameof(MainPageResources.NewWorksTabContent))]
    NewWork = 7
}
