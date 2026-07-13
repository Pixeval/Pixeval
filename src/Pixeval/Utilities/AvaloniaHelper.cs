// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FluentIcons.Common;
using Pixeval.I18N;
using Pixeval.Models.Navigation;
using Pixeval.Models.Options;
using Pixeval.Views;
using Pixeval.Views.Capability;
using Pixeval.Views.Download;
using Pixeval.Views.Home;
using Pixeval.Views.Login;
using Pixeval.Views.Search;
using Pixeval.Views.Settings;
using Pixeval.Views.ViewContainers;
using Pixeval.Views.Viewers;

namespace Pixeval.Utilities;

public static class AvaloniaHelper
{
    static AvaloniaHelper()
    {
    }

    public static void Init()
    {
    }

    extension(TopLevel topLevel)
    {
        public ViewContainerBase? ViewContainer
        {
            get
            {
                while (true)
                {
                    if (topLevel.Content is ViewContainerBase vc)
                        return vc;
                    var parent = topLevel.Parent;
                    if (parent is Popup)
                        parent = parent.Parent;
                    if (parent is not Visual visual || TopLevel.GetTopLevel(visual) is not { } t)
                        return null;
                    topLevel = t;
                }
            }
        }
    }

    public static Dictionary<Type, (Symbol Symbol, string Header)> PageTypeToHeaderMap { get; } =
    [
        Page<HomePage>(Symbol.Home, MainPageResources.TabHome),
        Page<LoginPage>(Symbol.PersonKey, MainPageResources.TabLogin, false),
        Page<SearchPage>(Symbol.SearchSparkle, MainPageResources.TabSearch),
        Page<WorkRecommendedPage>(Symbol.Calendar, MainPageResources.TabWorkRecommended),
        Page<WorkRankingPage>(Symbol.ArrowTrendingLines, MainPageResources.TabWorkRanking),
        Page<WorkBookmarksPage>(Symbol.Library, MainPageResources.TabWorkBookmarks),
        Page<WorkRelatedPage>(Symbol.LightbulbFilament, MainPageResources.TabWorkRelated, true, false),
        Page<SeriesPage>(Symbol.LayerDiagonalPerson, MainPageResources.TabSeries),
        Page<WorkPostsPage>(Symbol.Image, MainPageResources.TabWorkPosts),
        Page<WorkSearchResultPage>(Symbol.SearchSparkle, MainPageResources.TabWorkSearchResult, true, false),
        Page<UserFollowingPage>(Symbol.PersonHeart, MainPageResources.TabUserFollowing),
        Page<SpotlightPage>(Symbol.SlideTextSparkle, MainPageResources.TabSpotlight),
        Page<UserRecommendedPage>(Symbol.PeopleCommunity, MainPageResources.TabUserRecommended),
        Page<UserSearchResultPage>(Symbol.Person, MainPageResources.TabUserSearchResult, true, false),
        Page<UserFollowerPage>(Symbol.People, MainPageResources.TabUserFollower),
        Page<UserMyPixivPage>(Symbol.PeopleInterwoven, MainPageResources.TabUserMyPixiv),
        Page<RelatedUsersPage>(Symbol.PeopleCommunity, MainPageResources.TabRelatedUser),
        Page<WorkFollowingPage>(Symbol.AlertUrgent, MainPageResources.TabWorkFollowing),
        Page<WorkMyPixivPage>(Symbol.Molecule, MainPageResources.TabWorkMyPixiv),
        Page<WorkNewPage>(Symbol.ArrowSync, MainPageResources.TabWorkNew),
        Page<BrowsingHistoryPage>(Symbol.History, MainPageResources.TabBrowsingHistory),
        Page<WatchLaterPage>(Symbol.Clock, MainPageResources.TabWatchLater),
        Page<DownloadPage>(Symbol.ArrowSquareDown, MainPageResources.TabDownload),
        Page<ExtensionsPage>(Symbol.PuzzlePiece, MainPageResources.TabExtensions, false),
        Page<SettingsPage>(Symbol.Settings, MainPageResources.TabSettings, false),
        Page<AboutPage>(Symbol.PersonStarburst, MainPageResources.TabAbout, false, false),
        Page<HelpPage>(Symbol.ChatBubblesQuestion, MainPageResources.TabHelp, false, false),
        Page<IllustrationViewerPage>(Symbol.Image, MainPageResources.TabSingleImage, true, false),
        Page<NovelViewerPage>(Symbol.BookOpen, MainPageResources.TabSingleNovel, true, false),
        Page<UserViewerPage>(Symbol.Person, MainPageResources.TabSingleUser, true, false),
        Page<SeriesViewerPage>(Symbol.ListBar, MainPageResources.TabSingleSeries, true, false),
        Page<WorkInfoPage>(Symbol.BookContacts, MainPageResources.TabWorkInfo, true, false),
        Page<CommentsPage>(Symbol.ChatMultiple, MainPageResources.TabComments, true, false)
    ];

