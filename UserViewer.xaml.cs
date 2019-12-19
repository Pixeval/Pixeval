// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Exceptions.Log;
using Refit;

namespace Pixeval
{
    public partial class UserViewer
    {
        private readonly SnackbarMessageQueue messageQueue = new SnackbarMessageQueue();
        private readonly User user;

        private bool atUploadSelector;

        public UserViewer(User usr)
        {
            user = usr;
            DataContext = user;

            InitializeComponent();
            if (Dispatcher != null) Dispatcher.UnhandledException += DispatcherOnUnhandledException;

            UserViewerSnackBar.MessageQueue = messageQueue;
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            switch (e.Exception)
            {
                case QueryNotRespondingException _:
                    messageQueue.Enqueue(Externally.QueryNotResponding);
                    break;
                case ApiException apiException:
                    if (apiException.StatusCode == HttpStatusCode.BadRequest) messageQueue.Enqueue(Externally.QueryNotResponding);
                    break;
                case HttpRequestException _:
                    break;
                default:
                    ExceptionLogger.WriteException(e.Exception);
                    messageQueue.Enqueue(e.Exception.Message);
                    break;
            }

            e.Handled = true;
        }

        public static async void Show(string id)
        {
            var info = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest {Id = id});
            var v = new UserViewer(new User
            {
                Avatar = info.UserEntity.ProfileImageUrls.Medium,
                Id = info.UserEntity.Id.ToString(),
                Introduction = info.UserEntity.Comment,
                IsFollowed = info.UserEntity.IsFollowed,
                Name = info.UserEntity.Name
            });
            v.Show();
        }

        private void SetupUploads()
        {
            atUploadSelector = true;

            PixivHelper.DoIterate(new UploadIterator(user.Id), UiHelper.NewItemsSource<Illustration>(ImageListView));
        }

        private void SetupFavorite()
        {
            atUploadSelector = false;

            PixivHelper.DoIterate(new GalleryIterator(user.Id), UiHelper.NewItemsSource<Illustration>(ImageListView));
        }

        private void ShowcaseContainer_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ShowcaseTranslateTransform.Y.Equals(0)) ShowcaseTranslateTransform.SetCurrentValue(TranslateTransform.YProperty, -e.NewSize.Height);
        }

        private void UserViewer_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupBackgroundImage();
        }

        private async void SetupBackgroundImage()
        {
            var link = $"https://public-api.secure.pixiv.net/v1/users/{user.Id}/works.json?page=1&publicity=public&per_page=1&image_sizes=large";
            var httpClient = HttpClientFactory.PixivApi(ProtocolBase.PublicApiBaseUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer");

            var res = (await httpClient.GetStringAsync(link)).FromJson<dynamic>();
            if (((IEnumerable<JToken>) res.response).Any())
            {
                var img = res.response[0].image_urls.large.ToString();
                UiHelper.SetImageSource(BackgroundImage, await PixivEx.FromUrl(img));
            }
        }

        private void UploadSelector_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.GetResources<Storyboard>("SelectorSnackBarMoveLeftAnimation").Begin();
            this.GetResources<Storyboard>("SelectorOpacityMaskMoveLeftAnimation").Begin();

            SetupUploads();
        }

        private void FavoriteSelector_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.GetResources<Storyboard>("SelectorSnackBarMoveRightAnimation").Begin();
            this.GetResources<Storyboard>("SelectorOpacityMaskMoveRightAnimation").Begin();

            SetupFavorite();
        }

        private void IllustrationContainer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IllustViewer.Show(sender.GetDataContext<Illustration>(), ImageListView.ItemsSource as IEnumerable<Illustration>);
        }

        private async void DownloadNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();

            DownloadList.Remove(illust);
            await PixivEx.DownloadIllustInternal(illust);
            messageQueue.Enqueue(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.ToDownloadList.Clear();
            await PixivEx.DownloadIllustsInternal((IEnumerable<Illustration>) ImageListView.ItemsSource, Path.Combine(user.Name, $"{(atUploadSelector ? "作品" : "收藏")}"));
            messageQueue.Enqueue(Externally.AllDownloadComplete);
        }

        private void AddToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.Add(sender.GetDataContext<Illustration>());
            messageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }

        private void AddAllToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.AddRange(((IEnumerable<Illustration>) ImageListView.ItemsSource).ToList());
            messageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }

        private async void Thumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = sender.GetDataContext<Illustration>();

            if (dataContext != null && Uri.IsWellFormedUriString(dataContext.Thumbnail, UriKind.Absolute))
                UiHelper.SetImageSource(sender, await PixivEx.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnail, dataContext.Id));

            UiHelper.StartDoubleAnimationUseCubicEase(sender, "(Image.Opacity)", 0, 1, 500);
        }

        private void FavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        private void RemoveFavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        private void ShowcaseContainerToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            UploadSelector.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) {RoutedEvent = MouseLeftButtonDownEvent});
        }

        private async void ShowcaseContainer_OnLoaded(object sender, RoutedEventArgs e)
        {
            UiHelper.SetImageSource(UserAvatar, await PixivEx.FromUrl(this.GetDataContext<User>().Avatar));
        }

        private async void FollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            await PixivClient.Instance.FollowArtist(user);
            messageQueue.Enqueue(Externally.SuccessfullyFollowUser);
        }

        private async void UnFollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            await PixivClient.Instance.UnFollowArtist(user);
            messageQueue.Enqueue(Externally.SuccessfullyUnFollowUser);
        }
    }
}