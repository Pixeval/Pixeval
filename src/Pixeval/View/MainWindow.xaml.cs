#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Wpf.Core;
using Pixeval.Wpf.Data.Web.Delegation;
using Pixeval.Wpf.Data.Web.Request;
using Pixeval.Wpf.Objects.Generic;
using Pixeval.Wpf.Objects.I18n;
using Pixeval.Wpf.Objects.Native;
using Pixeval.Wpf.Objects.Primitive;
using Pixeval.Wpf.Persisting;
using Pixeval.Wpf.UserControls;
using Pixeval.Wpf.ViewModel;
using Refit;
using Xceed.Wpf.AvalonDock.Controls;
using static Pixeval.Wpf.Objects.Primitive.UiHelper;

#if RELEASE
using System.Net.Http;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Exceptions.Logger;

#endif

namespace Pixeval.Wpf.View
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
            //            Instance = this;
            //            NavigatorList.SelectedItem = MenuTab;
            
            //            MainWindowSnackBar.MessageQueue = MessageQueue;

            //            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            //#pragma warning disable 4014
            //            AcquireRecommendUser();
            //#pragma warning restore 4014
        }

        protected override void OnInitialized(EventArgs e)
        {
            InitializeComponent();
            base.OnInitialized(e);
        } 

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if RELEASE
            switch (e.Exception)
            {
                case QueryNotRespondingException _:
                    MessageQueue.Enqueue(AkaI18N.QueryNotResponding);
                    break;
                case ApiException apiException:
                    if (apiException.StatusCode == HttpStatusCode.BadRequest)
                        MessageQueue.Enqueue(AkaI18N.QueryNotResponding);
                    break;
                case HttpRequestException _: break;
                default:
                    ExceptionDumper.WriteException(e.Exception);
                    break;
            }

            e.Handled = true;
