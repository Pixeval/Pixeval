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
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Core.Persistent;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Objects.Generic;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Native;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using Pixeval.UI.UserControls;
using Refit;
using Xceed.Wpf.AvalonDock.Controls;
using MessageDialog = Pixeval.UI.UserControls.MessageDialog;

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

        public static readonly SnackbarMessageQueue MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
        
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            MainWindowSnackBar.MessageQueue = MessageQueue;
            previewNavigationItem = HomePageNavigationItem;

            if (Dispatcher != null)
            {
                Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            }

#pragma warning disable 4014
            AcquireRecommendUser();
#pragma warning restore 4014
        }
        
        public static void UpdateNavigationLanguage()
        {
            Instance.HomePageNavigationItem.Label = AkaI18N.HomePage;
            Instance.GalleryNavigationItem.Label = AkaI18N.MyGallery;
            Instance.FollowingNavigationItem.Label = AkaI18N.MyFollowing;
            Instance.SpotlightNavigationItem.Label = AkaI18N.Spotlight;
            Instance.RecommendNavigationItem.Label = AkaI18N.Recommend;
            Instance.IllustRankingNavigationItem.Label = AkaI18N.IllustRanking;
            Instance.FeedNavigationItem.Label = AkaI18N.Feed;
            Instance.UserUpdateNavigationItem.Label = AkaI18N.UserUpdate;
            Instance.SauceNaoNavigationItem.Label = AkaI18N.SearchImageBySource;
            Instance.DownloadQueueNavigationItem.Label = AkaI18N.DownloadQueueAndHistory;
            Instance.SettingNavigationItem.Label = AkaI18N.Setting;
            Instance.LogoutNavigationItem.Label = AkaI18N.Logout;
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
                    {
                        MessageQueue.Enqueue(AkaI18N.QueryNotResponding);
                    }
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
            UiHelper.CloseControls(TrendingTagPopup, AutoCompletionPopup);

            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                MessageQueue.Enqueue(AkaI18N.InputIsEmpty);
                return;
            }

            var keyword = KeywordTextBox.Text;
            if (QuerySingleArtistToggleButton.IsChecked == true)
            {
                ShowArtist(keyword);
            }
            else if (QueryArtistToggleButton.IsChecked == true)
            {
                TryQueryUser(keyword);
            }
            else if (QuerySingleWorkToggleButton.IsChecked == true)
            {
                TryQuerySingle(keyword);
            }
            else
            {
                QueryWorks(keyword);
            }
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
                await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest { Id = userId });
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
            MoveDownHomePage();
            SearchingHistoryManager.EnqueueSearchHistory(keyword);
            PixivHelper.Enumerate(new UserPreviewAsyncEnumerable(keyword), UiHelper.NewItemsSource<User>(UserPreviewListView));
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
                if (exception.StatusCode == HttpStatusCode.NotFound || exception.StatusCode == HttpStatusCode.BadRequest)
                {
                    MessageQueue.Enqueue(AkaI18N.IdDoNotExists);
                }
                else
                {
                    throw;
                }
            }
        }

        private void QueryWorks(string keyword)
        {
            MoveDownHomePage();
            SearchingHistoryManager.EnqueueSearchHistory(keyword);
            PixivHelper.Enumerate(Settings.Global.SortOnInserting ? (AbstractQueryAsyncEnumerable) new PopularityQueryAsyncEnumerable(keyword, Settings.Global.TagMatchOption, Session.Current.IsPremium, Settings.Global.QueryStart) : new PublishDateQueryAsyncEnumerable(keyword, Settings.Global.TagMatchOption, Session.Current.IsPremium, Settings.Global.QueryStart), UiHelper.NewItemsSource<Illustration>(ImageListView), Settings.Global.QueryPages);
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

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateNavigationLanguage();
            await AddUserNameAndAvatar();
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
            NavigatorList.SelectedItem = previewNavigationItem;
        }

        private void DownloadQueueDialogHost_OnDialogOpened(object sender, DialogOpenedEventArgs e)
        {
            var content = (DownloadQueue) DownloadQueueDialogHost.DialogContent;
            if (content == null)
                return;
            if (content.BrowsingHistoryTab.IsSelected)
            {
                content.RefreshBrowsingHistory();
            }
            else if (content.FavoriteSpotlightTab.IsSelected)
            {
                content.RefreshFavoriteSpotlight();
            }
        }

        private void DownloadQueueDialogHost_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            UiHelper.ReleaseItemsSource(((DownloadQueue) DownloadQueueDialogHost.DialogContent)?.BrowsingHistoryQueue);
            NavigatorList.SelectedItem = previewNavigationItem;
        }

        #region 主窗口

        private async void UserPreviewPopupContent_OnLoaded(object sender, RoutedEventArgs e)
        {
            var userInfo = sender.GetDataContext<User>();
            var ctrl = (UserPreviewPopupContent) sender;
            var usr = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest { Id = $"{sender.GetDataContext<User>().Id}" });
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
                Thumbnails = sender.GetDataContext<User>().Thumbnails
            };
            ctrl.DataContext = usrEntity;
            var result = await Tasks<string, BitmapImage>.Of(userInfo.Thumbnails.Take(3)).Mapping(PixivIO.FromUrl).Construct().WhenAll();
            ctrl.SetImages(result[0], result[1], result[2]);
        }

        private void RecommendIllustratorContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetUserBrowserContext(sender.GetDataContext<User>());
        }

        private async void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var lst = (Collection<Illustration>) (BrowsingUser() ? UserIllustsImageListView : ImageListView)?.ItemsSource;

            var browsing = lst != null && IllustBrowserDialogHost.IsOpen;
            var dataContext = IllustBrowserDialogHost.GetDataContext<Illustration>();

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.Key)
            {
                case Key.Oem5:
                    if (SettingsControl.IsOpen)
                        break;
                    var inputBoxControl = BrowsingUser() ? UserBrowserConditionInputBox : ConditionInputBox;
                    inputBoxControl.Visibility = ConditionInputBox.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    await Task.Delay(100);
                    if (inputBoxControl.Visibility == Visibility.Visible)
                    {
                        inputBoxControl.ConditionTextBox.Focus();
                    }
                    else
                    {
                        inputBoxControl.Focus();
                    }
                    return;
                case Key.Escape when IllustBrowserDialogHost.IsOpen:
                    IllustBrowserDialogHost.CurrentSession?.Close();
                    return;
                case Key.Escape when PixevalSettingDialog.IsOpen:
                    if (PixevalSettingDialog.IsOpen)
                    {
                        PixevalSettingDialog.CurrentSession?.Close();
                    }
                    break;
                case var x when (x == Key.PageDown || x == Key.Right || x == Key.Space) && browsing && !disableKeyEvent:
                    var nextIndex = lst!.IndexOf(dataContext) + 1;
                    if (nextIndex <= lst!.Count - 1)
                    {
                        SetIllustBrowserIllustrationDataContext(lst[nextIndex]);
                    }
                    break;
                case var x when (x == Key.PageUp || x == Key.Left) && browsing && !disableKeyEvent:
                    var prevIndex = lst!.IndexOf(dataContext) - 1;
                    if (prevIndex >= 0)
                    {
                        SetIllustBrowserIllustrationDataContext(lst[prevIndex]);
                    }
                    break;
            }
        }

        private void NavigatorScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UiHelper.Scroll(NavigatorScrollViewer, e);
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
            UiHelper.SetImageSource(sender, await PixivIO.FromUrl(context.Avatar));
        }

        private async void KeywordTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (PixevalContext.TrendingTags.IsNullOrEmpty())
            {
                PixevalContext.TrendingTags.AddRange(await PixivClient.GetTrendingTags());
            }
            TrendingTagPopup.OpenControl();
        }

        private async void KeywordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!KeywordTextBox.Text.IsNullOrEmpty())
            {
                TrendingTagPopup.CloseControl();
            }

            if (QueryArtistToggleButton.IsChecked == true || QuerySingleArtistToggleButton.IsChecked == true || QuerySingleWorkToggleButton.IsChecked == true || !doAutoCompletionQuery)
            {
                return;
            }

            var word = KeywordTextBox.Text;

            try
            {
                var result = await HttpClientFactory.AppApiService.GetAutoCompletion(new AutoCompletionRequest { Word = word });
                if (result.Tags.Any())
                {
                    AutoCompletionPopup.OpenControl();
                    AutoCompletionListBox.ItemsSource = result.Tags.Select(p => new AutoCompletion { Tag = p.Name, TranslatedName = p.TranslatedName });
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
            {
                if (AutoCompletionListBox.SelectedIndex != -1)
                {
                    SetAutoCompletedResultAndClosePopup(((AutoCompletion) AutoCompletionListBox.SelectedItem).Tag);
                }
            }

            AutoCompletionListBox.SelectedIndex = key switch
            {
                var x when x == Key.Down || x == Key.S => AutoCompletionListBox.SelectedIndex == -1 ? 0 : AutoCompletionListBox.SelectedIndex + 1,
                var x when x == Key.Up || x == Key.A   => AutoCompletionListBox.SelectedIndex != -1 && AutoCompletionListBox.SelectedIndex != 0 ? AutoCompletionListBox.SelectedIndex - 1 : AutoCompletionListBox.SelectedIndex,
                _                                      => AutoCompletionListBox.SelectedIndex
            };
        }

        private void AutoCompletionListBox_OnSelectionChanged(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            SetAutoCompletedResultAndClosePopup(sender.GetDataContext<AutoCompletion>().Tag);
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

        private void ContentContainer_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
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
            UiHelper.CloseControls(TrendingTagPopup, AutoCompletionPopup);
            DownloadQueueNavigationItem.IsSelected = false;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {e.Uri.AbsoluteUri}") { CreateNoWindow = true });
        }

        #endregion

        #region 导航栏

        private bool onEffectivelyUnselectableItems;

        private INavigationItem previewNavigationItem;
        
        private void NavigatorList_OnNavigationItemSelected(object sender, NavigationItemSelectedEventArgs args)
        {
            // the only possibility of which the onEffectivelyUnselectableItems is true while this event firing
            // is that the DownloadQueueDialogHost or SettingsDialogHost is closing, we do nothing here because
            // it's just a reset
            if (onEffectivelyUnselectableItems)
            {
                onEffectivelyUnselectableItems = false;
                return;
            }

            TopBarRetract((TranslateTransform) RestrictPolicySelector.RenderTransform);
            TopBarRetract((TranslateTransform) RankOptionSelector.RenderTransform);
            TrendingTagPopup.CloseControl();

            var item = args.NavigationItem;

            if (item != SettingNavigationItem && item != DownloadQueueNavigationItem)
            {
                previewNavigationItem = item;
            }
            else
            {
                onEffectivelyUnselectableItems = true;
            }
            var translateTransform = (TranslateTransform) HomeDisplayContainer.RenderTransform;
            if (item == HomePageNavigationItem && !translateTransform.Y.Equals(0))
            {
                UiHelper.ReleaseItemsSource(SpotlightListView);
                UiHelper.ReleaseItemsSource(ImageListView);
                UiHelper.ReleaseItemsSource(UserPreviewListView);
                EnumeratingSchedule.CancelCurrent();
                MoveUpHomePage();
            }
            else if (item != HomePageNavigationItem && translateTransform.Y.Equals(0) && !onEffectivelyUnselectableItems)
            {
                MoveDownHomePage();
            }

            switch (args.NavigationItem)
            {
                case var x when x == IllustRankingNavigationItem:
                    SetVisibility(ImageListView);
                    GetRanking();
                    break;
                case var x when x == FeedNavigationItem:
                    SetVisibility(FeedsListView);
                    MoveDownHomePage();
                    MessageQueue.Enqueue(AkaI18N.SearchingTrends);
                    PixivHelper.Enumerate(new FeedsAsyncEnumerable(), UiHelper.NewItemsSource<Trends>(FeedsListView), 20);
                    break;
                // these two tabs is considered effectively unselectable, they participate 
                // as buttons in the navigation list
                // ----------------------------------------------------------
                case var x when x == SettingNavigationItem:
                    PixevalSettingDialog.IsOpen = true;
                    break;
                case var x when x == DownloadQueueNavigationItem:
                    DownloadQueueDialogHost.IsOpen = true;
                    break;
                // ----------------------------------------------------------
                case var x when x == UserUpdateNavigationItem:
                    SetVisibility(ImageListView);
                    MoveDownHomePage();
                    MessageQueue.Enqueue(AkaI18N.SearchingUserUpdates);
                    PixivHelper.Enumerate(new UserUpdateAsyncEnumerable(), UiHelper.NewItemsSource<Illustration>(ImageListView));
                    break;
                case var x when x == GalleryNavigationItem:
                    SetVisibility(ImageListView);
                    MoveDownHomePage();
                    MessageQueue.Enqueue(AkaI18N.SearchingGallery);
                    PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(Session.Current.Id, PublicRestrictPolicy.IsChecked is true ? RestrictPolicy.Public : RestrictPolicy.Private), UiHelper.NewItemsSource<Illustration>(ImageListView));
                    break;
                case var x when x == RecommendNavigationItem:
                    SetVisibility(ImageListView);
                    MoveDownHomePage();
                    MessageQueue.Enqueue(AkaI18N.SearchingRecommend);
                    PixivHelper.Enumerate(Settings.Global.SortOnInserting ? (AbstractRecommendAsyncEnumerable) new PopularityRecommendAsyncEnumerable() : new PlainRecommendAsyncEnumerable(), UiHelper.NewItemsSource<Illustration>(ImageListView), 10);
                    break;
                case var x when x == SpotlightNavigationItem:
                    SetVisibility(SpotlightListView);
                    MoveDownHomePage();
                    var iterator = new SpotlightQueryAsyncEnumerable(Settings.Global.SpotlightQueryStart);
                    PixivHelper.Enumerate(iterator, UiHelper.NewItemsSource<SpotlightArticle>(SpotlightListView), 10);
                    break;
                case var x when x == FollowingNavigationItem:
                    SetVisibility(UserPreviewListView);
                    MoveDownHomePage();
                    MessageQueue.Enqueue(AkaI18N.SearchingFollower);
                    PixivHelper.Enumerate(AbstractUserFollowingAsyncEnumerable.Of(Session.Current.Id, PublicRestrictPolicy.IsChecked is true ? RestrictPolicy.Public : RestrictPolicy.Private), UiHelper.NewItemsSource<User>(UserPreviewListView));
                    break;
                case var x when x == LogoutNavigationItem:
                    Session.Clear();
                    Settings.Initialize();
                    BrowsingHistoryAccessor.GlobalLifeTimeScope.Dispose();
                    BrowsingHistoryAccessor.GlobalLifeTimeScope.DropDb();
                    FavoriteSpotlightAccessor.GlobalLifeTimeScope.Dispose();
                    FavoriteSpotlightAccessor.GlobalLifeTimeScope.DropDb();
                    var login = new SignIn();
                    login.Show();
                    Close();
                    break;
            }
            
            // if the selected items is not download queue or settings or some sort of effectively
            // unselectable items, clear the lists because it refers to an efficient tab switching
            if (!onEffectivelyUnselectableItems)
            {
                if (item != SpotlightNavigationItem)
                {
                    UiHelper.ReleaseItemsSource(SpotlightListView);
                }
                else if (item != FollowingNavigationItem)
                {
                    UiHelper.ReleaseItemsSource(UserPreviewListView);
                }
                else if (item != FeedNavigationItem)
                {
                    UiHelper.ReleaseItemsSource(FeedsListView);
                }
            }
            
            void SetVisibility(ListView listView)
            {
                listView.Visibility = Visibility.Visible;
                var lists = new List<ListView> { FeedsListView, ImageListView, UserPreviewListView, SpotlightListView };
                lists.Remove(listView);
                lists.ForEach(lv => lv.Visibility = Visibility.Hidden);
            }
        }

        #endregion

        #region 图片预览

        private async void Thumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = sender.GetDataContext<Illustration>();
            if (dataContext.Incomplete)
            {
                dataContext = await PixivHelper.IllustrationInfo(dataContext.Id);
            }

            if (dataContext?.Thumbnail is { } url && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                UiHelper.SetImageSource(sender, await PixivIO.FromUrl(url));
            }

            UiHelper.StartDoubleAnimationUseCubicEase(sender, "(Image.Opacity)", 0, 1, 800);
            UiHelper.StartDoubleAnimationUseCubicEase(sender, "(Image.RenderTransform).(ScaleTransform.ScaleX)", 1.2, 1, 800);
            UiHelper.StartDoubleAnimationUseCubicEase(sender, "(Image.RenderTransform).(ScaleTransform.ScaleY)", 1.2, 1, 800);
        }

        private void Thumbnail_OnUnloaded(object sender, RoutedEventArgs e)
        {
            UiHelper.ReleaseImage(sender);
        }

        private void FavorButton_OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            PixivClient.PostFavoriteAsync(sender.GetDataContext<Illustration>(), RestrictPolicy.Public);
            e.Handled = true;
        }

        private void DisfavorButton_OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            PixivClient.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
            e.Handled = true;
        }

        #endregion

        #region Spotlight

        private async void SpotlightThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            UiHelper.SetImageSource((Image) sender, await PixivIO.FromUrl(sender.GetDataContext<SpotlightArticle>().Thumbnail));
        }

        private async void SpotlightContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageQueue.Enqueue(AkaI18N.SearchingSpotlight);

            var article = sender.GetDataContext<SpotlightArticle>();

            var tasks = await Tasks<string, Illustration>.Of(await PixivClient.GetArticleWorks(article.Id.ToString())).Mapping(PixivHelper.IllustrationInfo).Construct().WhenAll();
            var result = tasks.Where(i => i != null).SelectMany(i => i.IsManga ? i.MangaMetadata : new[] { i }).Peek(i =>
            {
                i.FromSpotlight = true;
                i.IsManga = true;
                i.SpotlightTitle = article.Title;
                i.SpotlightArticleId = article.Id.ToString();
            }).ToArray();

            PixivHelper.RecordTimelineInternal(new BrowsingHistory
            {
                BrowseObjectId = article.Id.ToString(),
                BrowseObjectState = article.Title,
                BrowseObjectThumbnail = article.Thumbnail,
                IsReferToSpotlight = true,
                Type = "spotlight"
            });

            OpenIllustBrowser(result[0].Apply(r => r.MangaMetadata = result));
        }

        private void DownloadSpotlightItem_OnClick(object sender, RoutedEventArgs e)
        {
            sender.GetDataContext<SpotlightArticle>().Download();
            MessageQueue.Enqueue(AkaI18N.QueuedDownload);
        }

        private void AddSpotlightToFavorite_OnClick(object sender, RoutedEventArgs e)
        {
            var article = sender.GetDataContext<SpotlightArticle>();
            FavoriteSpotlightAccessor.GlobalLifeTimeScope.Insert(new FavoriteSpotlight
            {
                SpotlightArticleId = article.Id.ToString(),
                SpotlightThumbnail = article.Thumbnail,
                SpotlightTitle = article.Title
            });
        }

        #endregion

        #region 右键菜单

        private void DownloadNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadManager.EnqueueDownloadItem(sender.GetDataContext<Illustration>());
            MessageQueue.Enqueue(AkaI18N.QueuedDownload);
        }

        private void DownloadToMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();
            using var fileDialog = new CommonSaveFileDialog(AkaI18N.PleaseSelectLocation)
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                DefaultExtension = illust.IsUgoira ? "gif" : Path.GetExtension(illust.GetDownloadUrl())![1..],
                AlwaysAppendDefaultExtension = true,
                DefaultFileName = illust.Id
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DownloadManager.EnqueueDownloadItem(illust, fileDialog.FileName);
                MessageQueue.Enqueue(AkaI18N.QueuedDownload);
            }
        }

        private async void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (await MessageDialog.Warning(WarningDialog, AkaI18N.BatchDownloadAcknowledgment, true) == MessageDialogResult.Yes)
            {
                await Task.Run(async () =>
                {
                    var source = await Dispatcher.InvokeAsync(GetImageSourceCopy);
                    foreach (var illustration in source)
                    {
                        if (illustration != null)
                        {
                            DownloadManager.EnqueueDownloadItem(illustration);
                        }
                    }
                });
                MessageQueue.Enqueue(AkaI18N.QueuedAllToDownload);
            }
        }

        #endregion

        #region 用户预览

        private void UserIllustsImageListView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UiHelper.Scroll(UserBrowserPageScrollViewer, e);
        }

        private void CloseUserBrowserAnimation_OnCompleted(object sender, EventArgs e)
        {
            UserBrowserPageScrollViewer.DataContext = null;
            UiHelper.ReleaseImage(UserBrowserUserAvatar);
            UiHelper.ReleaseItemsSource(UserIllustsImageListView);
        }

        private async void PrivateFollow_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.FollowArtist(usr, RestrictPolicy.Private);
        }

        private bool animating;

        private void ContentDisplay_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!(Navigating(GalleryNavigationItem) || Navigating(FollowingNavigationItem) || Navigating(IllustRankingNavigationItem)) || animating)
            {
                return;
            }
            var transform = (TranslateTransform) (Navigating(IllustRankingNavigationItem) ? RankOptionSelector.RenderTransform : RestrictPolicySelector.RenderTransform);
            if (e.GetPosition(this).Y <= 40 && Width - e.GetPosition(this).X >= 60)
            {
                TopBarExpand(transform);
            }
            else if (e.GetPosition(this).Y > 40)
            {
                TopBarRetract(transform);
            }
        }

        private void PrivateRestrictPolicy_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            if (Navigating(GalleryNavigationItem))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingGallery);
                PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Private), UiHelper.NewItemsSource<Illustration>(ImageListView));
            }
            else if (Navigating(FollowingNavigationItem))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingFollower);
                PixivHelper.Enumerate(AbstractUserFollowingAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Private), UiHelper.NewItemsSource<User>(UserPreviewListView));
            }
        }

        private void PublicRestrictPolicy_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            if (Navigating(GalleryNavigationItem))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingGallery);
                PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Public), UiHelper.NewItemsSource<Illustration>(ImageListView));
            }
            else if (Navigating(FollowingNavigationItem))
            {
                MessageQueue.Enqueue(AkaI18N.SearchingFollower);
                PixivHelper.Enumerate(AbstractUserFollowingAsyncEnumerable.Of(Session.Current.Id, RestrictPolicy.Public), UiHelper.NewItemsSource<User>(UserPreviewListView));
            }
        }

        private void UserBrowserPageScrollViewer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeactivateControl();
        }

        private void UserPrevItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var usrDateContext = sender.GetDataContext<User>();

            SetUserBrowserContext(usrDateContext);
        }

        private void SetupUserUploads(string id)
        {
            PixivHelper.Enumerate(new UploadAsyncEnumerable(id), UiHelper.NewItemsSource<Illustration>(UserIllustsImageListView));
        }

        private void SetupUserGallery(string id)
        {
            PixivHelper.Enumerate(AbstractGalleryAsyncEnumerable.Of(id, RestrictPolicy.Public), UiHelper.NewItemsSource<Illustration>(UserIllustsImageListView));
        }

        private async void UserPrevItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var (avatar, thumbnails) = GetUserPrevImageControls(sender);
            var dataContext = sender.GetDataContext<User>();

            UiHelper.SetImageSource(avatar, await PixivIO.FromUrl(dataContext.Avatar));

            var counter = 0;
            foreach (var thumbnail in thumbnails)
            {
                if (counter < dataContext.Thumbnails.Length)
                {
                    UiHelper.SetImageSource(thumbnail, await PixivIO.FromUrl(dataContext.Thumbnails[counter++]));
                }
            }
        }

        private void UploadChecker_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsAtUploadCheckerPosition())
            {
                UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerIncreaseWidthAnimation").Apply(s => s.Completed += (o, args) =>
                {
                    CheckerSnackBar.HorizontalAlignment = HorizontalAlignment.Left;
                    UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerDecreaseWidthAnimation").Begin();
                }).Begin();
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
                    UserBrowserPageScrollViewer.GetResources<Storyboard>("CheckerDecreaseWidthAnimation").Begin();
                }).Begin();
            }

            SetupUserGallery(sender.GetDataContext<User>().Id);
        }

        private bool IsAtUploadCheckerPosition()
        {
            return CheckerSnackBar.HorizontalAlignment == HorizontalAlignment.Left && CheckerSnackBar.Width.Equals(100);
        }

        private async void FollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.FollowArtist(usr, RestrictPolicy.Public);
        }

        private async void UnFollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.UnFollowArtist(usr);
        }

        private void ShareUserButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject($"https://www.pixiv.net/users/{sender.GetDataContext<User>().Id}");
            MessageQueue.Enqueue(AkaI18N.ShareLinkCopiedToClipboard);
        }


        private void ViewUserInBrowserButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.pixiv.net/users/{sender.GetDataContext<User>().Id}") { CreateNoWindow = true });
        }

        #endregion

        #region 作品浏览器

        private async void SetAsWallPaperButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var trans = (IllustTransitioner) IllustBrowserContainer.Children[1];
            var transitions = (IEnumerable<TransitionerSlide>) trans.IllustTransition.ItemsSource;
            var selectedIndex = transitions.ToList()[trans.IllustTransition.SelectedIndex];
            var location = Path.Combine(PixevalContext.PermanentlyFolder, "wallpaper.bmp");
            var wallPaper = new WallpaperManager(location, (BitmapSource) ((IllustPresenter) selectedIndex.Content).ImgSource);
            await wallPaper.Execute();
        }

        private static TransitionerSlide InitTransitionerSlide(Illustration illust)
        {
            return new TransitionerSlide { ForwardWipe = new FadeWipe(), BackwardWipe = new FadeWipe(), Content = new IllustPresenter(illust) };
        }

        private void IllustBrowserDialogHost_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            IllustBrowserContainer.Children.RemoveAt(1);
            UiHelper.ReleaseImage(IllustBrowserUserAvatar);
            IllustBrowserDialogHost.DataContext = null;
        }

        private async void TagNavigateHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var txt = ((Tag) ((Hyperlink) sender).DataContext).Name;

            if (!UserBrowserPageScrollViewer.Opacity.Equals(0))
            {
                BackToMainPageButton.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent, Source = this });
            }

            IllustBrowserDialogHost.CurrentSession?.Close();
            NavigatorList.SelectedItem = HomePageNavigationItem;

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
            IllustBrowserDialogHost.CurrentSession?.Close();
            SetUserBrowserContext(usr);
        }

        private void ViewInBrowserButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.pixiv.net/artworks/{sender.GetDataContext<Illustration>().Id}") { CreateNoWindow = true });
        }

        private void DownloadButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DownloadManager.EnqueueDownloadItem(sender.GetDataContext<Illustration>());
            MessageQueue.Enqueue(AkaI18N.QueuedDownload);
        }

        private void IllustBrowserFavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.PostFavoriteAsync(sender.GetDataContext<Illustration>(), RestrictPolicy.Public);
        }

        private void IllustBrowserPrivateFavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.PostFavoriteAsync(sender.GetDataContext<Illustration>(), RestrictPolicy.Private);
        }

        private void IllustBrowserDisfavorButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixivClient.RemoveFavoriteAsync(sender.GetDataContext<Illustration>());
        }

        #endregion

        #region 动态

        private async void ReferImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var trend = sender.GetDataContext<Trends>();
            var img = (Image) sender;
            if (trend.IsReferToUser)
            {
                img.Effect = new BlurEffect { KernelType = KernelType.Gaussian, Radius = 50, RenderingBias = RenderingBias.Quality };
            }

            UiHelper.SetImageSource(sender, await PixivIO.FromUrl(trend.TrendObjectThumbnail));
        }

        private async void ReferUserAvatar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var trend = sender.GetDataContext<Trends>();
            if (trend.IsReferToUser)
            {
                UiHelper.SetImageSource(sender, await PixivIO.FromUrl(trend.TrendObjectThumbnail));
            }
        }

        private async void PostUserAvatar_OnLoaded(object sender, RoutedEventArgs e)
        {
            UiHelper.SetImageSource(sender, await PixivIO.FromUrl(sender.GetDataContext<Trends>().PostUserThumbnail));
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
            SetUserBrowserContext(new User { Id = ((Trends) ((Hyperlink) sender).DataContext).PostUserId });
            e.Handled = true;
        }

        #endregion

        #region 榜单

        private volatile RankOptionModel currentSelected;

        private void RankDatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Navigating(IllustRankingNavigationItem))
            {
                GetRanking();
            }
        }
        
        private void RankOptionPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Navigating(IllustRankingNavigationItem))
            {
                var option = e.AddedItems[0].GetDataContext<RankOptionModel[]>().First(p => p.IsSelected);
                if (option == currentSelected)
                    return;
                if (option.Corresponding.AttributeAttached<ForR18Only>() && Settings.Global.ExcludeTag.Any(t => t.ToUpper() == "R-18" || t.ToUpper() == "R-18G"))
                {
                    MessageQueue.Enqueue(AkaI18N.RankNeedR18On);
                    // 用这方法解决问题真tm傻逼
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        Dispatcher.Invoke(() => currentSelected.IsSelected = true);
                    });
                    return;
                }
                GetRanking();
            }
        }

        private void GetRanking()
        {
            var option = RankOptionPicker.SelectedItem.GetDataContext<RankOptionModel[]>().First(p => p.IsSelected);
            var dateTime = RankDatePicker.SelectedDate;
            currentSelected = option;

            if (dateTime is { } time)
            {
                PixivHelper.Enumerate(new RankingAsyncEnumerable(option.Corresponding, time), UiHelper.NewItemsSource<Illustration>(ImageListView));
                return;
            }

            MessageQueue.Enqueue(AkaI18N.RankDateCannotBeNull);
        }

        #endregion

        #region 工具

        // indicates whether to query new auto completion results when KeywordTextBox's text changes
        private bool doAutoCompletionQuery = true;
        private static readonly object AutoCompletionLock = new object();
        
        // Sets the keyword text-box's value to the selected auto completion result
        private void SetAutoCompletedResultAndClosePopup(string text)
        {
            lock (AutoCompletionLock)
            {
                doAutoCompletionQuery = false;
                KeywordTextBox.Text = text;
                doAutoCompletionQuery = true;
                AutoCompletionPopup.CloseControl();
            }
        }
        
        private bool disableKeyEvent;

        private void TopBarRetract(TranslateTransform transform)
        {
            if (transform.Y > -60)
            {
                var animation = new DoubleAnimation(transform.Y, -60, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
                animation.Completed += (o, args) => animating = false;
                animating = true;
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void TopBarExpand(TranslateTransform transform)
        {
            if (transform.Y < 0)
            {
                var animation = new DoubleAnimation(transform.Y, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() };
                animation.Completed += (o, args) => animating = false;
                animating = true;
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private async Task AcquireRecommendUser()
        {
            var list = UiHelper.NewItemsSource<User>(RecommendIllustratorListBox);
            list.AddRange(await RecommendIllustratorDeferrer.Instance.Acquire(6));
        }

        private IEnumerable<Illustration> GetImageSourceCopy()
        {
            var lst = (IEnumerable<Illustration>) (BrowsingUser() ? UserIllustsImageListView : ImageListView)?.ItemsSource;
            return lst != null ? lst.ToList() : new List<Illustration>();
        }

        private void MoveUpHomePage()
        {
            NavigateTo(HomePageNavigationItem);
            DoQueryButton.Enable();
            if (!((TranslateTransform) HomeDisplayContainer.RenderTransform).Y.Equals(0))
            {
                HomeDisplayContainer.GetResources<Storyboard>("MoveUpAnimation")?.Begin();
            }
        }

        private void MoveDownHomePage()
        {
            HomePageNavigationItem.IsSelected = false;
            DoQueryButton.Disable();
            if (((TranslateTransform) HomeDisplayContainer.RenderTransform).Y.Equals(0))
            {
                HomeDisplayContainer.GetResources<Storyboard>("MoveDownAnimation").Begin();
            }
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

        public async void SetUserBrowserContext(User user)
        {
            var usr = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest { Id = $"{user.Id}" });
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
            PixivHelper.RecordTimelineInternal(new BrowsingHistory
            {
                BrowseObjectId = usrEntity.Id,
                BrowseObjectState = usrEntity.Name,
                BrowseObjectThumbnail = usrEntity.Avatar,
                IsReferToUser = true,
                Type = "user"
            });
            UserBrowserPageScrollViewer.DataContext = usrEntity;
            UiHelper.SetImageSource(UserBrowserUserAvatar, await PixivIO.FromUrl(usrEntity.Avatar));
            SetupUserUploads(usrEntity.Id);
        }

        public void OpenUserBrowser()
        {
            this.GetResources<Storyboard>("OpenUserBrowserAnimation").Begin();
        }

        private void SetIllustBrowserIllustrationDataContext(Illustration illustration, bool record = true)
        {
            if (!illustration.FromSpotlight && record)
            {
                PixivHelper.RecordTimelineInternal(new BrowsingHistory
                {
                    BrowseObjectId = illustration.Id,
                    BrowseObjectState = illustration.Title,
                    BrowseObjectThumbnail = illustration.Thumbnail,
                    IllustratorName = illustration.UserName,
                    IsReferToIllust = true,
                    Type = "illust"
                });
            }

            IllustBrowserDialogHost.DataContext = illustration;
            disableKeyEvent = true;
            Task.WhenAll(Task.Run(async () =>
            {
                var userInfo = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest { Id = illustration.UserId });
                if (await PixivIO.FromUrl(userInfo.UserEntity.ProfileImageUrls.Medium) is { } avatar)
                {
                    UiHelper.SetImageSource(IllustBrowserUserAvatar, avatar);
                }
            }), Task.Run(() =>
            {
                var list = new ObservableCollection<TransitionerSlide>();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var template = new IllustTransitioner(list);
                    if (IllustBrowserContainer.Children[1] is IllustTransitioner)
                    {
                        IllustBrowserContainer.Children.RemoveAt(1);
                    }
                    IllustBrowserContainer.Children.Insert(1, template);
                    if (illustration.IsManga)
                    {
                        list.AddRange(illustration.MangaMetadata.Select(InitTransitionerSlide));
                    }
                    else
                    {
                        list.Add(InitTransitionerSlide(illustration));
                    }
                });
            })).ContinueWith(_ => disableKeyEvent = false, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async void OpenIllustBrowser(Illustration illustration, bool record = true)
        {
            SetIllustBrowserIllustrationDataContext(illustration, record);
            await Task.Delay(100);
            IllustBrowserDialogHost.OpenControl();
        }

        private bool Navigating(INavigationItem item)
        {
            return NavigatorList.SelectedItem?.Equals(item) is true;
        }

        private void NavigateTo(INavigationItem item)
        {
            NavigatorList.SelectedItem = item;
        }

        #endregion
    }
}