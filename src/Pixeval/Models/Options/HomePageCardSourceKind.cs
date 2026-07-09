// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;
using Pixeval.Views.Home;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum HomePageCardSourceKind
{
    [LocalizedResource(MainPageResources.TabWorkRecommended)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkRecommended, Key = nameof(HomeCardDefinition.Description))]
    WorkRecommended,

    [LocalizedResource(MainPageResources.TabWorkBookmarks)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkBookmarks, Key = nameof(HomeCardDefinition.Description))]
    WorkBookmarks,

    [LocalizedResource(MainPageResources.TabWorkRanking)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkRanking, Key = nameof(HomeCardDefinition.Description))]
    WorkRanking,

    [LocalizedResource(MainPageResources.TabWorkNew)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkNew, Key = nameof(HomeCardDefinition.Description))]
    WorkNew,

    [LocalizedResource(MainPageResources.TabWorkFollowing)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkFollowing, Key = nameof(HomeCardDefinition.Description))]
    WorkFollowing,

    [LocalizedResource(MainPageResources.TabWorkMyPixiv)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkMyPixiv, Key = nameof(HomeCardDefinition.Description))]
    WorkMyPixiv,

    [LocalizedResource(MainPageResources.TabWorkRelated)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkRelated, Key = nameof(HomeCardDefinition.Description))]
    WorkRelated,

    [LocalizedResource(MainPageResources.TabWorkPosts)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkPosts, Key = nameof(HomeCardDefinition.Description))]
    WorkPosts,

    [LocalizedResource(MainPageResources.TabWorkSearchResult)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkSearch, Key = nameof(HomeCardDefinition.Description))]
    WorkSearch,

    [LocalizedResource(MainPageResources.TabUserRecommended)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserRecommended, Key = nameof(HomeCardDefinition.Description))]
    UserRecommended,

    [LocalizedResource(MainPageResources.TabUserSearchResult)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserSearch, Key = nameof(HomeCardDefinition.Description))]
    UserSearch,

    [LocalizedResource(MainPageResources.TabUserFollowing)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserFollowing, Key = nameof(HomeCardDefinition.Description))]
    UserFollowing,

    [LocalizedResource(MainPageResources.TabUserFollower)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserFollower, Key = nameof(HomeCardDefinition.Description))]
    UserFollower,

    [LocalizedResource(MainPageResources.TabUserMyPixiv)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserMyPixiv, Key = nameof(HomeCardDefinition.Description))]
    UserMyPixiv,

    [LocalizedResource(MainPageResources.TabSpotlight)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSpotlight, Key = nameof(HomeCardDefinition.Description))]
    Spotlight,

    [LocalizedResource(MainPageResources.TabSingleImage)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSingleImage, Key = nameof(HomeCardDefinition.Description))]
    SingleImage,

    [LocalizedResource(MainPageResources.TabSingleNovel)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSingleNovel, Key = nameof(HomeCardDefinition.Description))]
    SingleNovel,

    [LocalizedResource(MainPageResources.TabSingleUser)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSingleUser, Key = nameof(HomeCardDefinition.Description))]
    SingleUser
}
