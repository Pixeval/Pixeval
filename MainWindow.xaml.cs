using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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
using Pixeval.Data.Model.ViewModel;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Pixeval.Persisting;
using Refit;
using Xceed.Wpf.AvalonDock.Controls;

namespace Pixeval
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static MainWindow Instance;

        private readonly SnackbarMessageQueue messageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(500))
        {
            IgnoreDuplicate = true
        };

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();
            NavigatorList.SelectedItem = MenuTab;
            MainWindowSnackBar.MessageQueue = messageQueue;

            UiHelper.SetItemsSource(ToDownloadListView, DownloadList.ToDownloadList);
            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            QueryOptionPopup.IsOpen = false;

            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                Notice("请在输入搜索关键字后再进行搜索~");
                return;
            }

            if (QuerySingleWorkToggleButton.IsChecked == true)
            {
                TryQuerySingle();
            }
            else
            {
                QueryWorks();
            }
        }

        private async void TryQuerySingle()
        {
            if (!int.TryParse(KeywordTextBox.Text, out _))
            {
                Notice("搜索单个作品时必须输入纯数字哟~ ");
                return;
            }
            try
            {
                IllustViewer.Show(await PixivHelper.IllustrationInfo(KeywordTextBox.Text));
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound && exception.StatusCode == HttpStatusCode.BadRequest)
                    Notice("您所输入的ID不存在, 请检查后再试一次吧~");
                else throw;
            }
        }

        private async void QueryWorks()
        {
            QueryStartUp();

            var pages = await PixivHelper.GetQueryPagesCount(KeywordTextBox.Text);

            NoticeProgress(pages);

            var container = new ObservableCollection<Illustration>();
            ImageListView.ItemsSource = container;

            Query(new QueryIterator(KeywordTextBox.Text, pages, Settings.Global.QueryStart), container, false);
        }

        #region 主窗口

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.InnerException != null && e.Exception.InnerException.Message.Contains("The server returned an invalid or unrecognized response"))
            {
            }
            if (e.Exception is QueryNotRespondingException)
            {
                ImageListView.ItemsSource = null;
                Notice("抱歉, Pixeval无法根据当前的设置找到任何作品, 或许是您的页码设置过大/关键字不存在/没有收藏任何作品, 请检查后再尝试吧~");
            }
            else
            {
                Notice(e.Exception.Message);
            }

            e.Handled = true;
        }

        private void KeywordTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            QueryOptionPopup.IsOpen = true;
        }

        private void QuerySingleWorkToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            QueryArtistToggleButton.IsChecked = false;
        }

        private void QueryArtistToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            QuerySingleWorkToggleButton.IsChecked = false;
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
            NoticeProgress();

            Query(new GalleryIterator(Identity.Global.Id), UiHelper.NewItemsSource<Illustration>(ImageListView), true);
        }

        private void RankingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            QueryStartUp();
            NoticeProgress();

            Query(new RankingIterator(), UiHelper.NewItemsSource<Illustration>(ImageListView), true);
        }

        private async void SpotlightTab_OnSelected(object sender, RoutedEventArgs e)
        {
            UiHelper.HideControl(ImageListView);

            QueryStartUp();

            var lst = UiHelper.NewItemsSource<SpotlightArticle>(SpotlightListView);
            var iterator = new SpotlightQueryIterator(Settings.Global.SpotlightQueryStart, Settings.Global.QueryPages);

            await foreach (var article in iterator.GetSpotlight()) lst.Add(article);
        }

        private void SpotlightTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            UiHelper.ShowControl(ImageListView);

            UiHelper.ReleaseItemsSource(SpotlightListView);
        }

        private async void FollowingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            UiHelper.HideControl(ImageListView);
            UiHelper.HideControl(SpotlightListView);

            QueryStartUp();
            NoticeProgress();

            var iterator = new UserFollowingIterator(Identity.Global.Id);
            var container = UiHelper.NewItemsSource<UserPreview>(UserPreviewListView);

            await foreach (var preview in iterator.GetUserPreviews()) container.Add(preview);
        }

        private void FollowingTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            UiHelper.ShowControl(ImageListView);
            UiHelper.ShowControl(SpotlightListView);

            UiHelper.ReleaseItemsSource(UserPreviewListView);
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

            if (Uri.IsWellFormedUriString(dataContext.Thumbnail, UriKind.Absolute)) UiHelper.SetImageSource((Image) sender, await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnail, dataContext.Id));

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
            NoticeProgress();

            var article = sender.GetDataContext<SpotlightArticle>();

            var extractor = new SpotlightContentExtractor(article.Id.ToString());

            var tasks = (await extractor.GetArticleWorks()).Select(PixivHelper.IllustrationInfo);
            var result = await Task.WhenAll(tasks);

            IllustViewer.Show(result[0], result);
        }

        private async void DownloadSpotlightItem_OnClick(object sender, RoutedEventArgs e)
        {
            var context = sender.GetDataContext<SpotlightArticle>();

            await PixivImage.DownloadSpotlight(context);
            messageQueue.Enqueue(Externally.DownloadSpotlightComplete(context));
        }

        #endregion

        #region 用户预览

        private async void UserPrevItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var (avatar, thumbnails) = GetUserPrevImageControls(sender);
            var dataContext = sender.GetDataContext<UserPreview>();

            UiHelper.SetImageSource(avatar, await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Avatar, dataContext.UserName));
            UiHelper.SetImageSource(thumbnails[0], await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnails[0], $"{dataContext.UserId}"));
            UiHelper.SetImageSource(thumbnails[1], await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnails[1], $"{dataContext.UserId}", 1));
            UiHelper.SetImageSource(thumbnails[2], await PixivImage.GetAndCreateOrLoadFromCacheInternal(dataContext.Thumbnails[2], $"{dataContext.UserId}", 2));
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

            await PixivImage.DownloadIllustInternal(illust);
            messageQueue.Enqueue(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            await PixivImage.DownloadIllustsInternal(DownloadList.ToDownloadList.ToList());
            messageQueue.Enqueue(Externally.AllDownloadComplete);
        }

        private void ClearDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DownloadList.ToDownloadList.Any())
            {
                DownloadList.ToDownloadList.Clear();
                messageQueue.Enqueue(Externally.ClearedDownloadList);
            }
        }

        #endregion

        #region 右键菜单

        private async void DownloadNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var illust = sender.GetDataContext<Illustration>();

            await PixivImage.DownloadIllustInternal(illust);
            messageQueue.Enqueue(Externally.DownloadComplete(illust));
        }

        private async void DownloadAllNowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            await PixivImage.DownloadIllustsInternal(((IEnumerable<Illustration>) ImageListView.ItemsSource).ToList());
        }

        private void AddToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.ToDownloadList.Add(sender.GetDataContext<Illustration>());
            messageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }


        private void AddAllToDownloadListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.ToDownloadList.AddRange(((IEnumerable<Illustration>)ImageListView.ItemsSource).ToList());
            messageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }

        #endregion

        #region 工具
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

        private static async void Query(IPixivIterator pixivIterator, Collection<Illustration> container, bool sync)
        {
            var counter = 1;
            while (pixivIterator.HasNext() && counter <= Settings.Global.QueryPages)
            {
                if (sync)
                    foreach (var illust in await pixivIterator.MoveNextAsync().ToListAsync())
                        AddToItemSource(illust, container);
                else
                    await foreach (var illust in pixivIterator.MoveNextAsync())
                        AddToItemSource(illust, container);
                counter++;
            }
        }

        private void NoticeProgress(int? pages = null)
        {
            ProgressDialog.IsOpen = true;
            if (pages != null) ProgressNoticeTextBlock.Text = $"正在为您查找第{Settings.Global.QueryStart}到第{Settings.Global.QueryPages + Settings.Global.QueryStart - 1}页, 您所查找的关键字总共有{pages}页";
        }

        private void ProgressDialog_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            ProgressNoticeTextBlock.Text = string.Empty;
        }

        private static void AddToItemSource(Illustration illust, Collection<Illustration> container)
        {
            if (IllustNotMatchCondition(Settings.Global.ExceptTags, Settings.Global.ContainsTags, illust))
                return;

            if (Settings.Global.SortOnInserting)
                container.AddSorted(illust, IllustrationComparator.Instance);
            else
                container.Add(illust);
        }

        private static bool IllustNotMatchCondition(ISet<string> exceptTag, ISet<string> containsTag, Illustration illustration)
        {
            return !exceptTag.IsNullOrEmpty() && exceptTag.Any(x => !x.IsNullOrEmpty() && illustration.Tags.Any(i => i.EqualsIgnoreCase(x))) ||
                   !containsTag.IsNullOrEmpty() && containsTag.Any(x => !x.IsNullOrEmpty() && !illustration.Tags.Any(i => i.EqualsIgnoreCase(x))) ||
                   illustration.Bookmark < Settings.Global.MinBookmark;
        }

        private void Notice(string message)
        {
            ProgressDialog.IsOpen = false;
            NoticeDialog.IsOpen = true;
            NoticeMessage.Text = message;
        }

        #endregion

        private void IllustrationContainer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IllustViewer.Show(sender.GetDataContext<Illustration>(), ImageListView.ItemsSource as IEnumerable<Illustration>);
        }
    }
}