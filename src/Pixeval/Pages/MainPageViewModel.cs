#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainPageViewModel.cs
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
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Net;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Pages.Capability;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.Misc;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.Pages;

public partial class MainPageViewModel : AutoActivateObservableRecipient, IRecipient<LoginCompletedMessage>
{
    public readonly NavigationViewTag AboutTag = new(typeof(AboutPage), null);

    public readonly NavigationViewTag BookmarksTag = new(typeof(BookmarksPage), App.AppViewModel.MakoClient.Bookmarks(App.AppViewModel.PixivUid!, PrivacyPolicy.Public, App.AppViewModel.AppSetting.TargetFilter));

    public readonly NavigationViewTag FollowingsTag = new(typeof(FollowingsPage), null);

    public readonly NavigationViewTag HistoriesTag = new(typeof(BrowsingHistoryPage), null);

    public readonly NavigationViewTag RankingsTag = new(typeof(RankingsPage), App.AppViewModel.MakoClient.Ranking(RankOption.Day, DateTime.Today - TimeSpan.FromDays(2)));

    public readonly NavigationViewTag RecentPostsTag = new(typeof(RecentPostsPage), App.AppViewModel.MakoClient.RecentPosts(PrivacyPolicy.Public));

    public readonly NavigationViewTag RecommendsTag = new(typeof(RecommendationPage), App.AppViewModel.MakoClient.Recommendations(targetFilter: App.AppViewModel.AppSetting.TargetFilter));

    public readonly NavigationViewTag SettingsTag = new(typeof(SettingsPage), App.AppViewModel.MakoClient.Configuration);

    public readonly NavigationViewTag SpotlightsTag = new(typeof(SpotlightsPage), null);

    [ObservableProperty]
    private ImageSource? _avatar;

    public double MainPageRootNavigationViewOpenPanelLength => 280;

    public SuggestionStateMachine SuggestionProvider { get; } = new();

    public void Receive(LoginCompletedMessage message)
    {
        // TODO
        // DownloadAndSetAvatar();
    }

    /// <summary>
    ///     Download user's avatar and set to the Avatar property.
    /// </summary>
    public async void DownloadAndSetAvatar()
    {
        var makoClient = App.AppViewModel.MakoClient;
        // get byte array of avatar
        // and set to the bitmap image
        Avatar = await (await makoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(makoClient.Session.AvatarUrl!))
            .GetOrThrow()
            .GetBitmapImageAsync(true);
    }

    public async Task ReverseSearchAsync(Stream stream)
    {
        try
        {
            App.AppViewModel.Window.ShowProgressRing();
            var result = await App.AppViewModel.MakoClient.ReverseSearchAsync(stream, App.AppViewModel.AppSetting.ReverseSearchApiKey!);
            if (result.Header is not null)
            {
                switch (result.Header!.Status)
                {
                    case 0:
                        if (result.Results?.FirstOrDefault() is { Header.IndexId: 5 or 6 } first)
                        {
                            var viewModels = new IllustrationViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(first.Data!.PixivId.ToString()))
                                .GetMangaIllustrationViewModels()
                                .ToArray();
                            App.AppViewModel.Window.HideProgressRing();
                            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", App.AppViewModel.AppWindowRootFrame);
                            App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
                            return;
                        }

                        break;
                    case var s:
                        await MessageDialogBuilder.CreateAcknowledgement(
                                App.AppViewModel.Window,
                                MainPageResources.ReverseSearchErrorTitle,
                                s > 0 ? MainPageResources.ReverseSearchServerSideErrorContent : MainPageResources.ReverseSearchClientSideErrorContent)
                            .ShowAsync();
                        break;
                }

                App.AppViewModel.Window.HideProgressRing();
                MessageDialogBuilder.CreateAcknowledgement(App.AppViewModel.Window, MainPageResources.ReverseSearchNotFoundTitle, MainPageResources.ReverseSearchNotFoundContent);
            }
        }
        catch (Exception e)
        {
            App.AppViewModel.Window.HideProgressRing();
            await App.AppViewModel.ShowExceptionDialogAsync(e);
        }
    }
}