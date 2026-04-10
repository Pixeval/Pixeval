// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Pixeval.I18N;
using Pixeval.Views.Capability;
using Pixeval.Views.Settings;
using Pixeval.Views.ViewContainers;
using LoginPage = Pixeval.Views.Login.LoginPage;

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

    extension(ViewContainerBase control)
    {
        public void NavigateTo<TParameter>(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType, 
            TParameter parameter,
            bool removeCurrent = false)
        {
            var (icon, header) = _PageIconMapping[pageType];
            control.NavigateTo(pageType, new SymbolIcon
            {
                Symbol = icon,
                FontSize = 16,
                IconVariant = IconVariant.Color
            }, header, parameter, removeCurrent);
        }

        public void NavigateTo(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
            Type pageType,
            bool removeCurrent = false)
            => control.NavigateTo<object?>(pageType, null, removeCurrent);

        public void NavigateTo<TPage, TParameter>(TParameter parameter, bool removeCurrent = false) where TPage : Control, new() =>
            control.NavigateTo(typeof(TPage), parameter, removeCurrent);

        public void NavigateTo<TPage>(bool removeCurrent = false) where TPage : Control, new()
            => control.NavigateTo<TPage, object?>(null, removeCurrent);
    }

    private static readonly FrozenDictionary<Type, (Symbol Icon, string Header)> _PageIconMapping = new Dictionary<Type, (Symbol Icon, string Header)>
    {
        [typeof(LoginPage)] = (Symbol.PersonKey, I18NManager.GetResource(MainPageResources.LoginTabContent)),
        [typeof(RecommendWorksPage)] = (Symbol.Calendar, I18NManager.GetResource(MainPageResources.RecommendationsTabContent)),
        [typeof(RankingsPage)] = (Symbol.ArrowTrendingLines, I18NManager.GetResource(MainPageResources.RankingsTabContent)),
        [typeof(BookmarksPage)] = (Symbol.Library, I18NManager.GetResource(MainPageResources.BookmarksTabContent)),
        [typeof(FollowingsPage)] = (Symbol.PersonHeart, I18NManager.GetResource(MainPageResources.FollowingsTabContent)),
        [typeof(SpotlightsPage)] = (Symbol.SlideTextSparkle, I18NManager.GetResource(MainPageResources.SpotlightsTabContent)),
        [typeof(RecommendUsersPage)] = (Symbol.PeopleCommunity, I18NManager.GetResource(MainPageResources.RecommendUsersTabContent)),
        [typeof(RecentWorkPostsPage)] = (Symbol.AlertUrgent, I18NManager.GetResource(MainPageResources.RecentPostsTabContent)),
        [typeof(NewWorksPage)] = (Symbol.ArrowSync, I18NManager.GetResource(MainPageResources.NewWorksTabContent)),
        [typeof(SearchUsersPage)] = (Symbol.Person, I18NManager.GetResource(MainPageResources.SearchUsersResult)),
        [typeof(SearchWorksPage)] = (Symbol.SearchSparkle, I18NManager.GetResource(MainPageResources.SearchWorksResult)),
        [typeof(UserWorkPostsPage)] = (default, I18NManager.GetResource(MainPageResources.NewWorksTabContent)),
        // [typeof(FeedsPage)] = (Symbol.Molecule, I18NManager.GetResource(MainPageResources.FeedTabContent)),
        // [typeof(BrowsingHistoryPage)] = (Symbol.History, I18NManager.GetResource(MainPageResources.HistoriesTabContent)),
        [typeof(DownloadPage)] = (Symbol.ArrowSquareDown, I18NManager.GetResource(MainPageResources.DownloadListTabContent)),
        // [typeof(ExtensionsPage)] = (Symbol.PuzzlePiece, I18NManager.GetResource(MainPageResources.ExtensionsTabContent))
        // [typeof(HelpPage)] = (Symbol.ChatBubblesQuestion, I18NManager.GetResource(MainPageResources.HelpTabContent)),
        // [typeof(AboutPage)] = (Symbol.PersonStarburst, I18NManager.GetResource(MainPageResources.AboutTabContent)),
        [typeof(SettingsPage)] = (Symbol.Settings, I18NManager.GetResource(MainPageResources.SettingsTabContent)),
    }.ToFrozenDictionary();

    public static IReadOnlyList<NavigationInfo> HeaderItems { get; } = [.. new[]
        {
            typeof(RecommendWorksPage),
            typeof(RankingsPage),
            typeof(BookmarksPage),
            typeof(FollowingsPage),
            typeof(SpotlightsPage),
            typeof(RecommendUsersPage),
            typeof(RecentWorkPostsPage),
            typeof(NewWorksPage)
        }
        .Select(t =>
        {
            var value = _PageIconMapping[t];
            return new NavigationInfo(t, value.Icon, value.Header);
        })];

    public static IReadOnlyList<NavigationInfo> FooterItems { get; } = [.. new[]
        {
            // typeof(BrowsingHistoryPage),
            typeof(DownloadPage),
            // typeof(ExtensionsPage),
            // typeof(HelpPage),
            // typeof(AboutPage),
            typeof(SettingsPage)
        }
        .Select(t =>
        {
            var value = _PageIconMapping[t];
            return new NavigationInfo(t, value.Icon, value.Header);
        })];

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
            if (sender is not MenuItem { Tag: { } parameter } s
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

            // TODO i18n: not link
            _ = clipboard.SetTextAsync(str)
                .ContinueWith(_ => viewContainer.ShowSuccess(I18NManager.GetResource(EntryItemResources.LinkCopiedToClipboard)),
                    TaskScheduler.FromCurrentSynchronizationContext());
        };
}

public record NavigationInfo(Type PageType, Symbol Icon, string Header);

public record NavigationInfo<T>(Type PageType, Symbol Icon, string Header, T Parameter) : NavigationInfo(PageType, Icon, Header);
