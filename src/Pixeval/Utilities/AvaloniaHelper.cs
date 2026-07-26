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
        Page<HomePage>(Symbol.Home, MainPageResources.Tab.Home),
        Page<LoginPage>(Symbol.PersonKey, MainPageResources.Tab.Login, false),
        Page<SearchPage>(Symbol.SearchSparkle, MainPageResources.Tab.Search),
        Page<WorkRecommendedPage>(Symbol.Calendar, MainPageResources.Tab.WorkRecommended),
        Page<WorkRankingPage>(Symbol.ArrowTrendingLines, MainPageResources.Tab.WorkRanking),
        Page<WorkBookmarksPage>(Symbol.Library, MainPageResources.Tab.WorkBookmarks),
        Page<WorkRelatedPage>(Symbol.LightbulbFilament, MainPageResources.Tab.WorkRelated, true, false),
        Page<SeriesPage>(Symbol.LayerDiagonalPerson, MainPageResources.Tab.Series),
        Page<WorkPostsPage>(Symbol.Image, MainPageResources.Tab.WorkPosts),
        Page<WorkSearchResultPage>(Symbol.SearchSparkle, MainPageResources.Tab.WorkSearchResult, true, false),
        Page<UserFollowingPage>(Symbol.PersonHeart, MainPageResources.Tab.UserFollowing),
        Page<SpotlightPage>(Symbol.SlideTextSparkle, MainPageResources.Tab.Spotlight),
        Page<UserRecommendedPage>(Symbol.PeopleCommunity, MainPageResources.Tab.UserRecommended),
        Page<UserSearchResultPage>(Symbol.Person, MainPageResources.Tab.UserSearchResult, true, false),
        Page<UserFollowerPage>(Symbol.People, MainPageResources.Tab.UserFollower),
        Page<UserMyPixivPage>(Symbol.PeopleInterwoven, MainPageResources.Tab.UserMyPixiv),
        Page<RelatedUsersPage>(Symbol.PeopleCommunity, MainPageResources.Tab.RelatedUser),
        Page<WorkFollowingPage>(Symbol.AlertUrgent, MainPageResources.Tab.WorkFollowing),
        Page<WorkMyPixivPage>(Symbol.Molecule, MainPageResources.Tab.WorkMyPixiv),
        Page<WorkNewPage>(Symbol.ArrowSync, MainPageResources.Tab.WorkNew),
        Page<BrowsingHistoryPage>(Symbol.History, MainPageResources.Tab.BrowsingHistory),
        Page<WatchLaterPage>(Symbol.Clock, MainPageResources.Tab.WatchLater),
        Page<DownloadPage>(Symbol.ArrowSquareDown, MainPageResources.Tab.Download),
        Page<ExtensionsPage>(Symbol.PuzzlePiece, MainPageResources.Tab.Extensions, false),
        Page<SettingsPage>(Symbol.Settings, MainPageResources.Tab.Settings, false),
        Page<AboutPage>(Symbol.PersonStarburst, MainPageResources.Tab.About, false, false),
        Page<HelpPage>(Symbol.ChatBubblesQuestion, MainPageResources.Tab.Help, false, false),
        Page<IllustrationViewerPage>(Symbol.Image, MainPageResources.Tab.SingleImage, true, false),
        Page<NovelViewerPage>(Symbol.BookOpen, MainPageResources.Tab.SingleNovel, true, false),
        Page<UserViewerPage>(Symbol.Person, MainPageResources.Tab.SingleUser, true, false),
        Page<SeriesViewerPage>(Symbol.ListBar, MainPageResources.Tab.SingleSeries, true, false),
        Page<WorkInfoPage>(Symbol.BookContacts, MainPageResources.Tab.WorkInfo, true, false),
        Page<CommentsPage>(Symbol.ChatMultiple, MainPageResources.Tab.Comments, true, false)
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
