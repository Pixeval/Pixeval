// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Pixeval.Models.Options;
using Pixeval.Views;
using Pixeval.Views.Capability;
using Pixeval.Views.Download;
using Pixeval.Views.Home;
using Pixeval.Views.Login;
using Pixeval.Views.Search;
using Pixeval.Views.Settings;
using Pixeval.Views.ViewContainers;

namespace Pixeval.Utilities;

public static class AvaloniaHelper
{
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
        new()
        {
            [typeof(HomePage)] = (Symbol.Home, I18NManager.GetResource(MainPageResources.TabHome)),
            [typeof(LoginPage)] = (Symbol.PersonKey, I18NManager.GetResource(MainPageResources.TabLogin)),
            [typeof(SearchPage)] = (Symbol.SearchSparkle, I18NManager.GetResource(MainPageResources.TabSearch)),
            [typeof(WorkRecommendedPage)] = (Symbol.Calendar, I18NManager.GetResource(MainPageResources.TabWorkRecommended)),
            [typeof(WorkRankingPage)] = (Symbol.ArrowTrendingLines, I18NManager.GetResource(MainPageResources.TabWorkRanking)),
            [typeof(WorkBookmarksPage)] = (Symbol.Library, I18NManager.GetResource(MainPageResources.TabWorkBookmarks)),
            [typeof(WorkRelatedPage)] = (Symbol.LightbulbFilament, I18NManager.GetResource(MainPageResources.TabWorkRelated)),
            [typeof(WorkPostsPage)] = (Symbol.Image, I18NManager.GetResource(MainPageResources.TabWorkPosts)),
            [typeof(WorkSearchResultPage)] = (Symbol.SearchSparkle, I18NManager.GetResource(MainPageResources.TabWorkSearch)),
            [typeof(UserFollowingPage)] = (Symbol.PersonHeart, I18NManager.GetResource(MainPageResources.TabUserFollowing)),
            [typeof(SpotlightPage)] = (Symbol.SlideTextSparkle, I18NManager.GetResource(MainPageResources.TabSpotlight)),
            [typeof(UserRecommendPage)] = (Symbol.PeopleCommunity, I18NManager.GetResource(MainPageResources.TabUserRecommended)),
            [typeof(UserSearchPage)] = (Symbol.Person, I18NManager.GetResource(MainPageResources.TabUserSearch)),
            [typeof(UserFollowerPage)] = (Symbol.People, I18NManager.GetResource(MainPageResources.TabUserFollower)),
            [typeof(UserMyPixivPage)] = (Symbol.PeopleInterwoven, I18NManager.GetResource(MainPageResources.TabUserMyPixiv)),
            [typeof(RelatedUsersPage)] = (Symbol.PeopleCommunity, I18NManager.GetResource(MainPageResources.TabRelatedUser)),
            [typeof(WorkFollowingPage)] = (Symbol.AlertUrgent, I18NManager.GetResource(MainPageResources.TabWorkFollowing)),
            [typeof(WorkMyPixivPage)] = (Symbol.Molecule, I18NManager.GetResource(MainPageResources.TabWorkMyPixiv)),
            [typeof(WorkNewPage)] = (Symbol.ArrowSync, I18NManager.GetResource(MainPageResources.TabWorkNew)),
            [typeof(BrowsingHistoryPage)] = (Symbol.History, I18NManager.GetResource(MainPageResources.TabBrowsingHistory)),
            [typeof(WatchLaterPage)] = (Symbol.Clock, I18NManager.GetResource(MainPageResources.TabWatchLater)),
            [typeof(DownloadPage)] = (Symbol.ArrowSquareDown, I18NManager.GetResource(MainPageResources.TabDownload)),
            [typeof(ExtensionsPage)] = (Symbol.PuzzlePiece, I18NManager.GetResource(MainPageResources.TabExtensions)),
            [typeof(SettingsPage)] = (Symbol.Settings, I18NManager.GetResource(MainPageResources.TabSettings)),
            [typeof(AboutPage)] = (Symbol.PersonStarburst, I18NManager.GetResource(MainPageResources.TabAbout)),
            [typeof(HelpPage)] = (Symbol.ChatBubblesQuestion, I18NManager.GetResource(MainPageResources.TabHelp))
        };

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
            [HomePageCardSourceKind.UserRecommended] = PageTypeToHeaderMap[typeof(UserRecommendPage)],
            [HomePageCardSourceKind.UserSearch] = PageTypeToHeaderMap[typeof(UserSearchPage)],
            [HomePageCardSourceKind.UserFollowing] = PageTypeToHeaderMap[typeof(UserFollowingPage)],
            [HomePageCardSourceKind.UserFollower] = PageTypeToHeaderMap[typeof(UserFollowerPage)],
            [HomePageCardSourceKind.UserMyPixiv] = PageTypeToHeaderMap[typeof(UserMyPixivPage)],
            [HomePageCardSourceKind.Spotlight] = PageTypeToHeaderMap[typeof(SpotlightPage)],
            [HomePageCardSourceKind.SingleImage] = (Symbol.Image, I18NManager.GetResource(MainPageResources.TabSingleImage)),
            [HomePageCardSourceKind.SingleNovel] = (Symbol.BookOpen, I18NManager.GetResource(MainPageResources.TabSingleNovel)),
            [HomePageCardSourceKind.SingleUser] = (Symbol.Person, I18NManager.GetResource(MainPageResources.TabSingleUser))
        };

    public static (Symbol Symbol, string Header) GetPageHeader(Type pageType) => PageTypeToHeaderMap[pageType];

    public static (Symbol Symbol, string Header) GetHomeCardHeader(HomePageCardSourceKind sourceKind) => HomeCardSourceKindToHeaderMap[sourceKind];

    public static IReadOnlyList<NavigationInfo> HeaderItems { get; } =
    [
        // new(typeof(HomePage)),
        new(typeof(SearchPage)),
        new(typeof(WorkRecommendedPage)),
        new(typeof(WorkRankingPage)),
        new(typeof(WorkBookmarksPage)),
        new(typeof(UserFollowingPage)),
        new(typeof(SpotlightPage)),
        new(typeof(UserRecommendPage)),
        new(typeof(UserFollowerPage)),
        new(typeof(UserMyPixivPage)),
        new(typeof(WorkFollowingPage)),
        new(typeof(WorkMyPixivPage)),
        new(typeof(WorkNewPage)),
        // new (typeof(FeedsPage)),
    ];

    public static IReadOnlyList<NavigationInfo> FooterItems { get; } =
    [
        new(typeof(BrowsingHistoryPage)),
        new(typeof(WatchLaterPage)),
        new(typeof(DownloadPage)),
        new(typeof(ExtensionsPage), false),
        new(typeof(SettingsPage), false)
    ];

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
}

public record NavigationInfo
{
    public NavigationInfo(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type PageType,
        bool NeedLogin = true)
    {
        this.PageType = PageType;
        this.NeedLogin = NeedLogin;
        var tuple = AvaloniaHelper.GetPageHeader(PageType);
        Header = tuple.Header;
        Icon = tuple.Symbol;
    }

    public string Header { get; }

    public Symbol Icon { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type PageType { get; }

    public bool NeedLogin { get; }
}
