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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using Newtonsoft.Json.Linq;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Persisting;
using Pixeval.UI.UserControls;
using Refit;
using Xceed.Wpf.AvalonDock.Controls;
using static Pixeval.Objects.UiHelper;

#if RELEASE
using System.Net.Http;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Exceptions.Logger;

#endif

namespace Pixeval.UI
{
    public partial class MainWindow
    {
        public static MainWindow Instance;

        public static readonly SnackbarMessageQueue MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2))
        {
            IgnoreDuplicate = true
        };

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            // 默认选中菜单栏
            NavigatorList.SelectedItem = MenuTab;
            MainWindowSnackBar.MessageQueue = MessageQueue;

            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            SetItemsSource(ToDownloadListView, DownloadList.ToDownloadList);

            // 获取推荐用户
            #pragma warning disable 4014
            AcquireRecommendUser();
            #pragma warning restore 4014
        }

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if RELEASE
            switch (e.Exception)
            {
                case QueryNotRespondingException _:
                    MessageQueue.Enqueue(Externally.QueryNotResponding);
                    break;
                case ApiException apiException:
                    if (apiException.StatusCode == HttpStatusCode.BadRequest) MessageQueue.Enqueue(Externally.QueryNotResponding);
                    break;
                case HttpRequestException _:
                    break;
                default:
                    ExceptionLogger.WriteException(e.Exception);
                    break;
            }

            e.Handled = true;
