// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Model;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Capability.Feeds;
using Pixeval.Pages.Download;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.Misc;
using Pixeval.Pages.Tags;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using IllustrationCacheTable = Pixeval.Caching.CacheTable<
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheKey,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheHeader,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheProtocol>;

namespace Pixeval.Pages;

public partial class MainPageViewModel : UiObservableObject
{
    public readonly NavigationViewTag<RecommendationPage> RecommendationsTag =
        new(MainPageResources.RecommendationsTabContent) { ImageUri = GetIconUri("recommendations") };

    public readonly NavigationViewTag<RankingsPage> RankingsTag =
        new(MainPageResources.RankingsTabContent) { ImageUri = GetIconUri("ranking") };

    public readonly NavigationViewTag<BookmarksPage> BookmarksTag =
        new(MainPageResources.BookmarksTabContent) { ImageUri = GetIconUri("bookmarks") };

    public readonly NavigationViewTag<FollowingsPage> FollowingsTag =
        new(MainPageResources.FollowingsTabContent) { ImageUri = GetIconUri("followings") };

    public readonly NavigationViewTag<SpotlightsPage> SpotlightsTag =
        new(MainPageResources.SpotlightsTabContent) { ImageUri = GetIconUri("spotlight") };

    public readonly NavigationViewTag<RecommendUsersPage> RecommendUsersTag =
        new(MainPageResources.RecommendUsersTabContent) { ImageUri = GetIconUri("recommend-user") };

    public readonly NavigationViewTag<RecentPostsPage> RecentPostsTag =
        new(MainPageResources.RecentPostsTabContent) { ImageUri = GetIconUri("recent-posts") };

    public readonly NavigationViewTag<NewWorksPage> NewWorksTag =
        new(MainPageResources.NewWorksTabContent) { ImageUri = GetIconUri("new-works") };

    public readonly NavigationViewTag<FeedPage> FeedTag =
        new(MainPageResources.FeedTabContent) { ImageUri = GetIconUri("feed") };

    public readonly NavigationViewTag<BrowsingHistoryPage> HistoriesTag =
        new(MainPageResources.HistoriesTabContent) { ImageUri = GetIconUri("history") };

    public readonly NavigationViewTag<DownloadPage> DownloadListTag =
        new(MainPageResources.DownloadListTabContent) { ImageUri = GetIconUri("download-list") };

    public readonly NavigationViewTag<TagsPage> TagsTag =
        new(MainPageResources.TagsTabContent) { ImageUri = GetIconUri("tag") };

    public readonly NavigationViewTag<ExtensionsPage> ExtensionsTag =
        new(MainPageResources.ExtensionsTabContent) { ImageUri = GetIconUri("extensions") };

    public readonly NavigationViewTag<HelpPage> HelpTag =
        new(MainPageResources.HelpTabContent) { ImageUri = GetIconUri("help") };

    public readonly NavigationViewTag<AboutPage> AboutTag =
        new(MainPageResources.AboutTabContent) { ImageUri = GetIconUri("about") };

    public readonly NavigationViewTag<SettingsPage> SettingsTag;

    public static NavigationViewTag<SettingsPage> GetSettingsTag(object? parameter = null) => 
        new(MainPageResources.SettingsTabContent, parameter) { ImageUri = GetIconUri("settings") };

    public readonly NavigationViewTag<SearchUsersPage> SearchUsersTag = new(MainPageResources.SearchUsersResult);

    public readonly NavigationViewTag<SearchWorksPage> SearchWorksTag = new(MainPageResources.SearchWorksResult);

    public IReadOnlyList<INavigationViewItem> MenuItems =>
    [
        RecommendationsTag,
        RankingsTag,
        BookmarksTag,
        FollowingsTag,
        SpotlightsTag,
        RecommendUsersTag,
        RecentPostsTag,
        NewWorksTag,
        FeedTag,
        new NavigationViewSeparator(),
        HistoriesTag,
        DownloadListTag
    ];

    public IReadOnlyList<INavigationViewItem> FooterMenuItems =>
    [
        TagsTag,
        ExtensionsTag,
        HelpTag,
        AboutTag,
        new NavigationViewSeparator(),
        SettingsTag
    ];

    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; } = App.AppViewModel.MakoClient.Me.Name;

    public MainPageViewModel(FrameworkElement owner) : base(owner)
    {
        SettingsTag = GetSettingsTag();
        SetAvatarSource();
        SubscribeTokenRefresh();
    }

    private async void SetAvatarSource()
    {
        AvatarSource = await App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationCacheTable>()
            .GetSourceFromCacheAsync(App.AppViewModel.MakoClient.Me.ProfileImageUrls.Px50X50);
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
                AvatarSource = await App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationCacheTable>().GetSourceFromCacheAsync(arg.ProfileImageUrls.Px50X50);
                UserName = arg.Name;
            }),
            OnDetachAction = listener => makoClient.TokenRefreshed -= listener.OnEvent
        };
        makoClient.TokenRefreshed += _tokenRefreshedListener.OnEvent;

        _tokenRefreshFailedListener?.Detach();
        _tokenRefreshFailedListener = new(App.AppViewModel.MakoClient)
        {
            OnEventAction = (m, changed, arg) => FrameworkElement.DispatcherQueue.TryEnqueue(() =>
                _ = FrameworkElement.CreateAcknowledgementAsync(
                    MainPageResources.RefreshingSessionFailedTitle,
                    MainPageResources.RefreshingSessionFailedContent)),
            OnDetachAction = listener => makoClient.TokenRefreshedFailed -= listener.OnEvent
        };
        makoClient.TokenRefreshedFailed += _tokenRefreshFailedListener.OnEvent;
    }

    public async Task ReverseSearchAsync(Stream stream)
    {
        try
        {
            var result = await App.AppViewModel.MakoClient.ReverseSearchAsync(stream, App.AppViewModel.AppSettings.ReverseSearchApiKey);
            if (result.Header.Status is 0)
            {
                var viewModels = await Task.WhenAll(result.Results
                    .Where(r => r.Header.IndexId is 5 or 6 && r.Header.Similarity >
                        App.AppViewModel.AppSettings.ReverseSearchResultSimilarityThreshold)
                    .Select(async r =>
                        new IllustrationItemViewModel(
                            await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(r.Data.PixivId))));

                if (viewModels.Length is 0)
                    _ = FrameworkElement.CreateAcknowledgementAsync(MainPageResources.ReverseSearchNotFoundTitle,
                            MainPageResources.ReverseSearchNotFoundContent);
                else
                    FrameworkElement.CreateIllustrationPage(viewModels[0], viewModels);
            }
            else
            {
                _ = await FrameworkElement.CreateAcknowledgementAsync(MainPageResources.ReverseSearchErrorTitle,
                    result.Header.Status > 0
                        ? MainPageResources.ReverseSearchServerSideErrorContent
                        : MainPageResources.ReverseSearchClientSideErrorContent);
            }
        }
        catch (Exception e)
        {
            _ = await FrameworkElement.CreateAcknowledgementAsync(MiscResources.ExceptionEncountered, e.ToString());
        }
    }

    private static Uri GetIconUri(string iconName)
    {
        return new($"ms-appx:///Assets/Images/Icons/{iconName}.png");
    }
}
