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
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.IllustrationView;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Misc;
using Pixeval.Pages.Capability;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.Pages;

public partial class MainPageViewModel : ObservableObject
{
    public readonly NavigationViewTag AboutTag = new(typeof(AboutPage), null);

    public readonly NavigationViewTag BookmarksTag = new(typeof(BookmarksPage),
        App.AppViewModel.MakoClient.Bookmarks(App.AppViewModel.PixivUid, PrivacyPolicy.Public,
            App.AppViewModel.AppSetting.TargetFilter));

    public readonly NavigationViewTag FollowingsTag = new(typeof(FollowingsPage), null);

    public readonly NavigationViewTag HistoriesTag = new(typeof(BrowsingHistoryPage), null);

    public readonly NavigationViewTag RankingsTag = new(typeof(RankingsPage),
        App.AppViewModel.MakoClient.Ranking(RankOption.Day, DateTime.Today - TimeSpan.FromDays(2)));

    public readonly NavigationViewTag RecentPostsTag = new(typeof(RecentPostsPage),
        App.AppViewModel.MakoClient.RecentPosts(PrivacyPolicy.Public));

    public readonly NavigationViewTag RecommendsTag = new(typeof(RecommendationPage),
        App.AppViewModel.MakoClient.Recommendations(targetFilter: App.AppViewModel.AppSetting.TargetFilter));

    public readonly NavigationViewTag SettingsTag =
        new(typeof(SettingsPage), App.AppViewModel.MakoClient.Configuration);

    public readonly NavigationViewTag SpotlightsTag = new(typeof(SpotlightsPage), null);

    [ObservableProperty]
    private SoftwareBitmapSource? _avatar;

    private readonly UIElement _owner;

    public MainPageViewModel(UIElement owner)
    {
        _owner = owner;
        DownloadAndSetAvatar();
    }

    public double MainPageRootNavigationViewOpenPanelLength => 280;

    public SuggestionStateMachine SuggestionProvider { get; } = new();

    /// <summary>
    ///     Download user's avatar and set to the Avatar property.
    /// </summary>
    public async void DownloadAndSetAvatar()
    {
        var makoClient = App.AppViewModel.MakoClient;
        // get byte array of avatar
        // and set to the bitmap image
        Avatar = (await makoClient.DownloadSoftwareBitmapSourceAsync(makoClient.Session.AvatarUrl!)).UnwrapOrThrow();
    }

    public async Task ReverseSearchAsync(Stream stream)
    {
        try
        {
            var result = await App.AppViewModel.MakoClient.ReverseSearchAsync(stream, App.AppViewModel.AppSetting.ReverseSearchApiKey!);
            if (result.Header.Status is 0)
            {
                var viewModels = await Task.WhenAll(result.Results
                    .Where(r => r.Header.IndexId is 5 or 6 && r.Header.Similarity >
                        App.AppViewModel.AppSetting.ReverseSearchResultSimilarityThreshold)
                    .Select(async r =>
                        new IllustrationItemViewModel(
                            await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(r.Data.PixivId))));

                if (viewModels.Length is 0)
                    _ = _owner.CreateAcknowledgement(MainPageResources.ReverseSearchNotFoundTitle,
                            MainPageResources.ReverseSearchNotFoundContent)
                        .ShowAsync();
                else
                    viewModels[0].CreateWindowWithPage(viewModels);
            }
            else
            {
                _ = await _owner.CreateAcknowledgement(MainPageResources.ReverseSearchErrorTitle,
                        result.Header.Status > 0
                            ? MainPageResources.ReverseSearchServerSideErrorContent
                            : MainPageResources.ReverseSearchClientSideErrorContent)
                    .ShowAsync();
            }
        }
        catch (Exception e)
        {
            _ = await _owner.CreateAcknowledgement(MiscResources.ExceptionEncountered,
                    e.ToString())
                .ShowAsync();
        }
    }
}
