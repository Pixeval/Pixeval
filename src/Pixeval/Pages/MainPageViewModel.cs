#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MainPageViewModel.cs
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Net;
using Pixeval.Database.Managers;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages
{
    public class MainPageViewModel : AutoActivateObservableRecipient, IRecipient<LoginCompletedMessage>
    {
        private ImageSource? _avatar;

        public double MainPageRootNavigationViewOpenPanelLength => 250;

        public readonly NavigationViewTag BookmarksTag = new(typeof(BookmarksPage), App.AppViewModel.MakoClient.Bookmarks(App.AppViewModel.PixivUid!, PrivacyPolicy.Public, App.AppViewModel.AppSetting.TargetFilter));

        public readonly NavigationViewTag HistoriesTag = new(typeof(BrowsingHistoryPage), null);

        public readonly NavigationViewTag RankingsTag = new(typeof(RankingsPage), App.AppViewModel.MakoClient.Ranking(RankOption.Day, DateTime.Today - TimeSpan.FromDays(2)));

        public readonly NavigationViewTag RecentPostsTag = new(typeof(RecentPostsPage), App.AppViewModel.MakoClient.RecentPosts(PrivacyPolicy.Public));

        public readonly NavigationViewTag RecommendsTag = new(typeof(RecommendationPage), App.AppViewModel.MakoClient.Recommendations(targetFilter: App.AppViewModel.AppSetting.TargetFilter));

        public readonly NavigationViewTag SettingsTag = new(typeof(SettingsPage), App.AppViewModel.MakoClient.Configuration);


        public ImageSource? Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }

        public ObservableCollection<SuggestionModel> Suggestions { get; } = new();

        public void Receive(LoginCompletedMessage message)
        {
            DownloadAndSetAvatar();
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

        public async Task AppendSearchHistoryAsync()
        {
            using var scope = App.AppViewModel.AppServicesScope;
            var manager = await scope.ServiceProvider.GetRequiredService<Task<SearchHistoryPersistentManager>>();
            var histories = (await manager.QueryAsync(query =>
            {
                query = query.OrderByDescending(x => x.Time);
                query = query.Take(4);
                return query;
            })).SelectNotNull(SuggestionModel.FromHistory);

            Suggestions.ReplaceByUpdate(histories);
        }
    }
}