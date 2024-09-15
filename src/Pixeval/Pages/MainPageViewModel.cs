#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MainPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Capability.Feeds;
using Pixeval.Pages.Download;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.Misc;
using Pixeval.Pages.Tags;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages;

public partial class MainPageViewModel : ObservableObject
{
    public readonly NavigationViewTag<HelpPage> HelpTag = new();

    public readonly NavigationViewTag<AboutPage> AboutTag = new();

    public readonly NavigationViewTag<BookmarksPage> BookmarksTag = new();

    public readonly NavigationViewTag<FollowingsPage> FollowingsTag = new();

    public readonly NavigationViewTag<BrowsingHistoryPage> HistoriesTag = new();

    public readonly NavigationViewTag<RankingsPage> RankingsTag = new();

    public readonly NavigationViewTag<RecentPostsPage> RecentPostsTag = new();

    public readonly NavigationViewTag<NewWorksPage> NewWorksTag = new();
    
    public readonly NavigationViewTag<FeedPage> FeedTag = new();

    public readonly NavigationViewTag<RecommendationPage> RecommendsTag = new();

    public readonly NavigationViewTag<RecommendUsersPage> RecommendUsersTag = new();

    public readonly NavigationViewTag<TagsPage> TagsTag = new();

    public readonly NavigationViewTag<DownloadPage> DownloadListTag = new();

    public readonly NavigationViewTag<SettingsPage> SettingsTag = new();

    public readonly NavigationViewTag<SpotlightsPage> SpotlightsTag = new();

    public readonly IconElement RecommendationIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("recommendations-128x128"))};

    public readonly IconElement RankingIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("ranking-128x128")) };

    // Due to my incompetence, the sizes of the icons must be adjusted per item basis in order to ensure the visual coherence, which is quite annoying.
    // This was supposed to be an easy fix, but I'm already exhausted now. 

    public readonly IconElement BookmarksIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("bookmarks-128x128"))};

    public readonly IconElement FollowingsIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("followings-128x128")) };

    public readonly IconElement SpotlightIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("spotlight-128x128")) };

    public readonly IconElement RecommendUserIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("recommend-user-128x128")) };

    public readonly IconElement RecentPostsIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("recent-posts-128x128")) };

    public readonly IconElement NewWorksIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("new-works-128x128")) };

    public readonly IconElement FeedIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("feed-128x128")) };

    public readonly IconElement TagIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("tag-128x128")) };

    public readonly IconElement HistoryIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("history-128x128")) };

    public readonly IconElement DownloadListIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("download-list-128x128")) };

    public readonly IconElement HelpIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("help-128x128")) };

    public readonly IconElement AboutIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("about-128x128")) };

    public readonly IconElement SettingsIcon = new ImageIcon { Source = new BitmapImage(AppInfo.NavigationIconUri("settings-128x128")) };

    [ObservableProperty]
    private SoftwareBitmapSource? _avatarSource;

    public string UserName => App.AppViewModel.MakoClient.Session.Name;

    private readonly UIElement _owner;

    public MainPageViewModel(UIElement owner)
    {
        _owner = owner;
        DownloadAndSetAvatar();
    }

    public double MainPageRootNavigationViewOpenPanelLength => 280;

    public SuggestionStateMachine SuggestionProvider { get; } = new();

    /// <summary>
    /// Download user's avatar and set to the Avatar property.
    /// </summary>
    public async void DownloadAndSetAvatar()
    {
        var makoClient = App.AppViewModel.MakoClient;
        // get byte array of avatar
        // and set to the bitmap image
        var result = await makoClient.DownloadSoftwareBitmapSourceAsync(makoClient.Session.AvatarUrl!);
        AvatarSource = result is Result<SoftwareBitmapSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.ImageNotAvailable;
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
