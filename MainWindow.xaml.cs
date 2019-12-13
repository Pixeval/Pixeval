using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Xceed.Wpf.AvalonDock.Controls;

namespace Pixeval
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigatorList.SelectedItem = MenuTab;

            UiHelper.SetItemsSource(ToDownloadListView, Data.Model.ViewModel.DownloadList.ToDownloadList);
            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private async void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                Notice("请在输入搜索关键字后再进行搜索~");
                return;
            }

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
            if (e.Exception is QueryNotRespondingException)
            {
                ImageListView.ItemsSource = null;
                Notice("抱歉, Pixeval无法根据当前的设置找到任何作品, 或许是您的页码设置过大/关键字不存在/没有收藏任何作品, 请检查后再尝试吧~");
            }
            else
            {
                Notice(e.Exception.Message);
            }

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
            ToLoseFocus.Focus();
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
            var imgViewer = new IllustViewer(sender.GetDataContext<Illustration>(), ImageListView.ItemsSource as IEnumerable<Illustration>);
            imgViewer.Show();
        }

        private void RemoveFromDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            Data.Model.ViewModel.DownloadList.Remove(sender.GetDataContext<Illustration>());
        }

        private void ToDownloadListViewItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewer = new IllustViewer(sender.GetDataContext<Illustration>());
            viewer.Show();
        }
    }
}