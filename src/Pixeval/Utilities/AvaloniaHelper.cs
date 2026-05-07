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
using Pixeval.Views.Settings;
using Pixeval.Views.ViewContainers;
using DownloadPage = Pixeval.Views.Download.DownloadPage;
using SearchPage = Pixeval.Views.Search.SearchPage;

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
        new(typeof(RecommendWorksPage), Symbol.Home, I18NManager.GetResource(MainPageResources.HomeTabContent)),
        new(typeof(SearchPage), Symbol.SearchSparkle, I18NManager.GetResource(MainPageResources.SearchTabContent)),
        new(typeof(RecommendWorksPage), Symbol.Calendar, I18NManager.GetResource(MainPageResources.RecommendationsTabContent)),
        new(typeof(RankingsPage), Symbol.ArrowTrendingLines, I18NManager.GetResource(MainPageResources.RankingsTabContent)),
        new(typeof(BookmarksPage), Symbol.Library, I18NManager.GetResource(MainPageResources.BookmarksTabContent)),
        new(typeof(FollowingsPage), Symbol.PersonHeart, I18NManager.GetResource(MainPageResources.FollowingsTabContent)),
        new(typeof(SpotlightsPage), Symbol.SlideTextSparkle, I18NManager.GetResource(MainPageResources.SpotlightsTabContent)),
        new(typeof(RecommendUsersPage), Symbol.PeopleCommunity, I18NManager.GetResource(MainPageResources.RecommendUsersTabContent)),
        new(typeof(RecentWorkPostsPage), Symbol.AlertUrgent, I18NManager.GetResource(MainPageResources.RecentPostsTabContent)),
        new(typeof(NewWorksPage), Symbol.ArrowSync, I18NManager.GetResource(MainPageResources.NewWorksTabContent)),
        // new (typeof(FeedsPage), Symbol.Molecule, I18NManager.GetResource(MainPageResources.FeedTabContent)),
    ];

    public static IReadOnlyList<NavigationInfo> FooterItems { get; } =
    [
        new (typeof(BrowsingHistoryPage), Symbol.History, I18NManager.GetResource(MainPageResources.HistoriesTabContent)),
        new (typeof(DownloadPage), Symbol.ArrowSquareDown, I18NManager.GetResource(MainPageResources.DownloadListTabContent)),
        new (typeof(ExtensionsPage), Symbol.PuzzlePiece, I18NManager.GetResource(MainPageResources.ExtensionsTabContent)),
        new (typeof(HelpPage), Symbol.ChatBubblesQuestion, I18NManager.GetResource(MainPageResources.HelpTabContent)),
        new (typeof(SettingsPage), Symbol.Settings, I18NManager.GetResource(MainPageResources.SettingsTabContent))
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
    string Header);
