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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Pixeval.Persisting;
using Refit;
using Xceed.Wpf.AvalonDock.Controls;

namespace Pixeval
{
    public partial class MainWindow
    {
        public static MainWindow Instance;

        private readonly SnackbarMessageQueue messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2))
        {
            IgnoreDuplicate = true
        };

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            NavigatorList.SelectedItem = MenuTab;
            MainWindowSnackBar.MessageQueue = messageQueue;

            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            UiHelper.SetItemsSource(ToDownloadListView, DownloadList.ToDownloadList);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.InnerException == null || !(e.Exception is HttpRequestException))
            {
                Notice(e.Exception is QueryNotRespondingException ? Externally.QueryNotResponding : e.Exception.Message);

                e.Handled = true;
            }
        }

        private void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            QueryOptionPopup.IsOpen = false;

            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                Notice(Externally.InputIsEmpty);
                return;
            }

            if (QuerySingleArtistToggleButton.IsChecked == true)
                ShowArtist(KeywordTextBox.Text);
            else if (QueryArtistToggleButton.IsChecked == true)
            {
                QueryStartUp();
                TryQueryUser(KeywordTextBox.Text);
            }
            else if (QuerySingleWorkToggleButton.IsChecked == true)
                TryQuerySingle(KeywordTextBox.Text);
            else
            {
                QueryStartUp();
                QueryWorks(KeywordTextBox.Text);
            }
        }

        private async void ShowArtist(string userId)
        {
            if (!int.TryParse(userId, out _))
            {
                Notice(Externally.InputIllegal("单个用户"));
                return;
            }

            try
            {
                await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest {Id = userId});
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    Notice(Externally.CannotFindUser);
                    return;
                }
            }

            UserViewer.Show(userId);
        }

        private async void TryQueryUser(string keyword)
        {
            var iterator = new UserPreviewIterator(keyword);

            var list = UiHelper.NewItemsSource<User>(UserPreviewListView);

            await foreach (var i in iterator.GetUserPreview()) list.Add(i);
        }

        private async void TryQuerySingle(string illustId)
        {
            if (!int.TryParse(illustId, out _))
            {
                Notice(Externally.InputIllegal("单个作品"));
                return;
            }

            try
            {
                IllustViewer.Show(await PixivHelper.IllustrationInfo(illustId));
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound && exception.StatusCode == HttpStatusCode.BadRequest)
                    Notice(Externally.IdDoNotExists);
                else throw;
            }
        }

        private async void QueryWorks(string keyword)
        {
            var pages = await PixivHelper.GetQueryPagesCount(keyword);

            Notice(null, pages);

            var container = new ObservableCollection<Illustration>();
            ImageListView.ItemsSource = container;

            Query(new QueryIterator(KeywordTextBox.Text, pages, Settings.Global.QueryStart), container, true);
        }

        private void IllustrationContainer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IllustViewer.Show(sender.GetDataContext<Illustration>(), ImageListView.ItemsSource as IEnumerable<Illustration>);
        }

        #region 主窗口

        private void KeywordTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            QueryOptionPopup.IsOpen = true;
        }

        private void QuerySingleWorkToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            QueryArtistToggleButton.IsChecked = false;
            QuerySingleArtistToggleButton.IsChecked = false;
        }

        private void QueryArtistToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            HideContentDisplays(UserPreviewListView);
            UiHelper.ReleaseItemsSource(UserPreviewListView);
        }

        private void QueryArtistToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ShowContentDisplays(UserPreviewListView);
            QuerySingleWorkToggleButton.IsChecked = false;
            QuerySingleArtistToggleButton.IsChecked = false;
        }

        private void QuerySingleArtistToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            QuerySingleWorkToggleButton.IsChecked = false;
            QueryArtistToggleButton.IsChecked = false;
        }

        private async void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            await AddUserNameAndAvatar();
        }

        private async Task AddUserNameAndAvatar()
        {
            if (!Identity.Global.AvatarUrl.IsNullOrEmpty() && !Identity.Global.Name.IsNullOrEmpty())
            {
                UserName.Text = Identity.Global.Name;
                UserAvatar.Source = await PixivImage.FromUrl(Identity.Global.AvatarUrl);
            }
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DeactivateControl();
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            DeactivateControl();
        }

        private void DeactivateControl()
        {
            ToLoseFocus.Focus();
            QueryOptionPopup.IsOpen = false;
            DownloadListTab.IsSelected = false;
        }

        #endregion

        #region 导航栏

        private void MenuTab_OnSelected(object sender, RoutedEventArgs e)
        {
            UiHelper.ReleaseItemsSource(ImageListView);
        }

        private void GalleryTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            Notice("正在搜索...");

            Query(new GalleryIterator(Identity.Global.Id), UiHelper.NewItemsSource<Illustration>(ImageListView));
        }

        private void RankingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            Notice("正在搜索...");

            Query(new RankingIterator(), UiHelper.NewItemsSource<Illustration>(ImageListView), true);
        }

        private async void SpotlightTab_OnSelected(object sender, RoutedEventArgs e)
        {
            ShowContentDisplays(SpotlightListView);

            QueryStartUp();

            var lst = UiHelper.NewItemsSource<SpotlightArticle>(SpotlightListView);
            var iterator = new SpotlightQueryIterator(Settings.Global.SpotlightQueryStart, Settings.Global.QueryPages);

            await foreach (var article in iterator.GetSpotlight()) lst.Add(article);
        }

        private void SpotlightTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            HideContentDisplays(SpotlightListView);
            UiHelper.ReleaseItemsSource(SpotlightListView);
        }

        private async void FollowingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            ShowContentDisplays(UserPreviewListView);

            QueryStartUp();
            Notice("正在搜索...");

            var iterator = new UserFollowingIterator(Identity.Global.Id);
            var container = UiHelper.NewItemsSource<User>(UserPreviewListView);

            await foreach (var preview in iterator.GetUserPreviews()) container.Add(preview);
        }

        private void FollowingTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            HideContentDisplays(UserPreviewListView);
            UiHelper.ReleaseItemsSource(UserPreviewListView);
        }

        private void ShowContentDisplays(ListView control)
        {
            if (control == SpotlightListView)
            {
                UiHelper.HideControl(ImageListView);
                UiHelper.HideControl(UserPreviewListView);
            }
            else if (control == UserPreviewListView)
            {
                UiHelper.ShowControl(UserPreviewListView);
                UiHelper.HideControl(ImageListView);
            }
        }

        private void HideContentDisplays(ListView control)
        {
            if (control == SpotlightListView)
            {
                UiHelper.ShowControl(ImageListView);
                UiHelper.ShowControl(UserPreviewListView);
            }
            else if (control == UserPreviewListView)
            {
                UiHelper.ShowControl(ImageListView);
                UiHelper.HideControl(UserPreviewListView);
            }
        }

        private void SignOutTab_OnSelected(object sender, RoutedEventArgs e)
        {
            Identity.Clear();
            var login = new SignIn();
            login.Show();
            Close();
        }

        private void NavigatorList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScheduleHomePage();
        }

        private void ScheduleHomePage()
        {
            QueryOptionPopup.IsOpen = false;
            if (NavigatorList.SelectedItem is ListViewItem current)
            {
                var translateTransform = (TranslateTransform) HomeDisplayContainer.RenderTransform;
                if (current == MenuTab && !translateTransform.Y.Equals(0))
                    HomeContainerMoveUp();
                else if (current != MenuTab && translateTransform.Y.Equals(0)) HomeContainerMoveDown();
            }
        }

        private void HomeContainerMoveDown()
        {
            DoQueryButton.Unable();
            (HomeDisplayContainer.Resources["MoveDownAnimation"] as Storyboard)?.Begin();
        }

        private void HomeContainerMoveUp()
        {
            DoQueryButton.Enable();
            (HomeDisplayContainer.Resources["MoveUpAnimation"] as Storyboard)?.Begin();
        }

        #endregion

        #region 图片预览

        private async void Thumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = sender.GetDataContext<Illustration>();

            if (dataContext != null && Uri.IsWellFormedUriString(dataContext.Thumbnail, UriKind.Absolute)) UiHelper.SetImageSource((Image) sender, await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnail, dataContext.Id));

            UiHelper.StartDoubleAnimationUseCubicEase(sender, "(Image.Opacity)", 0, 1, 500);
        }

        private void Thumbnail_OnUnloaded(object sender, RoutedEventArgs e)
        {
            UiHelper.ReleaseImage((Image) sender);
        }

        private void FavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        private void RemoveFavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        #endregion

        #region 设置

        private void ClearCacheButton_OnClick(object sender, RoutedEventArgs e)
        {
            Directory.Delete(PixevalEnvironment.TempFolder, true);
            Notice("清理完成");
        }

        private void SettingDialog_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            SettingsTab.IsSelected = false;
        }

        private void OpenFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog("选择存储位置")
            {
                InitialDirectory = Settings.Global.DownloadLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok) DownloadLocationTextBox.Text = fileDialog.FileName;
        }

        private void ClearSettingButton_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Global.Clear();
        }

        private void RetractDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadListTab.IsSelected = false;
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>();
            set.AddRange(Settings.Global.ExceptTags);
            set.AddRange(new[] {"R-18", "R-18G"});
            Settings.Global.ExceptTags = set;
        }

        private void QueryR18_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>(Settings.Global.ExceptTags);
            set.Remove("R-18");
            set.Remove("R-18G");
            Settings.Global.ExceptTags = set;
        }

        private void ExceptTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ExceptTagTextBox.Text.Split(" ");
            Settings.Global.ExceptTags = new HashSet<string>(text);
        }

        private void ContainsTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ContainsTagTextBox.Text.Split(" ");
            Settings.Global.ContainsTags = new HashSet<string>(text);
        }

        #endregion

        #region PixivVision

        private async void SpotlightThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = sender.GetDataContext<SpotlightArticle>();
            UiHelper.SetImageSource((Image) sender, await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnail, dataContext.Id.ToString()));
        }

        private void SpotlightThumbnail_OnUnloaded(object sender, RoutedEventArgs e)
        {
            UiHelper.ReleaseImage((Image) sender);
        }

        private async void SpotlightContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Notice("正在搜索...");

            var article = sender.GetDataContext<SpotlightArticle>();

            var tasks = (await PixivClient.Instance.GetArticleWorks(article.Id.ToString())).Select(PixivHelper.IllustrationInfo).Where(i => i != null);
            var result = await Task.WhenAll(tasks);

            IllustViewer.Show(result[0], result);
        }

        private async void DownloadSpotlightItem_OnClick(object sender, RoutedEventArgs e)
        {
            var context = sender.GetDataContext<SpotlightArticle>();

            await PixivImage.DownloadSpotlight(context);
            Notice(Externally.DownloadSpotlightComplete(context));
        }

        #endregion

        #region 用户预览

        private void UserPrevItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserViewer.Show(sender.GetDataContext<User>().Id);
        }

        private async void UserPrevItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var (avatar, thumbnails) = GetUserPrevImageControls(sender);
            var dataContext = sender.GetDataContext<User>();

            UiHelper.SetImageSource(avatar, await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Avatar, dataContext.Name));

            var counter = 0;
            foreach (var thumbnail in thumbnails)
                if (counter < dataContext.Thumbnails.Length)
                    UiHelper.SetImageSource(thumbnail, await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnails[counter], $"{dataContext.Id}", counter++));
        }

        private void UserPrevItem_OnUnloaded(object sender, RoutedEventArgs e)
        {
            var (avatar, thumbnails) = GetUserPrevImageControls(sender);
            UiHelper.ReleaseImage(avatar);
            foreach (var thumbnail in thumbnails) UiHelper.ReleaseImage(thumbnail);
        }

        #endregion

        #region 下载列表

        private void RemoveFromDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.Remove(sender.GetDataContext<Illustration>());
        }

        private void ToDownloadListViewItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IllustViewer.Show(sender.GetDataContext<Illustration>(), DownloadList.ToDownloadList);
        }

        private async void DownloadSingleFromDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();

            DownloadList.Remove(illust);
            await PixivImage.DownloadIllustInternal(illust);
            Notice(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            await PixivImage.DownloadIllustsInternal(DownloadList.ToDownloadList.ToList());
            Notice(Externally.AllDownloadComplete);
        }

        private void ClearDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DownloadList.ToDownloadList.Any())
            {
                DownloadList.ToDownloadList.Clear();
                Notice(Externally.ClearedDownloadList);
            }
        }

        #endregion

        #region 右键菜单

        private async void DownloadNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();

            DownloadList.Remove(illust);
            await PixivImage.DownloadIllustInternal(illust);
            Notice(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.ToDownloadList.Clear();
            await PixivImage.DownloadIllustsInternal((IEnumerable<Illustration>) ImageListView.ItemsSource);
            Notice(Externally.AllDownloadComplete);
        }

        private void AddToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.Add(sender.GetDataContext<Illustration>());
            Notice(Externally.AddedAllToDownloadList);
        }


        private void AddAllToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.AddRange(((IEnumerable<Illustration>) ImageListView.ItemsSource).ToList());
            Notice(Externally.AddedAllToDownloadList);
        }

        #endregion

        #region 工具

        private void Notice(string str, int? pages = null)
        {
            messageQueue.Enqueue(pages != null ? Externally.NoticeProgressString((int) pages) : str);
        }

        private void QueryStartUp()
        {
            MenuTab.IsSelected = false;
            HomeContainerMoveDown();
        }

        private static (Image avatar, Image[] thumbnails) GetUserPrevImageControls(object sender)
        {
            var list = ((Card) sender).FindVisualChildren<Image>().ToArray();

            return (list[0], list.Skip(1).ToArray());
        }

        private static async void Query(IPixivIterator pixivIterator, Collection<Illustration> container, bool useCounter = false)
        {
            var counter = 1;
            while (pixivIterator.HasNext())
            {
                if (useCounter && counter > Settings.Global.QueryPages) break;
                await foreach (var illust in pixivIterator.MoveNextAsync())
                    container.AddIllust(illust);
                counter++;
            }
        }

        #endregion
    }
}