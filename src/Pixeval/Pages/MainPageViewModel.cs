// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Mako;
using Mako.Model;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Capability.Feeds;
using Pixeval.Pages.Download;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.Misc;
using Pixeval.Pages.Tags;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Pages;

public partial class MainPageViewModel : UiObservableObject
{
    [ObservableProperty]
    public partial bool RestrictedModeProcessing { get; set; }

    [ObservableProperty]
    public partial bool AiShowProcessing { get; set; }

    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; private set; }

    [ObservableProperty]
    public partial string UserName { get; private set; } = App.AppViewModel.MakoClient.Me.Name;

    [ObservableProperty]
    public partial bool IsPremium { get; private set; } = App.AppViewModel.MakoClient.Me.IsPremium;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Url))]
    public partial long Id { get; private set; } = App.AppViewModel.MakoClient.Me.Id;

    public Uri Url => MakoHelper.GenerateUserWebUri(Id);

    public NavigationViewTag<FeedPage> FeedTag { get; } = new(MainPageResources.FeedTabContent) { ImageUri = GetIconUri("feed") };

    public NavigationViewTag<ExtensionsPage> ExtensionsTag { get; } = new(MainPageResources.ExtensionsTabContent) { ImageUri = GetIconUri("extensions"), ShowIconBadge = App.AppViewModel.VersionContext.NeverUsedExtensions };

    public NavigationViewTag<SettingsPage> SettingsTag { get; } = GetSettingsTag();

    public static NavigationViewTag<SettingsPage> GetSettingsTag(object? parameter = null) =>
        new(MainPageResources.SettingsTabContent, parameter) { ImageUri = GetIconUri("settings") };

    public NavigationViewTag<SearchUsersPage> SearchUsersTag { get; } = new(MainPageResources.SearchUsersResult);

    public NavigationViewTag<SearchWorksPage> SearchWorksTag { get; } = new(MainPageResources.SearchWorksResult);

    public IReadOnlyList<INavigationViewItem> MenuItems =>
    [
        new NavigationViewTag<RecommendationPage>(MainPageResources.RecommendationsTabContent) { ImageUri = GetIconUri("recommendations") },
        new NavigationViewTag<RankingsPage>(MainPageResources.RankingsTabContent) { ImageUri = GetIconUri("ranking") },
        new NavigationViewTag<BookmarksPage>(MainPageResources.BookmarksTabContent) { ImageUri = GetIconUri("bookmarks") },
        new NavigationViewTag<FollowingsPage>(MainPageResources.FollowingsTabContent) { ImageUri = GetIconUri("followings") },
        new NavigationViewTag<SpotlightsPage>(MainPageResources.SpotlightsTabContent) { ImageUri = GetIconUri("spotlight") },
        new NavigationViewTag<RecommendUsersPage>(MainPageResources.RecommendUsersTabContent) { ImageUri = GetIconUri("recommend-user") },
        new NavigationViewTag<RecentPostsPage>(MainPageResources.RecentPostsTabContent) { ImageUri = GetIconUri("recent-posts") },
        new NavigationViewTag<NewWorksPage>(MainPageResources.NewWorksTabContent) { ImageUri = GetIconUri("new-works") },
        FeedTag,
        new NavigationViewSeparator(),
        new NavigationViewTag<BrowsingHistoryPage>(MainPageResources.HistoriesTabContent) { ImageUri = GetIconUri("history") },
        new NavigationViewTag<DownloadPage>(MainPageResources.DownloadListTabContent) { ImageUri = GetIconUri("download-list") }
    ];

    public IReadOnlyList<INavigationViewItem> FooterMenuItems =>
    [
        new NavigationViewTag<TagsPage>(MainPageResources.TagsTabContent) { ImageUri = GetIconUri("tag") },
        ExtensionsTag,
        new NavigationViewTag<HelpPage>(MainPageResources.HelpTabContent) { ImageUri = GetIconUri("help") },
        new NavigationViewTag<AboutPage>(MainPageResources.AboutTabContent) { ImageUri = GetIconUri("about") },
        new NavigationViewSeparator(),
        SettingsTag
    ];

    public MainPageViewModel(FrameworkElement owner) : base(owner)
    {
        SubscribeTokenRefresh();
    }

    public double MainPageRootNavigationViewOpenPanelLength => 280;

    public SuggestionStateMachine SuggestionProvider { get; } = new();

    private WeakEventListener<MakoClient, object?, TokenUser> _tokenRefreshedListener = null!;

    private WeakEventListener<MakoClient, object?, Exception> _tokenRefreshFailedListener = null!;

    public void SubscribeTokenRefresh()
    {
        var makoClient = App.AppViewModel.MakoClient;
        _tokenRefreshedListener?.Detach();
        _tokenRefreshedListener = new(App.AppViewModel.MakoClient)
        {
            OnEventAction = (m, changed, arg) => FrameworkElement.DispatcherQueue.TryEnqueue(async () =>
            {
                AvatarSource = await CacheHelper.GetSourceFromCacheAsync(arg.ProfileImageUrls.Px50X50);
                UserName = arg.Name;
                IsPremium = arg.IsPremium;
                Id = arg.Id;
            }),
            OnDetachAction = listener => makoClient.TokenRefreshed -= listener.OnEvent
        };
        makoClient.TokenRefreshed += _tokenRefreshedListener.OnEvent;

        _tokenRefreshFailedListener?.Detach();
        _tokenRefreshFailedListener = new(App.AppViewModel.MakoClient)
        {
            OnEventAction = (m, changed, arg) => FrameworkElement.DispatcherQueue.TryEnqueue(() =>
                FrameworkElement.ErrorGrowl(
                    MainPageResources.RefreshingSessionFailedTitle,
                    MainPageResources.RefreshingSessionFailedContent)),
            OnDetachAction = listener => makoClient.TokenRefreshedFailed -= listener.OnEvent
        };
        makoClient.TokenRefreshedFailed += _tokenRefreshFailedListener.OnEvent;
    }

    public async void TryLoadAvatar()
    {
        if (AvatarSource is not null || App.AppViewModel.MakoClient.TryGetMe() is not { } me)
            return;
        await Task.Yield();
        try
        {
            AvatarSource = await CacheHelper.GetSourceFromCacheAsync(me.ProfileImageUrls.Px50X50);
        }
        catch
        {
            // ignored
        }
    }

    public async Task ReverseSearchAsync(Stream stream)
    {
        try
        {
            FrameworkElement.InfoGrowl(MainPageResources.ReverseSearchStartContent);
            var result = await App.AppViewModel.MakoClient.ReverseSearchAsync(stream, App.AppViewModel.AppSettings.ReverseSearchApiKey);
            if (result.Header.Status is 0)
            {
                var viewModels = await Task.WhenAll(result.Results
                    .Where(r => r.Header.IndexId is 5 /*pixiv*/ or 6 /*pixivhistorical*/ && r.Header.Similarity >
                        App.AppViewModel.AppSettings.ReverseSearchResultSimilarityThreshold)
                    .Select(async r =>
                        new IllustrationItemViewModel(
                            await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(r.Data.PixivId))));

                if (viewModels.Length is 0)
                    FrameworkElement.ErrorGrowl(MainPageResources.ReverseSearchNotFoundTitle,
                            MainPageResources.ReverseSearchNotFoundContent);
                else
                    FrameworkElement.CreateIllustrationPage(viewModels[0], viewModels);
            }
            else
            {
                FrameworkElement.ErrorGrowl(MainPageResources.ReverseSearchErrorTitle,
                    result.Header.Status > 0
                        ? MainPageResources.ReverseSearchServerSideErrorContent
                        : MainPageResources.ReverseSearchClientSideErrorContent);
            }
        }
        catch (Exception e)
        {
            FrameworkElement.ErrorGrowl(MiscResources.ExceptionEncountered, e.ToString());
        }
    }

    private static Uri GetIconUri(string iconName) => new($"ms-appx:///Assets/Images/Icons/{iconName}.png");
}
