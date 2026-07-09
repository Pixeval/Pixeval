// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Models.Navigation;

public static class NavigationMenuYaml
{
    public const string DefaultYaml =
        $"""
         newTab: Home

         header:
           - page: Search
           - page: WorkRecommended
           - page: WorkRanking
           - page: WorkBookmarks
           - page: UserRecommended
           - folder: "${NavigationSettingsPageResources.WorkFolderTitle}"
             icon: DesignIdeas
             children:
               - page: WorkFollowing
               - page: WorkMyPixiv
               - page: WorkNew
               - page: Spotlight
           - folder: "${NavigationSettingsPageResources.UserFolderTitle}"
             icon: PeopleTeam
             children:
               - page: UserFollowing
               - page: UserFollower
               - page: UserMyPixiv

         footer:
           - page: BrowsingHistory
           - page: WatchLater
           - page: Download
           - page: Extensions
           - page: Settings
           
         """;
}
