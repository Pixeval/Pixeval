// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
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
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;

namespace Pixeval.Pages;

public partial class MainPageViewModel : ObservableObject
{
    public readonly NavigationViewTag<RecommendationPage> RecommendationsTag = new();

    public readonly NavigationViewTag<RankingsPage> RankingsTag = new();

    public readonly NavigationViewTag<BookmarksPage> BookmarksTag = new();

    public readonly NavigationViewTag<FollowingsPage> FollowingsTag = new();

    public readonly NavigationViewTag<SpotlightsPage> SpotlightsTag = new();

    public readonly NavigationViewTag<RecommendUsersPage> RecommendUsersTag = new();

    public readonly NavigationViewTag<RecentPostsPage> RecentPostsTag = new();

    public readonly NavigationViewTag<NewWorksPage> NewWorksTag = new();
    
    public readonly NavigationViewTag<FeedPage> FeedTag = new();

    public readonly NavigationViewTag<TagsPage> TagsTag = new();

    public readonly NavigationViewTag<BrowsingHistoryPage> HistoriesTag = new();

    public readonly NavigationViewTag<DownloadPage> DownloadListTag = new();

    public readonly NavigationViewTag<ExtensionsPage> ExtensionsTag = new();

    public readonly NavigationViewTag<HelpPage> HelpTag = new();

    public readonly NavigationViewTag<AboutPage> AboutTag = new();

    public readonly NavigationViewTag<SettingsPage> SettingsTag = new();

    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; } = App.AppViewModel.MakoClient.Me.Name;

    private readonly FrameworkElement _owner;

    public MainPageViewModel(FrameworkElement owner)
    {
        _owner = owner;
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
            OnEventAction = (m, changed, arg) => _owner.DispatcherQueue.TryEnqueue(async () =>
            {
                AvatarSource = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync(arg.ProfileImageUrls.Px50X50);
                UserName = arg.Name;
            }),
            OnDetachAction = listener => makoClient.TokenRefreshed -= listener.OnEvent
        };
        makoClient.TokenRefreshed += _tokenRefreshedListener.OnEvent;

        _tokenRefreshFailedListener?.Detach();
        _tokenRefreshFailedListener = new(App.AppViewModel.MakoClient)
        {
            OnEventAction = (m, changed, arg) => _owner.DispatcherQueue.TryEnqueue(() =>
                _ = _owner.CreateAcknowledgementAsync(
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
                    _ = _owner.CreateAcknowledgementAsync(MainPageResources.ReverseSearchNotFoundTitle,
                            MainPageResources.ReverseSearchNotFoundContent);
                else
                    viewModels[0].CreateWindowWithPage(viewModels);
            }
            else
            {
                _ = await _owner.CreateAcknowledgementAsync(MainPageResources.ReverseSearchErrorTitle,
                    result.Header.Status > 0
                        ? MainPageResources.ReverseSearchServerSideErrorContent
                        : MainPageResources.ReverseSearchClientSideErrorContent);
            }
        }
        catch (Exception e)
        {
            _ = await _owner.CreateAcknowledgementAsync(MiscResources.ExceptionEncountered, e.ToString());
        }
    }
}