    public static Dictionary<HomePageCardSourceKind, (Symbol Symbol, string Header)> HomeCardSourceKindToHeaderMap { get; } =
        new()
        {
            [HomePageCardSourceKind.WorkRecommended] = PageTypeToHeaderMap[typeof(WorkRecommendedPage)],
            [HomePageCardSourceKind.WorkBookmarks] = PageTypeToHeaderMap[typeof(WorkBookmarksPage)],
            [HomePageCardSourceKind.WorkRanking] = PageTypeToHeaderMap[typeof(WorkRankingPage)],
            [HomePageCardSourceKind.WorkNew] = PageTypeToHeaderMap[typeof(WorkNewPage)],
            [HomePageCardSourceKind.WorkFollowing] = PageTypeToHeaderMap[typeof(WorkFollowingPage)],
            [HomePageCardSourceKind.WorkMyPixiv] = PageTypeToHeaderMap[typeof(WorkMyPixivPage)],
            [HomePageCardSourceKind.WorkRelated] = PageTypeToHeaderMap[typeof(WorkRelatedPage)],
            [HomePageCardSourceKind.WorkPosts] = PageTypeToHeaderMap[typeof(WorkPostsPage)],
            [HomePageCardSourceKind.WorkSearch] = PageTypeToHeaderMap[typeof(WorkSearchResultPage)],
            [HomePageCardSourceKind.UserRecommended] = PageTypeToHeaderMap[typeof(UserRecommendedPage)],
            [HomePageCardSourceKind.UserSearch] = PageTypeToHeaderMap[typeof(UserSearchResultPage)],
            [HomePageCardSourceKind.UserFollowing] = PageTypeToHeaderMap[typeof(UserFollowingPage)],
            [HomePageCardSourceKind.UserFollower] = PageTypeToHeaderMap[typeof(UserFollowerPage)],
            [HomePageCardSourceKind.UserMyPixiv] = PageTypeToHeaderMap[typeof(UserMyPixivPage)],
            [HomePageCardSourceKind.Spotlight] = PageTypeToHeaderMap[typeof(SpotlightPage)],
            [HomePageCardSourceKind.SingleSeries] = PageTypeToHeaderMap[typeof(SeriesViewerPage)],
            [HomePageCardSourceKind.SingleImage] = PageTypeToHeaderMap[typeof(IllustrationViewerPage)],
            [HomePageCardSourceKind.SingleNovel] = PageTypeToHeaderMap[typeof(NovelViewerPage)],
            [HomePageCardSourceKind.SingleUser] = PageTypeToHeaderMap[typeof(UserViewerPage)]
        };

    public static (Symbol Symbol, string Header) GetPageHeader(Type pageType) => PageTypeToHeaderMap[pageType];

    public static (Symbol Symbol, string Header) GetHomeCardHeader(HomePageCardSourceKind sourceKind) => HomeCardSourceKindToHeaderMap[sourceKind];

    public static EventHandler<RoutedEventArgs>? LaunchUriTagInWebBrowser { get; }
        = (sender, e) =>
        {
            if (sender is not Control { Tag: { } parameter } s
                || TopLevel.GetTopLevel(s) is not { Launcher: { } launcher })
                return;
            if (parameter is not Uri uri)
                if (parameter is not string str || !Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out uri!))
                    return;
            _ = launcher.LaunchUriAsync(uri);
        };

    public static EventHandler<RoutedEventArgs>? LaunchFileTagInWebBrowser { get; }
        = (sender, e) =>
        {
            if (sender is not Control { Tag: { } parameter } s
                || TopLevel.GetTopLevel(s) is not { Launcher: { } launcher })
                return;
            if (parameter is not string str)
                return;
            if (new FileInfo(str) is { Exists: true } file)
                _ = launcher.LaunchFileInfoAsync(file);
            else if (new DirectoryInfo(str) is { Exists: true } directory)
                _ = launcher.LaunchDirectoryInfoAsync(directory);
        };

    public static EventHandler<RoutedEventArgs>? CopyTagToClipboard { get; }
        = (sender, e) =>
        {
            if (sender is not Control { Tag: { } parameter } s
                || TopLevel.GetTopLevel(s) is not
                {
                    ViewContainer: { } viewContainer,
                    Clipboard: { } clipboard
                })
                return;

            if (parameter is not string str)
                if (parameter is Uri uri)
                    str = uri.OriginalString;
                else
                    return;

            _ = clipboard.SetTextAsync(str)
                .ContinueWith(_ => viewContainer.ShowSuccess(I18NManager.GetResource(MiscResources.Copied)),
                    TaskScheduler.FromCurrentSynchronizationContext());
        };

    extension(Dictionary<Type, (Symbol Symbol, string Header)> dictionary)
    {
        private void Add(KeyValuePair<Type, (Symbol Symbol, string Header)> item)
        {
            dictionary[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPage"></typeparam>
    /// <param name="icon"></param>
    /// <param name="headerResource"></param>
    /// <param name="needLogin">需要登录后才能打开的页面</param>
    /// <param name="inNavigation">需要参数而无法直接从导航栏打开的页</param>
    /// <returns></returns>
    private static KeyValuePair<Type, (Symbol Symbol, string Header)> Page<TPage>(
        Symbol icon,
        string headerResource,
        bool needLogin = true,
        bool inNavigation = true)
        where TPage : Page, new()
    {
        var header = I18NManager.GetResource(headerResource);
        if (inNavigation)
        {
            var navigation = new NavigationPageDefinition(
                typeof(TPage).Name[..^4], // XXPage
                typeof(TPage),
                icon,
                headerResource,
                header,
                needLogin);
            NavigationPageRegistry.Pages.Add(navigation);
        }

        return new(typeof(TPage), (icon, header));
    }
}