#endif
        }

        private void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseControls(QueryOptionPopup, AutoCompletionPopup);

            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                MessageQueue.Enqueue(Externally.InputIsEmpty);
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
                MessageQueue.Enqueue(Externally.InputIllegal("单个用户"));
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
                    MessageQueue.Enqueue(Externally.CannotFindUser);
                    return;
                }
            }

            OpenUserBrowser();
            SetUserBrowserContext(new User {Id = userId});
        }

        private void TryQueryUser(string keyword)
        {
            QueryStartUp();

            PixivHelper.DoIterate(new UserPreviewIterator(keyword), NewItemsSource<User>(UserPreviewListView));
        }

        private async void TryQuerySingle(string illustId)
        {
            if (!int.TryParse(illustId, out _))
            {
                MessageQueue.Enqueue(Externally.InputIllegal("单个作品"));
                return;
            }

            try
            {
                OpenIllustBrowser(await PixivHelper.IllustrationInfo(illustId));
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound || exception.StatusCode == HttpStatusCode.BadRequest)
                    MessageQueue.Enqueue(Externally.IdDoNotExists);
                else throw;
            }
        }

        private void QueryWorks(string keyword)
        {
            QueryStartUp();
            PixivHelper.DoIterate(new QueryIterator(keyword, Settings.Global.QueryStart), NewItemsSource<Illustration>(ImageListView), true);
        }

        private void IllustrationContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenIllustBrowser(sender.GetDataContext<Illustration>());
            e.Handled = true;
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

        #region 主窗口

        private async void IllustratorIllustsStackPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            var userInfo = sender.GetDataContext<User>();
            var imageCtrl = ((StackPanel) sender).Children.Cast<Image>().ToArray();
            var result = await Tasks<string, BitmapImage>.Of(userInfo.Thumbnails.Take(3))
                .Mapping(PixivEx.FromUrl)
                .Construct()
                .WhenAll();
            for (var i = 0; i < result.Length; i++) imageCtrl[i].Source = result[i];
        }

        private void RecommendIllustratorContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetUserBrowserContext(sender.GetDataContext<User>());
            OpenUserBrowser();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (IllustBrowserDialogHost.IsOpen)
                {
                    IllustBrowserDialogHost.CurrentSession.Close();
                    return;
                }

                if (PixevalSettingPage.SettingDialog.IsOpen) PixevalSettingPage.SettingDialog.CurrentSession.Close();
            }
        }

        private void NavigatorScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scroll(NavigatorScrollViewer, e);
        }

        private async void ReloadRecommendIllustratorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = (TextBlock) sender;
            tb.Disable();
            await AcquireRecommendUser();
            tb.Enable();
        }

        private async void RecommendIllustratorAvatar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = sender.GetDataContext<User>();
            SetImageSource(sender, await PixivEx.FromUrl(context.Avatar));
        }

        private void KeywordTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            QueryOptionPopup.OpenControl();
        }

        private async void KeywordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (QueryArtistToggleButton.IsChecked == true || QuerySingleArtistToggleButton.IsChecked == true || QuerySingleWorkToggleButton.IsChecked == true)
                return;

            var word = KeywordTextBox.Text;

            try
            {
                var result = await HttpClientFactory.AppApiService.GetAutoCompletion(new AutoCompletionRequest {Word = word});
                if (result.Tags.Any())
                {
                    AutoCompletionPopup.OpenControl();
                    AutoCompletionListBox.ItemsSource = result.Tags.Select(p => new AutoCompletion {Tag = p.Name, TranslatedName = p.TranslatedName});
                }
            }
            catch (ApiException)
            {
                AutoCompletionPopup.CloseControl();
            }
        }

        private void KeywordTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            if (key == Key.Enter)
                if (AutoCompletionListBox.SelectedIndex != -1)
                    KeywordTextBox.Text = ((AutoCompletion) AutoCompletionListBox.SelectedItem).Tag;

            AutoCompletionListBox.SelectedIndex = key switch
            {
                var x when x == Key.Down || x == Key.S => (AutoCompletionListBox.SelectedIndex == -1 ? 0 : AutoCompletionListBox.SelectedIndex + 1),
                var x when x == Key.Up || x == Key.A   => (AutoCompletionListBox.SelectedIndex != -1 && AutoCompletionListBox.SelectedIndex != 0 ? AutoCompletionListBox.SelectedIndex - 1 : AutoCompletionListBox.SelectedIndex),
                _                                      => AutoCompletionListBox.SelectedIndex
            };
        }

        private void AutoCompletionElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            KeywordTextBox.Text = sender.GetDataContext<AutoCompletion>().Tag;
        }

        private void QuerySingleWorkToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            QueryArtistToggleButton.IsChecked = false;
            QuerySingleArtistToggleButton.IsChecked = false;
        }

        private void QueryArtistToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ReleaseItemsSource(UserPreviewListView);
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
            CloseControls(QueryOptionPopup, AutoCompletionPopup);
            DownloadListTab.IsSelected = false;
        }

        #endregion

        #region 导航栏

        private void SettingsTab_OnSelected(object sender, RoutedEventArgs e)
        {
            PixevalSettingPage.Open();
        }

        private void UpdateIllustTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            MessageQueue.Enqueue("正在获取关注用户的最新作品...");

            PixivHelper.DoIterate(new UserUpdateIterator(), ImageListViewNewItemSource());
        }

        private void GalleryTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            MessageQueue.Enqueue("正在获取收藏夹...");

            PixivHelper.DoIterate(new GalleryIterator(Identity.Global.Id), ImageListViewNewItemSource());
        }

        private void RankingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            MessageQueue.Enqueue("正在获取每日推荐的作品...");

            PixivHelper.DoIterate(new RankingIterator(), ImageListViewNewItemSource(), true);
        }

        private void SpotlightTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();

            var iterator = new SpotlightQueryIterator(Settings.Global.SpotlightQueryStart, Settings.Global.QueryPages);
            PixivHelper.DoIterate(iterator, NewItemsSource<SpotlightArticle>(SpotlightListView), true);
        }

        private void FollowingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            MessageQueue.Enqueue("正在获取关注列表...");

            PixivHelper.DoIterate(new UserFollowingIterator(Identity.Global.Id), NewItemsSource<User>(UserPreviewListView));
        }

        private void FollowingTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            ReleaseItemsSource(UserPreviewListView);
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
            QueryOptionPopup.CloseControl();
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
            DoQueryButton.Disable();
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
                SetImageSource(sender, await PixivEx.FromUrl(dataContext.Thumbnail));

            StartDoubleAnimationUseCubicEase(sender, "(Image.Opacity)", 0, 1, 500);
        }

        private void FavorButton_OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>());
            e.Handled = true;
        }

        private void DisfavorButton_OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
            e.Handled = true;
        }

        #endregion

        #region Spotlight

        private async void SpotlightThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = sender.GetDataContext<SpotlightArticle>();
            var cover = PixivEx.GetSpotlightCover(dataContext);

            SetImageSource((Image) sender, await PixivEx.FromUrl(cover));
        }

        private async void SpotlightContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageQueue.Enqueue("正在搜索Pixivision...");

            var article = sender.GetDataContext<SpotlightArticle>();

            var tasks = await Tasks<string, Illustration>.Of(await PixivClient.Instance.GetArticleWorks(article.Id.ToString()))
                .Mapping(PixivHelper.IllustrationInfo)
                .Construct()
                .WhenAll();
            var result = tasks.Peek(i => i.IsManga = true).ToArray();

            OpenIllustBrowser(result[0].Apply(r => r.MangaMetadata = result.ToArray()));
        }

        private async void DownloadSpotlightItem_OnClick(object sender, RoutedEventArgs e)
        {
            var context = sender.GetDataContext<SpotlightArticle>();

            await PixivEx.DownloadSpotlight(context);
            MessageQueue.Enqueue(Externally.DownloadSpotlightComplete(context));
        }

        #endregion

        #region 下载列表

        private void RemoveFromDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.Remove(sender.GetDataContext<Illustration>());
        }

        private void ToDownloadListViewItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenIllustBrowser(sender.GetDataContext<Illustration>());
        }

        private async void DownloadSingleFromDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();

            DownloadList.Remove(illust);
            await PixivEx.DownloadIllustInternal(illust);
            MessageQueue.Enqueue(Externally.DownloadComplete(illust));
        }

        private void DownloadAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivEx.DownloadIllustsInternal(DownloadList.ToDownloadList.ToList());
            DownloadList.ToDownloadList.Clear();
            MessageQueue.Enqueue(Externally.AllDownloadComplete);
        }

        private void ClearDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DownloadList.ToDownloadList.Any())
            {
                DownloadList.ToDownloadList.Clear();
                MessageQueue.Enqueue(Externally.ClearedDownloadList);
            }
        }

        #endregion

        #region 右键菜单

        private async void DownloadNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();

            DownloadList.Remove(illust);
            await PixivEx.DownloadIllustInternal(illust);
            MessageQueue.Enqueue(Externally.DownloadComplete(illust));
        }

        private void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            PixivEx.DownloadIllustsInternal(GetImageSourceCopy());
            MessageQueue.Enqueue(Externally.AllDownloadComplete);
        }

        private void AddToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.Add(sender.GetDataContext<Illustration>());
            MessageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }


        private void AddAllToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.AddRange(GetImageSourceCopy());
            MessageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }

        #endregion

        #region 用户预览

        private void UserIllustsImageListView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scroll(UserBrowserPageScrollViewer, e);
        }

        private void UserPrevItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var usrDateContext = sender.GetDataContext<User>();

            SetUserBrowserContext(usrDateContext);
        }

        private void SetupUserUploads(string id)
        {
            PixivHelper.DoIterate(new UploadIterator(id), NewItemsSource<Illustration>(UserIllustsImageListView));
        }

        private void SetupUserGallery(string id)
        {
            PixivHelper.DoIterate(new GalleryIterator(id), NewItemsSource<Illustration>(UserIllustsImageListView));
        }

        private void SetUserBanner(string id)
        {
            Task.Run(async () =>
            {
                var link = $"https://public-api.secure.pixiv.net/v1/users/{id}/works.json?page=1&publicity=public&per_page=1&image_sizes=large";
                var httpClient = HttpClientFactory.PixivApi(ProtocolBase.PublicApiBaseUrl);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer");

                var res = (await httpClient.GetStringAsync(link)).FromJson<dynamic>();
                if (((IEnumerable<JToken>) res.response).Any())
                {
                    var img = res.response[0].image_urls.large.ToString();
                    SetImageSource(UserBanner, await PixivEx.FromUrl(img));
                }
            });
        }

        private async void UserPrevItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var (avatar, thumbnails) = GetUserPrevImageControls(sender);
            var dataContext = sender.GetDataContext<User>();

            SetImageSource(avatar, await PixivEx.FromUrl(dataContext.Avatar));

            var counter = 0;
            foreach (var thumbnail in thumbnails)
                if (counter < dataContext.Thumbnails.Length)
                    SetImageSource(thumbnail, await PixivEx.FromUrl(dataContext.Thumbnails[counter++]));
        }

        private void RetractDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadListTab.IsSelected = false;
        }

        private void UploadChecker_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsAtUploadCheckerPosition())
            {
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerIncreaseWidthAnimation").Apply(s => s.Completed += (o, args) =>
                {
                    CheckerSnackBar.HorizontalAlignment = HorizontalAlignment.Left;
                    CheckerSnackBarOpacityMask.HorizontalAlignment = HorizontalAlignment.Left;
                    UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerDecreaseWidthAnimation").Begin();
                    UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskDecreaseWidthAnimation").Begin();
                }).Begin();
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskIncreaseWidthAnimation").Begin();
            }

            SetupUserUploads(sender.GetDataContext<User>().Id);
        }

        private void GalleryChecker_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsAtUploadCheckerPosition())
            {
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerIncreaseWidthAnimation").Apply(s => s.Completed += (o, args) =>
                {
                    CheckerSnackBar.HorizontalAlignment = HorizontalAlignment.Right;
                    CheckerSnackBarOpacityMask.HorizontalAlignment = HorizontalAlignment.Right;
                    UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerDecreaseWidthAnimation").Begin();
                    UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskDecreaseWidthAnimation").Begin();
                }).Begin();
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskIncreaseWidthAnimation").Begin();
            }

            SetupUserGallery(sender.GetDataContext<User>().Id);
        }

        private bool IsAtUploadCheckerPosition()
        {
            return CheckerSnackBar.HorizontalAlignment == HorizontalAlignment.Left && CheckerSnackBar.Width.Equals(90);
        }

        private void BackToMainPageButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserBrowserPageScrollViewer.DataContext = null;
            ReleaseImage(UserBanner);
            ReleaseImage(UserBrowserUserAvatar);
            ReleaseItemsSource(UserIllustsImageListView);
        }

        private async void FollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.FollowArtist(usr);
        }

        private async void UnFollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.UnFollowArtist(usr);
        }

        private void ShareUserButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText($"https://www.pixiv.net/member.php?id={sender.GetDataContext<User>().Id}");
            MessageQueue.Enqueue("链接已复制到剪切板");
        }


        private void ViewUserInBrowserButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.pixiv.net/artworks/{sender.GetDataContext<User>().Id}") {CreateNoWindow = true});
        }

        #endregion

        #region 作品浏览器

        private void IllustrationContainer_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void IllustBrowserDialogHost_OnDialogOpened(object sender, DialogOpenedEventArgs e)
        {
            var context = sender.GetDataContext<Illustration>();

            var list = new ObservableCollection<TransitionerSlide>();

            var template = new IllustTransitioner(list);
            IllustBrowserContainer.Children.Insert(1, template);

            if (context.IsManga)
            {
                if (context.MangaMetadata.IsNullOrEmpty()) context = await PixivHelper.IllustrationInfo(context.Id);

                var tasks = await Tasks<Illustration, (BitmapImage image, Illustration illust)>.Of(context.MangaMetadata)
                    .Mapping(illustration => Task.Run(async () => (await PixivEx.FromUrl(illustration.Large), illustration)))
                    .Construct()
                    .WhenAll();

                list.AddRange(tasks.Select(i => InitTransitionerSlide(i.image, i.illust)));
            }
            else
            {
                list.Add(InitTransitionerSlide(await PixivEx.FromUrl(context.Large), context));
            }
        }

        private static TransitionerSlide InitTransitionerSlide(ImageSource imgSource, Illustration illust)
        {
            return new TransitionerSlide
            {
                ForwardWipe = new CircleWipe(),
                BackwardWipe = new CircleWipe(),
                Content = new IllustPresenter(imgSource, illust)
            };
        }

        private void IllustBrowserDialogHost_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            IllustBrowserContainer.Children.RemoveAt(1);
            ReleaseImage(IllustBrowserUserAvatar);
            IllustBrowserDialogHost.DataContext = null;
        }

        private async void TagNavigateHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var txt = ((Tag) ((Hyperlink) sender).DataContext).Name;

            if (!UserBrowserPageScrollViewer.Opacity.Equals(0))
            {
                BackToMainPageButton.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent,
                    Source = this
                });
            }

            IllustBrowserDialogHost.CurrentSession.Close();
            Instance.NavigatorList.SelectedItem = Instance.MenuTab;

            await Task.Delay(300);
            Instance.KeywordTextBox.Text = txt;
        }

        private void ShareButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText($"https://www.pixiv.net/artworks/{sender.GetDataContext<Illustration>().Id}");
            MessageQueue.Enqueue("作品链接已经复制到剪贴板");
        }

        private void AddToDownloadListButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DownloadList.Add(sender.GetDataContext<Illustration>());
            MessageQueue.Enqueue(Externally.AddedToDownloadList);
        }

        private void AddAllToDownloadListButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SpotlightTab.IsSelected)
                DownloadList.Add(sender.GetDataContext<Illustration>());
            else if (GetImageSourceCopy() is { } illusts) DownloadList.AddRange(illusts);
            MessageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }

        private void ImageBrowserUserAvatar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var usr = new User {Id = sender.GetDataContext<Illustration>().UserId};
            IllustBrowserDialogHost.CurrentSession.Close();
            SetUserBrowserContext(usr);
        }

        private void ViewInBrowserButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.pixiv.net/artworks/{sender.GetDataContext<Illustration>().Id}") {CreateNoWindow = true});
        }

        private async void DownloadButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var model = sender.GetDataContext<Illustration>();

            DownloadList.Remove(model);
            await PixivEx.DownloadIllustInternal(model);
            MessageQueue.Enqueue(Externally.DownloadComplete(model));
        }

        private void IllustBrowserFavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        private void IllustBrowserDisfavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.Instance.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        #endregion

        #region 工具

        private async Task AcquireRecommendUser()
        {
            var list = NewItemsSource<User>(RecommendIllustratorListBox);
            list.AddRange(await RecommendIllustratorDeferrer.Instance.Acquire(6));
        }

        private IEnumerable<Illustration> GetImageSourceCopy()
        {
            var lst = (IEnumerable<Illustration>) (BrowsingUser() ? UserIllustsImageListView : ImageListView)?.ItemsSource;
            return lst != null ? lst.ToList() : new List<Illustration>();
        }

        private void QueryStartUp()
        {
            MenuTab.IsSelected = false;
            HomeContainerMoveDown();
        }

        private bool BrowsingUser()
        {
            return !UserBrowserPageScrollViewer.Opacity.Equals(0D);
        }

        private static (Image avatar, Image[] thumbnails) GetUserPrevImageControls(object sender)
        {
            var list = ((Card) sender).FindVisualChildren<Image>().ToArray();

            return (list.First(p => p.Name == "UserAvatar"), list.Where(p => p.Name != "UserAvatar").ToArray());
        }

        private ObservableCollection<Illustration> ImageListViewNewItemSource()
        {
            return NewItemsSource<Illustration>(ImageListView);
        }

        private async void SetUserBrowserContext(User user)
        {
            var usr = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest {Id = $"{user.Id}"});
            var usrEntity = new User
            {
                Avatar = usr.UserEntity.ProfileImageUrls.Medium,
                Background = usr.UserEntity.ProfileImageUrls.Medium,
                Follows = (int) usr.UserProfile.TotalFollowUsers,
                Id = usr.UserEntity.Id.ToString(),
                Introduction = usr.UserEntity.Comment,
                IsFollowed = usr.UserEntity.IsFollowed,
                IsPremium = usr.UserProfile.IsPremium,
                Name = usr.UserEntity.Name,
                Thumbnails = user.Thumbnails
            };
            UserBrowserPageScrollViewer.DataContext = usrEntity;

            SetUserBanner(usrEntity.Id);
            SetUserBanner(usrEntity.Id);
            SetImageSource(UserBrowserUserAvatar, await PixivEx.FromUrl(usrEntity.Avatar));

            SetupUserUploads(usrEntity.Id);
        }

        private void OpenUserBrowser()
        {
            this.GetResources<Storyboard>("UserBrowserOpacityIncreaseAnimation").Begin();
            this.GetResources<Storyboard>("UserBrowserScaleXIncreaseAnimation").Begin();
            this.GetResources<Storyboard>("UserBrowserScaleYIncreaseAnimation").Begin();
            this.GetResources<Storyboard>("ContentContainerOpacityIncreaseAnimation").Begin();
            this.GetResources<Storyboard>("ContentContainerScaleXIncreaseAnimation").Begin();
            this.GetResources<Storyboard>("ContentContainerScaleYIncreaseAnimation").Begin();
        }

        private async void OpenIllustBrowser(Illustration illustration)
        {
            IllustBrowserDialogHost.DataContext = illustration;

            var userInfo = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest { Id = illustration.UserId });
            var avatar = await PixivEx.FromUrl(userInfo.UserEntity.ProfileImageUrls.Medium);
            if (avatar != null)
            {
                SetImageSource(IllustBrowserUserAvatar, avatar);
            }

            IllustBrowserDialogHost.OpenControl();
        }

        #endregion
    }
}