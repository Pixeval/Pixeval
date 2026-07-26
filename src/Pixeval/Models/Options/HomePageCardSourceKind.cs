// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;
using Pixeval.Views.Home;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum HomePageCardSourceKind
{
    [LocalizedResource(MainPageResources.Tab.WorkRecommended)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkRecommended, Key = nameof(HomeCardDefinition.Description))]
    WorkRecommended,

    [LocalizedResource(MainPageResources.Tab.WorkBookmarks)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkBookmarks, Key = nameof(HomeCardDefinition.Description))]
    WorkBookmarks,

    [LocalizedResource(MainPageResources.Tab.WorkRanking)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkRanking, Key = nameof(HomeCardDefinition.Description))]
    WorkRanking,

    [LocalizedResource(MainPageResources.Tab.WorkNew)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkNew, Key = nameof(HomeCardDefinition.Description))]
    WorkNew,

    [LocalizedResource(MainPageResources.Tab.WorkFollowing)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkFollowing, Key = nameof(HomeCardDefinition.Description))]
    WorkFollowing,

    [LocalizedResource(MainPageResources.Tab.WorkMyPixiv)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkMyPixiv, Key = nameof(HomeCardDefinition.Description))]
    WorkMyPixiv,

    [LocalizedResource(MainPageResources.Tab.WorkRelated)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkRelated, Key = nameof(HomeCardDefinition.Description))]
    WorkRelated,

    [LocalizedResource(MainPageResources.Tab.WorkPosts)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkPosts, Key = nameof(HomeCardDefinition.Description))]
    WorkPosts,

    [LocalizedResource(MainPageResources.Tab.WorkSearchResult)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.WorkSearch, Key = nameof(HomeCardDefinition.Description))]
    WorkSearch,

    [LocalizedResource(MainPageResources.Tab.UserRecommended)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.UserRecommended, Key = nameof(HomeCardDefinition.Description))]
    UserRecommended,

    [LocalizedResource(MainPageResources.Tab.UserSearchResult)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.UserSearch, Key = nameof(HomeCardDefinition.Description))]
    UserSearch,

    [LocalizedResource(MainPageResources.Tab.UserFollowing)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.UserFollowing, Key = nameof(HomeCardDefinition.Description))]
    UserFollowing,

    [LocalizedResource(MainPageResources.Tab.UserFollower)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.UserFollower, Key = nameof(HomeCardDefinition.Description))]
    UserFollower,

    [LocalizedResource(MainPageResources.Tab.UserMyPixiv)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.UserMyPixiv, Key = nameof(HomeCardDefinition.Description))]
    UserMyPixiv,

    [LocalizedResource(MainPageResources.Tab.Spotlight)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.Spotlight, Key = nameof(HomeCardDefinition.Description))]
    Spotlight,

    [LocalizedResource(MainPageResources.Tab.SingleImage)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.SingleImage, Key = nameof(HomeCardDefinition.Description))]
    SingleImage,

    [LocalizedResource(MainPageResources.Tab.SingleNovel)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.SingleNovel, Key = nameof(HomeCardDefinition.Description))]
    SingleNovel,

    [LocalizedResource(MainPageResources.Tab.SingleUser)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.SingleUser, Key = nameof(HomeCardDefinition.Description))]
    SingleUser,

    [LocalizedResource(MainPageResources.Tab.SingleSeries)] [LocalizedResource(EnumResources.HomePageCardSourceKindDescription.SingleSeries, Key = nameof(HomeCardDefinition.Description))]
    SingleSeries
}
