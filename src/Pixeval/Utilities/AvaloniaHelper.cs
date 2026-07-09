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
using Pixeval.Views;
using Pixeval.Views.Capability;
using Pixeval.Views.Download;
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

    public static IReadOnlyList<NavigationInfo> HeaderItems { get; } =
    [
        // new(typeof(HomePage), Symbol.Home, MainPageResources.TabHome),
        new(typeof(SearchPage), Symbol.SearchSparkle, MainPageResources.TabSearch),
        new(typeof(WorkRecommendedPage), Symbol.Calendar, MainPageResources.TabWorkRecommended),
        new(typeof(WorkRankingPage), Symbol.ArrowTrendingLines, MainPageResources.TabWorkRanking),
        new(typeof(WorkBookmarksPage), Symbol.Library, MainPageResources.TabWorkBookmarks),
        new(typeof(UserFollowingPage), Symbol.PersonHeart, MainPageResources.TabUserFollowing),
        new(typeof(SpotlightPage), Symbol.SlideTextSparkle, MainPageResources.TabSpotlight),
        new(typeof(UserRecommendPage), Symbol.PeopleCommunity, MainPageResources.TabUserRecommended),
        new(typeof(UserFollowerPage), Symbol.People, MainPageResources.TabUserFollower),
        new(typeof(UserMyPixivPage), Symbol.PeopleInterwoven, MainPageResources.TabUserMyPixiv),
        new(typeof(WorkFollowingPage), Symbol.AlertUrgent, MainPageResources.TabWorkFollowing),
        new(typeof(WorkMyPixivPage), Symbol.Molecule, MainPageResources.TabWorkMyPixiv),
        new(typeof(WorkNewPage), Symbol.ArrowSync, MainPageResources.TabWorkNew),
        // new (typeof(FeedsPage), Symbol.Molecule, MainPageResources.FeedTabContent),
    ];

    public static IReadOnlyList<NavigationInfo> FooterItems { get; } =
    [
        new(typeof(BrowsingHistoryPage), Symbol.History, MainPageResources.TabBrowsingHistory),
        new(typeof(WatchLaterPage), Symbol.Clock, MainPageResources.TabWatchLater),
        new(typeof(DownloadPage), Symbol.ArrowSquareDown, MainPageResources.TabDownload),
        new(typeof(ExtensionsPage), Symbol.PuzzlePiece, MainPageResources.TabExtensions, false),
        new(typeof(SettingsPage), Symbol.Settings, MainPageResources.TabSettings, false)
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

public record NavigationInfo(
    [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type PageType,
    Symbol Icon,
    string Header,
    bool NeedLogin = true)
{
    public string Header { get; } = I18NManager.GetResource(Header);
}
