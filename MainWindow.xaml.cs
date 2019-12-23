// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
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
using Pixeval.Objects.Exceptions.Log;
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
            switch (e.Exception)
            {
                case QueryNotRespondingException _:
                    Notice(Externally.QueryNotResponding);
                    break;
                case ApiException apiException:
                    if (apiException.StatusCode == HttpStatusCode.BadRequest) Notice(Externally.QueryNotResponding);
                    break;
                case HttpRequestException _:
                    break;
                default:
                    ExceptionLogger.WriteException(e.Exception);
                    Notice(e.Exception.Message);
                    break;
            }

            e.Handled = true;
        }

        private void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            QueryOptionPopup.IsOpen = false;

            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                Notice(Externally.InputIsEmpty);
                return;
            }

            var keyword = KeywordTextBox.Text;
            if (QuerySingleArtistToggleButton.IsChecked == true)
                ShowArtist(keyword);
            else if (QueryArtistToggleButton.IsChecked == true)
                TryQueryUser(keyword);
            else if (QuerySingleWorkToggleButton.IsChecked == true)
                TryQuerySingle(keyword);
            else
                QueryWorks(keyword);
        }

        private async void ShowArtist(string userId)
        {
            if (!userId.IsNumber())
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

        private void TryQueryUser(string keyword)
        {
            QueryStartUp();

            PixivHelper.DoIterate(new UserPreviewIterator(keyword), UiHelper.NewItemsSource<User>(UserPreviewListView));
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
                if (exception.StatusCode == HttpStatusCode.NotFound || exception.StatusCode == HttpStatusCode.BadRequest)
                    Notice(Externally.IdDoNotExists);
                else throw;
            }
        }

        private void QueryWorks(string keyword)
        {
            QueryStartUp();
            PixivHelper.DoIterate(new QueryIterator(keyword, Settings.Global.QueryStart), UiHelper.NewItemsSource<Illustration>(ImageListView), true);
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

        private async void KeywordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var word = KeywordTextBox.Text;

            try
            {
                var result = await HttpClientFactory.AppApiService.GetAutoCompletion(new AutoCompletionRequest { Word = word });
                if (result.Tags.Any())
                {
                    AutoCompletionPopup.IsOpen = true;
                    AutoCompletionListBox.ItemsSource = result.Tags.Select(p => new AutoCompletion { Tag = p.Name, TranslatedName = p.TranslatedName });
                }
            }
            catch (ApiException)
            {
                AutoCompletionPopup.IsOpen = false;
            }
        }

        private void KeywordTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            if (key == Key.Enter)
            {
                if (AutoCompletionListBox.SelectedIndex != -1)
                    KeywordTextBox.Text = ((AutoCompletion) AutoCompletionListBox.SelectedItem).Tag;
            }

            AutoCompletionListBox.SelectedIndex = key switch
            {
                var x when x == Key.Down || x == Key.S => AutoCompletionListBox.SelectedIndex == -1 ? 0 : AutoCompletionListBox.SelectedIndex + 1,
                var x when x == Key.Up || x == Key.A   => AutoCompletionListBox.SelectedIndex != -1 && AutoCompletionListBox.SelectedIndex != 0 ? AutoCompletionListBox.SelectedIndex - 1 : AutoCompletionListBox.SelectedIndex,
                _                                      => AutoCompletionListBox.SelectedIndex
            };
        }

        private void QuerySingleWorkToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            QueryArtistToggleButton.IsChecked = false;
            QuerySingleArtistToggleButton.IsChecked = false;
        }

        private void QueryArtistToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            UiHelper.ReleaseItemsSource(UserPreviewListView);
        }

        private void QueryArtistToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
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
                UserAvatar.Source = await PixivEx.FromUrl(Identity.Global.AvatarUrl);
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
            AutoCompletionPopup.IsOpen = false;
            DownloadListTab.IsSelected = false;
        }

        #endregion

        #region 导航栏

        private void UpdateIllustTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            Notice("正在获取关注用户的最新作品...");

            PixivHelper.DoIterate(new UserUpdateIterator(), ImageListViewNewItemSource());
        }

        private void GalleryTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            Notice("正在获取收藏夹...");

            PixivHelper.DoIterate(new GalleryIterator(Identity.Global.Id), ImageListViewNewItemSource());
        }

        private void RankingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            Notice("正在获取每日推荐的作品...");

            PixivHelper.DoIterate(new RankingIterator(), ImageListViewNewItemSource(), true);
        }

        private void SpotlightTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();

            var iterator = new SpotlightQueryIterator(Settings.Global.SpotlightQueryStart, Settings.Global.QueryPages);
            PixivHelper.DoIterate(iterator, UiHelper.NewItemsSource<SpotlightArticle>(SpotlightListView), true);
        }

        private void FollowingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            Notice("正在获取关注列表...");

            PixivHelper.DoIterate(new UserFollowingIterator(Identity.Global.Id), UiHelper.NewItemsSource<User>(UserPreviewListView));
        }

        private void FollowingTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            UiHelper.ReleaseItemsSource(UserPreviewListView);
        }

        private void SignOutTab_OnSelected(object sender, RoutedEventArgs e)
        {
            Identity.Clear();
            Settings.Global.Initialize();
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
            HomeDisplayContainer.GetResources<Storyboard>("MoveDownAnimation").Begin();
        }

        private void HomeContainerMoveUp()
        {
            DoQueryButton.Enable();
            HomeDisplayContainer.GetResources<Storyboard>("MoveUpAnimation")?.Begin();
        }

        #endregion

        #region 图片预览

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
            Settings.Global.Initialize();
        }

        private void RetractDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadListTab.IsSelected = false;
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>();
            if (Settings.Global.ExceptTags != null) set.AddRange(Settings.Global.ExceptTags);
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
            var cover = PixivEx.GetSpotlightCover(dataContext);

            UiHelper.SetImageSource((Image) sender, await PixivEx.GetAndCreateOrLoadFromCacheInternal(cover, dataContext.Id.ToString()));
        }

        private async void SpotlightContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Notice("正在搜索Pixivision...");

            var article = sender.GetDataContext<SpotlightArticle>();

            var tasks = (await PixivClient.Instance.GetArticleWorks(article.Id.ToString())).Select(PixivHelper.IllustrationInfo).Where(i => i != null);
            var result = await Task.WhenAll(tasks);

            IllustViewer.Show(result[0], result);
        }

        private async void DownloadSpotlightItem_OnClick(object sender, RoutedEventArgs e)
        {
            var context = sender.GetDataContext<SpotlightArticle>();

            await PixivEx.DownloadSpotlight(context);
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

            UiHelper.SetImageSource(avatar, await PixivEx.GetAndCreateOrLoadFromCacheInternal(dataContext.Avatar, dataContext.Name));

            var counter = 0;
            foreach (var thumbnail in thumbnails)
                if (counter < dataContext.Thumbnails.Length)
                    UiHelper.SetImageSource(thumbnail, await PixivEx.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnails[counter], $"{dataContext.Id}", counter++));
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
            await PixivEx.DownloadIllustInternal(illust);
            Notice(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            await PixivEx.DownloadIllustsInternal(DownloadList.ToDownloadList.ToList());
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
            await PixivEx.DownloadIllustInternal(illust);
            Notice(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.ToDownloadList.Clear();
            await PixivEx.DownloadIllustsInternal((IEnumerable<Illustration>) ImageListView.ItemsSource);
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

            return (list.First(p => p.Name == "UserAvatar"), list.Where(p => p.Name != "UserAvatar").ToArray());
        }

        private ObservableCollection<Illustration> ImageListViewNewItemSource()
        {
            return UiHelper.NewItemsSource<Illustration>(ImageListView);
        }

        #endregion
    }
}