#endif
        }

        private void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseControls(TrendingTagPopup, AutoCompletionPopup);

            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                MessageQueue.Enqueue(AkaI18N.InputIsEmpty);
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
                MessageQueue.Enqueue(AkaI18N.UserIdIllegal);
                return;
            }

            try
            {
                await HttpClientFactory.AppApiService().GetUserInformation(new UserInformationRequest { Id = userId });
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    MessageQueue.Enqueue(AkaI18N.CannotFindUser);
                    return;
                }
            }

            OpenUserBrowser();
            SetUserBrowserContext(new User { Id = userId });
        }

        private void TryQueryUser(string keyword)
        {
            QueryStartup();
            SearchingHistoryManager.EnqueueSearchHistory(keyword);
            PixivHelper.Enumerate(new UserPreviewAsyncEnumerable(keyword), NewItemsSource<User>(UserPreviewListView));
        }

        private async void TryQuerySingle(string illustId)
        {
            if (!int.TryParse(illustId, out _))
            {
                MessageQueue.Enqueue(AkaI18N.IdIllegal);
                return;
            }

            try
            {
                OpenIllustBrowser(await PixivHelper.IllustrationInfo(illustId));
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound ||
                    exception.StatusCode == HttpStatusCode.BadRequest)
                    MessageQueue.Enqueue(AkaI18N.IdDoNotExists);
                else
                    throw;
            }
        }

        private void QueryWorks(string keyword)
        {
            QueryStartup();
            SearchingHistoryManager.EnqueueSearchHistory(keyword);
            PixivHelper.Enumerate(Settings.Global.SortOnInserting
                                      ? (AbstractQueryAsyncEnumerable)new PopularityQueryAsyncEnumerable(keyword, Settings.Global.TagMatchOption, Session.Current.IsPremium, Settings.Global.QueryStart)
                                      : new PublishDateQueryAsyncEnumerable(keyword, Settings.Global.TagMatchOption, Session.Current.IsPremium, Settings.Global.QueryStart),
                                  NewItemsSource<Illustration>(ImageListView), Settings.Global.QueryPages);
        }

        private void IllustrationContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenIllustBrowser(sender.GetDataContext<Illustration>());
            e.Handled = true;
        }

        private void IllustrationContainer_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private  void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //await AddUserNameAndAvatar();
        }

        private async Task AddUserNameAndAvatar()
        {
            if (!Session.Current.AvatarUrl.IsNullOrEmpty() && !Session.Current.Name.IsNullOrEmpty())
            {
                UserName.Text = Session.Current.Name;
                UserAvatar.Source = await PixivIO.FromUrl(Session.Current.AvatarUrl);
            }
        }

        private void PixevalSettingDialog_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            SettingsTab.IsSelected = false;
        }

        private void DownloadQueueDialogHost_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            DownloadListTab.IsSelected = false;
        }

        #region 主窗口

        private async void UserPreviewPopupContent_OnLoaded(object sender, RoutedEventArgs e)
        {
            var userInfo = sender.GetDataContext<User>();
            var ctrl = (UserPreviewPopupContent)sender;
            var usr = await HttpClientFactory.AppApiService()
                .GetUserInformation(new UserInformationRequest { Id = $"{sender.GetDataContext<User>().Id}" });
            var usrEntity = new User
            {
                Avatar = usr.UserEntity.ProfileImageUrls.Medium,
                Background = usr.UserEntity.ProfileImageUrls.Medium,
                Follows = (int)usr.UserProfile.TotalFollowUsers,
                Id = usr.UserEntity.Id.ToString(),
                Introduction = usr.UserEntity.Comment,
                IsFollowed = usr.UserEntity.IsFollowed,
                IsPremium = usr.UserProfile.IsPremium,
                Name = usr.UserEntity.Name,
                Thumbnails = sender.GetDataContext<User>().Thumbnails
            };
            ctrl.DataContext = usrEntity;
            var result = await Tasks<string, BitmapImage>.Of(userInfo.Thumbnails.Take(3))
                .Mapping(PixivIO.FromUrl)
                .Construct()
                .WhenAll();
            ctrl.SetImages(result[0], result[1], result[2]);
        }

        private void RecommendIllustratorContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetUserBrowserContext(sender.GetDataContext<User>());
        }

        private async void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.Key)
            {
                case Key.Oem5:
                    var inputBoxControl = BrowsingUser() ? UserBrowserConditionInputBox : ConditionInputBox;
                    inputBoxControl.Visibility = ConditionInputBox.Visibility == Visibility.Visible
                        ? Visibility.Hidden
                        : Visibility.Visible;
                    await Task.Delay(100);
                    if (inputBoxControl.Visibility == Visibility.Visible)
                        inputBoxControl.ConditionTextBox.Focus();
                    else
                        inputBoxControl.Focus();
                    return;
                case Key.Escape when IllustBrowserDialogHost.IsOpen:
                    IllustBrowserDialogHost.CurrentSession.Close();
                    return;
                case Key.Escape:
                    if (PixevalSettingDialog.IsOpen) PixevalSettingDialog.CurrentSession.Close();
                    break;
            }
        }

        private void NavigatorScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scroll(NavigatorScrollViewer, e);
        }

        private async void ReloadRecommendIllustratorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = (TextBlock)sender;
            tb.Disable();
            await AcquireRecommendUser();
            tb.Enable();
        }

        private async void RecommendIllustratorAvatar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = sender.GetDataContext<User>();
            SetImageSource(sender, await PixivIO.FromUrl(context.Avatar));
        }

        private async void KeywordTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            //TODO
            //if (AppContext.TrendingTags.IsNullOrEmpty()) AppContext.TrendingTags.AddRange(await PixivClient.Instance.GetTrendingTags());
            //TrendingTagPopup.OpenControl();
        }

        private async void KeywordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!KeywordTextBox.Text.IsNullOrEmpty()) TrendingTagPopup.CloseControl();

            if (QueryArtistToggleButton.IsChecked == true ||
                QuerySingleArtistToggleButton.IsChecked == true ||
                QuerySingleWorkToggleButton.IsChecked == true)
                return;

            var word = KeywordTextBox.Text;

            try
            {
                var result = await HttpClientFactory.AppApiService()
                    .GetAutoCompletion(new AutoCompletionRequest { Word = word });
                if (result.Tags.Any())
                {
                    AutoCompletionPopup.OpenControl();
                    AutoCompletionListBox.ItemsSource = result.Tags.Select(p => new AutoCompletion
                    {
                        Tag = p.Name,
                        TranslatedName = p.TranslatedName
                    });
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
                    KeywordTextBox.Text = ((AutoCompletion)AutoCompletionListBox.SelectedItem).Tag;

            AutoCompletionListBox.SelectedIndex = key switch
            {
                var x when x == Key.Down || x == Key.S => AutoCompletionListBox.SelectedIndex == -1
                    ? 0
                    : AutoCompletionListBox.SelectedIndex + 1,
                var x when x == Key.Up || x == Key.A => AutoCompletionListBox.SelectedIndex != -1 &&
                AutoCompletionListBox.SelectedIndex != 0
                    ? AutoCompletionListBox.SelectedIndex - 1
                    : AutoCompletionListBox.SelectedIndex,
                _ => AutoCompletionListBox.SelectedIndex
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
            ConditionInputBox.Visibility = Visibility.Hidden;
            UserBrowserConditionInputBox.Visibility = Visibility.Hidden;
            ToLoseFocus.Focus();
            CloseControls(TrendingTagPopup, AutoCompletionPopup);
            DownloadListTab.IsSelected = false;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {e.Uri.AbsoluteUri}") { CreateNoWindow = true });
        }

        #endregion

        #region 导航栏

        private void SettingsTab_OnSelected(object sender, RoutedEventArgs e)
        {
            PixevalSettingDialog.IsOpen = true;
        }

        private void DownloadListTab_OnSelected(object sender, RoutedEventArgs e)
        {
            DownloadQueueDialogHost.IsOpen = true;
        }

        private void UpdateIllustTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartup();
            MessageQueue.Enqueue(AkaI18N.SearchingUserUpdates);

            PixivHelper.Enumerate(new UserUpdateAsyncEnumerable(), NewItemsSource<Illustration>(ImageListView));
        }

        private void GalleryTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartup();
            MessageQueue.Enqueue(AkaI18N.SearchingGallery);

            PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(Session.Current.Id,
                                                                    PublicRestrictPolicy.IsChecked is true
                                                                        ? RestrictPolicy.Public
                                                                        : RestrictPolicy.Private),
                                  NewItemsSource<Illustration>(ImageListView));
        }

        private void RecommendTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartup();
            MessageQueue.Enqueue(AkaI18N.SearchingRecommend);

            PixivHelper.Enumerate(
                Settings.Global.SortOnInserting
                    ? (AbstractRecommendAsyncEnumerable)new PopularityRecommendAsyncEnumerable()
                    : new PublishDateRecommendAsyncEnumerable(), NewItemsSource<Illustration>(ImageListView), 10);
        }

        private void SpotlightTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartup();

            var iterator = new SpotlightQueryAsyncEnumerable(Settings.Global.SpotlightQueryStart);
            PixivHelper.Enumerate(iterator, NewItemsSource<SpotlightArticle>(SpotlightListView), 10);
        }

        private void FollowingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartup();
            MessageQueue.Enqueue(AkaI18N.SearchingFollower);

            PixivHelper.Enumerate(AbstractUserFollowingAsyncEnumerable.Of(Session.Current.Id,
                                                                          PublicRestrictPolicy.IsChecked is true
                                                                              ? RestrictPolicy.Public
                                                                              : RestrictPolicy.Private),
                                  NewItemsSource<User>(UserPreviewListView));
        }

        private void FollowingTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            ReleaseItemsSource(UserPreviewListView);
        }

        private void SignOutTab_OnSelected(object sender, RoutedEventArgs e)
        {
            Session.Clear();
            Settings.Initialize();
            BrowsingHistoryAccessor.GlobalLifeTimeScope.Dispose();
            BrowsingHistoryAccessor.GlobalLifeTimeScope.DropDb();
            var login = new SignIn();
            login.Show();
            Close();
        }

        private void NavigatorList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TopBarRetract((TranslateTransform)RestrictPolicySelector.RenderTransform);
            TopBarRetract((TranslateTransform)RankOptionSelector.RenderTransform);
            TrendingTagPopup.CloseControl();
            if (NavigatorList.SelectedItem is ListViewItem current)
            {
                var translateTransform = (TranslateTransform)HomeDisplayContainer.RenderTransform;
                if (current == MenuTab && !translateTransform.Y.Equals(0))
                {
                    ReleaseItemsSource(SpotlightListView);
                    ReleaseItemsSource(ImageListView);
                    ReleaseItemsSource(UserPreviewListView);
                    EnumeratingSchedule.CancelCurrent();
                    HomeContainerMoveUp();
                }
                else if (current != MenuTab && translateTransform.Y.Equals(0))
                {
                    HomeContainerMoveDown();
                }
            }
        }

        private void NavigatorList_OnKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ExternalNavigatorList_OnKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void HomeContainerMoveDown()
        {
            DoQueryButton.Disable();
            if (((TranslateTransform)HomeDisplayContainer.RenderTransform).Y.Equals(0)) HomeDisplayContainer.GetResources<Storyboard>("MoveDownAnimation").Begin();
        }

        private void HomeContainerMoveUp()
        {
            DoQueryButton.Enable();
            if (!((TranslateTransform)HomeDisplayContainer.RenderTransform).Y.Equals(0)) HomeDisplayContainer.GetResources<Storyboard>("MoveUpAnimation")?.Begin();
        }

        #endregion

        #region 图片预览

        private async void Thumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = sender.GetDataContext<Illustration>();

            if (dataContext != null && Uri.IsWellFormedUriString(dataContext.Thumbnail, UriKind.Absolute))
            {
                if (dataContext.Thumbnail != null && Uri.IsWellFormedUriString(dataContext.Thumbnail, UriKind.Absolute))
                    SetImageSource(sender, await PixivIO.FromUrl(dataContext.Thumbnail));
            }

            StartDoubleAnimationUseCubicEase(sender, "(Image.Opacity)", 0, 1, 800);
            StartDoubleAnimationUseCubicEase(sender, "(Image.RenderTransform).(ScaleTransform.ScaleX)", 1.2, 1, 800);
            StartDoubleAnimationUseCubicEase(sender, "(Image.RenderTransform).(ScaleTransform.ScaleY)", 1.2, 1, 800);
        }

        private void Thumbnail_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage(sender);
        }

        private void FavorButton_OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>(), RestrictPolicy.Public);
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
            SetImageSource((Image)sender, await PixivIO.FromUrl(sender.GetDataContext<SpotlightArticle>().Thumbnail));
        }

        private async void SpotlightContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageQueue.Enqueue(AkaI18N.SearchingSpotlight);

            var article = sender.GetDataContext<SpotlightArticle>();

            var tasks = await Tasks<string, Illustration>
                .Of(await PixivClient.Instance.GetArticleWorks(article.Id.ToString()))
                .Mapping(PixivHelper.IllustrationInfo)
                .Construct()
                .WhenAll();
            var result = tasks.Peek(i =>
            {
                i.IsManga = true;
                i.FromSpotlight = true;
                i.SpotlightTitle = article.Title;
            }).ToArray();

            PixivHelper.RecordTimelineInternal(new BrowsingHistory
            {
                BrowseObjectId = article.Id.ToString(),
                BrowseObjectState = article.Title,
                BrowseObjectThumbnail = article.Thumbnail,
                IsReferToSpotlight = true,
                Type = "spotlight"
            });

            OpenIllustBrowser(result[0].Apply(r => r.MangaMetadata = result.ToArray()));
        }

        private void DownloadSpotlightItem_OnClick(object sender, RoutedEventArgs e)
        {
            sender.GetDataContext<SpotlightArticle>().Download();
            MessageQueue.Enqueue(AkaI18N.QueuedDownload);
        }

        #endregion

        #region 右键菜单

        private void DownloadNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadOption option = null;
            if (BrowsingUser() && IsAtUploadCheckerPosition())
            {
                option = new DownloadOption
                {
                    CreateNewWhenFromUser = Settings.Global.CreateNewFolderWhenDownloadFromUser
                };
            }
            DownloadManager.EnqueueDownloadItem(sender.GetDataContext<Illustration>(), option);
            MessageQueue.Enqueue(AkaI18N.QueuedDownload);
        }

        private void DownloadToMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog(AkaI18N.PleaseSelectLocation)
            {
                InitialDirectory = Settings.Global.DownloadLocation ??
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DownloadManager.EnqueueDownloadItem(sender.GetDataContext<Illustration>(), new DownloadOption { RootDirectory = fileDialog.FileName });
                MessageQueue.Enqueue(AkaI18N.QueuedDownload);
            }
        }

        private async void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadOption option = null;
            if (BrowsingUser() && IsAtUploadCheckerPosition())
            {
                option = new DownloadOption
                {
                    CreateNewWhenFromUser = Settings.Global.CreateNewFolderWhenDownloadFromUser
                };
            }

            await Task.Run(async () =>
            {
                var source = await Dispatcher.InvokeAsync(GetImageSourceCopy);
                foreach (var illustration in source)
                    if (illustration != null)
                        DownloadManager.EnqueueDownloadItem(illustration, option);
            });
            MessageQueue.Enqueue(AkaI18N.QueuedAllToDownload);
        }

        #endregion

        #region 用户预览

        private void Timeline_OnCompleted(object sender, EventArgs e)
        {
            UserBrowserPageScrollViewer.DataContext = null;
            ReleaseImage(UserBanner);
            ReleaseImage(UserBrowserUserAvatar);
            ReleaseItemsSource(UserIllustsImageListView);
        }

        private async void PrivateFollow_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.FollowArtist(usr, RestrictPolicy.Private);
        }

        private bool _animating;

        private void ContentDisplay_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!(Navigating(GalleryTab) || Navigating(FollowingTab) || Navigating(RankingTab)) || _animating) return;
            var transform = (TranslateTransform)(Navigating(RankingTab)
                ? RankOptionSelector.RenderTransform
                : RestrictPolicySelector.RenderTransform);
            if (e.GetPosition(this).Y <= 40 && Width - e.GetPosition(this).X >= 60)
                TopBarExpand(transform);
            else if (e.GetPosition(this).Y > 60) TopBarRetract(transform);
        }

        private void PrivateRestrictPolicy_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (Navigating(GalleryTab))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingGallery);
                PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Private),
                                      NewItemsSource<Illustration>(ImageListView));
            }
            else if (Navigating(FollowingTab))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingFollower);
                PixivHelper.Enumerate(
                    AbstractUserFollowingAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Private),
                    NewItemsSource<User>(UserPreviewListView));
            }
        }

        private void PublicRestrictPolicy_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (Navigating(GalleryTab))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingGallery);
                PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Public),
                                      NewItemsSource<Illustration>(ImageListView));
            }
            else if (Navigating(FollowingTab))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingFollower);
                PixivHelper.Enumerate(
                    AbstractUserFollowingAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Public),
                    NewItemsSource<User>(UserPreviewListView));
            }
        }

        private void UserBrowserPageScrollViewer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeactivateControl();
        }

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
            PixivHelper.Enumerate(new UploadAsyncEnumerable(id),
                                  NewItemsSource<Illustration>(UserIllustsImageListView));
        }

        private void SetupUserGallery(string id)
        {
            PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(id, RestrictPolicy.Public),
                                  NewItemsSource<Illustration>(UserIllustsImageListView));
        }

        private void SetUserBanner(string id)
        {
            Task.Run(async () =>
            {
                try
                {
                    SetImageSource(UserBanner, await PixivIO.FromUrl((await HttpClientFactory.WebApiService().GetWebApiUserDetail(id)).ResponseBody.UserDetails.CoverImage.ProfileCoverImage.The720X360));
                }
                catch
                {
                    /* ignore */
                }
            });
        }

        private async void UserPrevItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var (avatar, thumbnails) = GetUserPrevImageControls(sender);
            var dataContext = sender.GetDataContext<User>();

            SetImageSource(avatar, await PixivIO.FromUrl(dataContext.Avatar));

            var counter = 0;
            foreach (var thumbnail in thumbnails)
                if (counter < dataContext.Thumbnails.Length)
                    SetImageSource(thumbnail, await PixivIO.FromUrl(dataContext.Thumbnails[counter++]));
        }

        private void UploadChecker_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsAtUploadCheckerPosition())
            {
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerIncreaseWidthAnimation")
                    .Apply(s => s.Completed
                               += (o, args) =>
                               {
                                   CheckerSnackBar.HorizontalAlignment = HorizontalAlignment.Left;
                                   CheckerSnackBarOpacityMask.HorizontalAlignment = HorizontalAlignment.Left;
                                   UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerDecreaseWidthAnimation").Begin();
                                   UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskDecreaseWidthAnimation").Begin();
                               }).Begin();
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskIncreaseWidthAnimation")
                    .Begin();
            }

            SetupUserUploads(sender.GetDataContext<User>().Id);
        }

        private void GalleryChecker_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsAtUploadCheckerPosition())
            {
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerIncreaseWidthAnimation")
                    .Apply(s => s.Completed += (o, args) =>
                    {
                        CheckerSnackBar.HorizontalAlignment = HorizontalAlignment.Right;
                        CheckerSnackBarOpacityMask.HorizontalAlignment = HorizontalAlignment.Right;
                        UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerDecreaseWidthAnimation").Begin();
                        UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskDecreaseWidthAnimation").Begin();
                    }).Begin();
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerOpacityMaskIncreaseWidthAnimation")
                    .Begin();
            }

            SetupUserGallery(sender.GetDataContext<User>().Id);
        }

        private bool IsAtUploadCheckerPosition()
        {
            return CheckerSnackBar.HorizontalAlignment == HorizontalAlignment.Left && CheckerSnackBar.Width.Equals(120);
        }

        private async void FollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.FollowArtist(usr, RestrictPolicy.Public);
        }

        private async void UnFollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.UnFollowArtist(usr);
        }

        private void ShareUserButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject($"https://www.pixiv.net/users/{sender.GetDataContext<User>().Id}");
            MessageQueue.Enqueue(AkaI18N.ShareLinkCopiedToClipboard);
        }


        private void ViewUserInBrowserButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.pixiv.net/users/{sender.GetDataContext<User>().Id}")
            {
                CreateNoWindow = true
            });
        }

        #endregion

        #region 作品浏览器

        private async void SetAsWallPaperButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var trans = (IllustTransitioner)IllustBrowserContainer.Children[1];
            var transitions = (IEnumerable<TransitionerSlide>)trans.IllustTransition.ItemsSource;
            var selectedIndex = transitions.ToList()[trans.IllustTransition.SelectedIndex];
            var location = Path.Combine(AppContext.PermanentlyFolder, "wallpaper.bmp");
            var wallPaper = new WallAdjudicator(location,
                                                (BitmapSource)((IllustPresenter)selectedIndex.Content).ImgSource);
            await wallPaper.Execute();
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
                list.AddRange(context.MangaMetadata.Select(InitTransitionerSlide));
            }
            else
            {
                list.Add(InitTransitionerSlide(context));
            }
        }

        private static TransitionerSlide InitTransitionerSlide(Illustration illust)
        {
            return new TransitionerSlide
            {
                ForwardWipe = new FadeWipe(),
                BackwardWipe = new FadeWipe(),
                Content = new IllustPresenter(illust)
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
            var txt = ((Tag)((Hyperlink)sender).DataContext).Name;

            if (!UserBrowserPageScrollViewer.Opacity.Equals(0))
                BackToMainPageButton.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent,
                    Source = this
                });

            IllustBrowserDialogHost.CurrentSession.Close();
            NavigatorList.SelectedItem = Instance.MenuTab;

            await Task.Delay(300);
            KeywordTextBox.Text = txt;
        }

        private void ShareButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject($"https://www.pixiv.net/artworks/{sender.GetDataContext<Illustration>().Id}");
            MessageQueue.Enqueue(AkaI18N.ShareLinkCopiedToClipboard);
        }

        private void ImageBrowserUserAvatar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var usr = new User { Id = sender.GetDataContext<Illustration>().UserId };
            IllustBrowserDialogHost.CurrentSession.Close();
            SetUserBrowserContext(usr);
        }

        private void ViewInBrowserButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd",
                                               $"/c start https://www.pixiv.net/artworks/{sender.GetDataContext<Illustration>().Id}")
            { CreateNoWindow = true });
        }

        private void DownloadButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DownloadManager.EnqueueDownloadItem(sender.GetDataContext<Illustration>());
            MessageQueue.Enqueue(AkaI18N.QueuedDownload);
        }

        private void IllustBrowserFavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>(), RestrictPolicy.Public);
        }

        private void IllustBrowserPrivateFavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(sender.GetDataContext<Illustration>(), RestrictPolicy.Private);
        }

        private void IllustBrowserDisfavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.Instance.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        #endregion

        #region 动态

        private async void ReferImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var trend = sender.GetDataContext<Trends>();
            var img = (Image)sender;
            if (trend.IsReferToUser)
                img.Effect = new BlurEffect
                {
                    KernelType = KernelType.Gaussian,
                    Radius = 50,
                    RenderingBias = RenderingBias.Quality
                };

            SetImageSource(sender, await PixivIO.FromUrl(trend.TrendObjectThumbnail));
        }

        private async void ReferUserAvatar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var trend = sender.GetDataContext<Trends>();
            if (trend.IsReferToUser) SetImageSource(sender, await PixivIO.FromUrl(trend.TrendObjectThumbnail));
        }

        private async void PostUserAvatar_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetImageSource(sender, await PixivIO.FromUrl(sender.GetDataContext<Trends>().PostUserThumbnail));
        }

        private void TrendsTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartup();
            MessageQueue.Enqueue(AkaI18N.SearchingTrends);

            PixivHelper.Enumerate(new TrendsAsyncEnumerable(), NewItemsSource<Trends>(TrendsListView), 20);
        }

        private async void ReferImage_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenIllustBrowser(await PixivHelper.IllustrationInfo(sender.GetDataContext<Trends>().TrendObjectId));
            e.Handled = true;
        }

        private void ReferUser_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUserBrowser();
            SetUserBrowserContext(new User { Id = sender.GetDataContext<Trends>().TrendObjectId });
            e.Handled = true;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            OpenUserBrowser();
            SetUserBrowserContext(new User { Id = ((Trends)((Hyperlink)sender).DataContext).PostUserId });
            e.Handled = true;
        }

        #endregion

        #region 榜单

        private void RankDatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Navigating(RankingTab)) GetRanking();
        }

        private void RankOptionPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Navigating(RankingTab)) GetRanking();
        }

        private void GetRanking()
        {
            var option = RankOptionPicker.SelectedItem.GetDataContext<RankOptionModel[]>().First(p => p.IsSelected);
            var dateTime = RankDatePicker.SelectedDate;

            if (option.Corresponding.AttributeAttached<ForR18Only>() &&
                Settings.Global.ExcludeTag.Any(t => t.ToUpper() == "R-18" || t.ToUpper() == "R-18G"))
            {
                MessageQueue.Enqueue(AkaI18N.RankNeedR18On);
                NewItemsSource<Illustration>(ImageListView);
                return;
            }

            if (dateTime is { } time)
            {
                PixivHelper.Enumerate(new RankingAsyncEnumerable(option.Corresponding, time),
                                      NewItemsSource<Illustration>(ImageListView));
                return;
            }

            MessageQueue.Enqueue(AkaI18N.RankDateCannotBeNull);
        }

        private void RankingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            GetRanking();
        }

        #endregion

        #region 工具

        private void TopBarRetract(TranslateTransform transform)
        {
            if (transform.Y > -60)
            {
                var animation = new DoubleAnimation(transform.Y, -60, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new CubicEase()
                };
                animation.Completed += (o, args) => _animating = false;
                _animating = true;
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void TopBarExpand(TranslateTransform transform)
        {
            if (transform.Y < 0)
            {
                var animation = new DoubleAnimation(transform.Y, 0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new CubicEase()
                };
                animation.Completed += (o, args) => _animating = false;
                _animating = true;
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private async Task AcquireRecommendUser()
        {
            var list = NewItemsSource<User>(RecommendIllustratorListBox);
            list.AddRange(await RecommendIllustratorDeferrer.Instance.Acquire(6));
        }

        private IEnumerable<Illustration> GetImageSourceCopy()
        {
            var lst = (IEnumerable<Illustration>)(BrowsingUser() ? UserIllustsImageListView : ImageListView)
                ?.ItemsSource;
            return lst != null ? lst.ToList() : new List<Illustration>();
        }

        private void QueryStartup()
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
            var list = ((Card)sender).FindVisualChildren<Image>().ToArray();

            return (list.First(p => p.Name == "UserAvatar"), list.Where(p => p.Name != "UserAvatar").ToArray());
        }

        public async void SetUserBrowserContext(User user)
        {
            var usr = await HttpClientFactory.AppApiService()
                .GetUserInformation(new UserInformationRequest { Id = $"{user.Id}" });
            var usrEntity = new User
            {
                Avatar = usr.UserEntity.ProfileImageUrls.Medium,
                Background = usr.UserEntity.ProfileImageUrls.Medium,
                Follows = (int)usr.UserProfile.TotalFollowUsers,
                Id = usr.UserEntity.Id.ToString(),
                Introduction = usr.UserEntity.Comment,
                IsFollowed = usr.UserEntity.IsFollowed,
                IsPremium = usr.UserProfile.IsPremium,
                Name = usr.UserEntity.Name,
                Thumbnails = user.Thumbnails
            };
            PixivHelper.RecordTimelineInternal(new BrowsingHistory
            {
                BrowseObjectId = usrEntity.Id,
                BrowseObjectState = usrEntity.Name,
                BrowseObjectThumbnail = usrEntity.Avatar,
                IsReferToUser = true,
                Type = "user"
            });
            UserBrowserPageScrollViewer.DataContext = usrEntity;
            SetUserBanner(usrEntity.Id);
            SetImageSource(UserBrowserUserAvatar, await PixivIO.FromUrl(usrEntity.Avatar));
            SetupUserUploads(usrEntity.Id);
        }

        public void OpenUserBrowser()
        {
            this.GetResources<Storyboard>("OpenUserBrowserAnimation").Begin();
        }

        public async void OpenIllustBrowser(Illustration illustration, bool record = true)
        {
            if (!illustration.FromSpotlight && record)
                PixivHelper.RecordTimelineInternal(new BrowsingHistory
                {
                    BrowseObjectId = illustration.Id,
                    BrowseObjectState = illustration.Title,
                    BrowseObjectThumbnail = illustration.Thumbnail,
                    IllustratorName = illustration.UserName,
                    IsReferToIllust = true,
                    Type = "illust"
                });

            IllustBrowserDialogHost.DataContext = illustration;
            await Task.Delay(100);
            IllustBrowserDialogHost.OpenControl();
            var userInfo = await HttpClientFactory.AppApiService()
                .GetUserInformation(new UserInformationRequest { Id = illustration.UserId });
            if (await PixivIO.FromUrl(userInfo.UserEntity.ProfileImageUrls.Medium) is { } avatar) SetImageSource(IllustBrowserUserAvatar, avatar);
        }

        private bool Navigating(ListViewItem item)
        {
            return NavigatorList.SelectedItem?.Equals(item) is true;
        }

        #endregion
    }
}
