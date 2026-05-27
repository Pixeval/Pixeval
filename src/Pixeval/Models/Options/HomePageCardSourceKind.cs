// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;
using Pixeval.Views.Home;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum HomePageCardSourceKind
{
    [LocalizedResource(MainPageResources.TabWorkRecommended)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkRecommended, Key = nameof(HomeCardTemplate.Description))]
    WorkRecommended,

    [LocalizedResource(MainPageResources.TabWorkBookmarks)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkBookmarks, Key = nameof(HomeCardTemplate.Description))]
    WorkBookmarks,

    [LocalizedResource(MainPageResources.TabWorkRanking)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkRanking, Key = nameof(HomeCardTemplate.Description))]
    WorkRanking,

    [LocalizedResource(MainPageResources.TabWorkNew)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkNew, Key = nameof(HomeCardTemplate.Description))]
    WorkNew,

    [LocalizedResource(MainPageResources.TabWorkFollowing)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkFollowing, Key = nameof(HomeCardTemplate.Description))]
    WorkFollowing,

    [LocalizedResource(MainPageResources.TabWorkPosts)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkPosts, Key = nameof(HomeCardTemplate.Description))]
    WorkPosts,

    [LocalizedResource(MainPageResources.TabWorkSearch)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionWorkSearch, Key = nameof(HomeCardTemplate.Description))]
    WorkSearch,

    [LocalizedResource(MainPageResources.TabUserRecommended)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserRecommended, Key = nameof(HomeCardTemplate.Description))]
    UserRecommended,

    [LocalizedResource(MainPageResources.TabUserSearch)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserSearch, Key = nameof(HomeCardTemplate.Description))]
    UserSearch,

    [LocalizedResource(MainPageResources.TabUserFollowing)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserFollowing, Key = nameof(HomeCardTemplate.Description))]
    UserFollowing,

    [LocalizedResource(MainPageResources.TabUserMyPixiv)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionUserMyPixiv, Key = nameof(HomeCardTemplate.Description))]
    UserMyPixiv,

    [LocalizedResource(MainPageResources.TabSpotlight)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSpotlight, Key = nameof(HomeCardTemplate.Description))]
    Spotlight,

    [LocalizedResource(MainPageResources.TabSingleImage)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSingleImage, Key = nameof(HomeCardTemplate.Description))]
    SingleImage,

    [LocalizedResource(MainPageResources.TabSingleNovel)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSingleNovel, Key = nameof(HomeCardTemplate.Description))]
    SingleNovel,

    [LocalizedResource(MainPageResources.TabSingleUser)]
    [LocalizedResource(EnumResources.HomePageCardSourceKindDescriptionSingleUser, Key = nameof(HomeCardTemplate.Description))]
    SingleUser
}
