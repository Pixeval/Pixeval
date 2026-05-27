// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;
using Pixeval.Views.Home;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum HomePageCardSourceKind
{
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkRecommendedTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkRecommendedDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkRecommended,

    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkBookmarksTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkBookmarksDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkBookmarks,

    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkRankingTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkRankingDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkRanking,

    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkNewTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkNewDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkNew,

    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkFollowingTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkFollowingDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkFollowing,

    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkPostsTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkPostsDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkPosts,

    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkSearchTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindWorkSearchDescription, Key = nameof(HomeCardTemplate.Description))]
    WorkSearch,

    [LocalizedResource(EnumResources.HomePageCardSourceKindUserRecommendedTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindUserRecommendedDescription, Key = nameof(HomeCardTemplate.Description))]
    UserRecommended,

    [LocalizedResource(EnumResources.HomePageCardSourceKindUserSearchTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindUserSearchDescription, Key = nameof(HomeCardTemplate.Description))]
    UserSearch,

    [LocalizedResource(EnumResources.HomePageCardSourceKindUserFollowingTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindUserFollowingDescription, Key = nameof(HomeCardTemplate.Description))]
    UserFollowing,

    [LocalizedResource(EnumResources.HomePageCardSourceKindUserMyPixivTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindUserMyPixivDescription, Key = nameof(HomeCardTemplate.Description))]
    UserMyPixiv,

    [LocalizedResource(EnumResources.HomePageCardSourceKindSpotlightTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindSpotlightDescription, Key = nameof(HomeCardTemplate.Description))]
    Spotlight,

    [LocalizedResource(EnumResources.HomePageCardSourceKindSingleImageTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindSingleImageDescription, Key = nameof(HomeCardTemplate.Description))]
    SingleImage,

    [LocalizedResource(EnumResources.HomePageCardSourceKindSingleNovelTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindSingleNovelDescription, Key = nameof(HomeCardTemplate.Description))]
    SingleNovel,

    [LocalizedResource(EnumResources.HomePageCardSourceKindSingleUserTitle)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindSingleUserDescription, Key = nameof(HomeCardTemplate.Description))]
    SingleUser
}
