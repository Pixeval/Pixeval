using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Pixeval.Persisting;

namespace Pixeval
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigatorList.SelectedItem = MenuTab;

            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

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

            e.Handled = true;
        }

        private async void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            await AddUserNameAndAvatar();
            await HttpClientFactory.AppApiService.GetUserNav("bison倉鼠", 5000);
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

        private void SettingDialog_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            SettingsTab.IsSelected = false;
        }

        private async void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                Notice("请在输入搜索关键字后再进行搜索~");
                return;
            }

            StartUp();

            var pages = await PixivHelper.GetQueryPagesCount(KeywordTextBox.Text);

            NoticeProgress(pages);

            var container = new ObservableCollection<Illustration>();
            ImageListView.ItemsSource = container;

            Query(new QueryIterator(KeywordTextBox.Text, pages, Settings.Global.QueryStart), container, false);
        }

        private void MenuTab_OnSelected(object sender, RoutedEventArgs e)
        {
            ImageListView.ItemsSource = null;
        }

        private void GalleryTab_OnSelected(object sender, RoutedEventArgs e)
        {
            StartUp();

            var container = new ObservableCollection<Illustration>();
            ImageListView.ItemsSource = container;

            NoticeProgress();

            Query(new GalleryIterator(Identity.Global.Id), container, true);
        }

        private void RankingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            StartUp();

            var container = new ObservableCollection<Illustration>();
            ImageListView.ItemsSource = container;

            NoticeProgress();

            Query(new RankingIterator(), container, true);
        }

        private async void SpotlightTab_OnSelected(object sender, RoutedEventArgs e)
        {
            StartUp();

            var lst = new ObservableCollection<SpotlightArticle>();

            SpotlightListView.ItemsSource = lst;

            var iterator = new SpotlightQueryIterator(Settings.Global.SpotlightQueryStart, Settings.Global.QueryPages);
            await foreach (var article in iterator.GetSpotlight())
            {
                lst.Add(article);
            }
        }

        private void SpotlightTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            SpotlightListView.ItemsSource = null;
        }

        private async void FollowingTab_OnSelected(object sender, RoutedEventArgs e)
        {
            StartUp();

            var container = new ObservableCollection<UserPreview>();
            UserPreviewListView.ItemsSource = container;

            NoticeProgress();

            var iterator = new UserFollowingIterator(Identity.Global.Id);

            await foreach (var preview in iterator.GetUserPreviews())
            {
                container.Add(preview);
            }
        }

        private void FollowingTab_OnUnselected(object sender, RoutedEventArgs e)
        {
            UserPreviewListView.ItemsSource = null;
        }

        private void StartUp()
        {
            MenuTab.IsSelected = false;
            HomeContainerMoveDown();
        }

        private void NoticeProgress(int? pages = null)
        {
            ProgressDialog.IsOpen = true;
            if (pages != null)
            {
                ProgressNoticeTextBlock.Text = $"正在为您查找第{Settings.Global.QueryStart}到第{Settings.Global.QueryPages + Settings.Global.QueryStart - 1}页, 您所查找的关键字总共有{pages}页";
            }
        }

        private static async void Query(IPixivIterator pixivIterator, Collection<Illustration> container, bool sync)
        {
            var counter = 1;
            while (pixivIterator.HasNext() && counter <= Settings.Global.QueryPages)
            {
                if (sync)
                {
                    foreach (var illust in await pixivIterator.MoveNextAsync().ToListAsync())
                    {
                        AddToItemSource(illust, container);
                    }
                }
                else
                {
                    await foreach (var illust in pixivIterator.MoveNextAsync())
                    {
                        AddToItemSource(illust, container);
                    }
                }
                counter++;
            }
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

        private void Notice(string message)
        {
            ProgressDialog.IsOpen = false;
            NoticeDialog.IsOpen = true;
            NoticeMessage.Text = message;
            MenuTab.IsSelected = true;
        }

        private static bool IllustNotMatchCondition(ISet<string> exceptTag, ISet<string> containsTag, Illustration illustration)
        {
            var tags = illustration.Tags.Select(p => p.ToLower());
            return !exceptTag.IsNullOrEmpty() && exceptTag.Any(x => !x.IsNullOrEmpty() && tags.Contains(x)) ||
                   !containsTag.IsNullOrEmpty() && containsTag.Any(x => !x.IsNullOrEmpty() && !tags.Contains(x)) ||
                   illustration.Bookmark < Settings.Global.MinBookmark;

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

        private async void Thumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var image = (Image) sender;

            var dataContext = image.DataContext<Illustration>();

            if (Uri.IsWellFormedUriString(dataContext.Thumbnail, UriKind.Absolute))
            {
                var bmp = await PixivImage.GetAndCreateOrLoadFromCache(Settings.Global.CachingThumbnail, dataContext.Thumbnail, dataContext.Id);

                image.Source = bmp;
            }

            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            Storyboard.SetTarget(doubleAnimation, image);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Image.Opacity)"));
            sb.Children.Add(doubleAnimation);
            sb.Begin();
        }

        private void Thumbnail_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage((Image) sender);
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            Settings.Global.ExceptTags = new HashSet<string>(new[] {"R-18", "R-18G"});
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

        private async void FavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var context = ((Button) sender).DataContext<Illustration>();
            await PixivClient.Instance.PostFavoriteAsync(context);
        }

        private async void RemoveFavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var context = ((Button)sender).DataContext<Illustration>();
            await PixivClient.Instance.RemoveFavoriteAsync(context);
        }

        private async void SpotlightThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var img = (Image) sender;
            var dataContext = img.DataContext<SpotlightArticle>();

            img.Source = await PixivImage.GetAndCreateOrLoadFromCache(Settings.Global.CachingThumbnail, dataContext.Thumbnail, dataContext.Id.ToString());
        }

        private void SpotlightThumbnail_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage((Image) sender);
        }

        #region 大把废话

        private async void UserPrevItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var images = ((ListViewItem) sender).FindChild<Grid>().Children[1].GetChildObjects().Cast<Image>().ToList();
            var dataContext = ((ListViewItem) sender).DataContext<UserPreview>();

            for (var index = 0; index < images.Count; index++)
            {
                var image = images[index];
                image.Source = await PixivImage.GetAndCreateOrLoadFromCache(Settings.Global.CachingThumbnail, dataContext.Thumbnails[0], $"{dataContext.UserId}_{index}");
            }
        }

        private async void UserAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var image = ((Image) sender);
            var dataContext = image.DataContext<UserPreview>();

            image.Source = await PixivImage.GetAndCreateOrLoadFromCache(Settings.Global.CachingThumbnail, dataContext.Avatar, dataContext.UserName);
        }

        private void UserAvatar_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage((Image) sender);
        }

        private void UserPrevImage1_OnLoaded(object sender, RoutedEventArgs e)
        {
            var control = (Image) sender;
            LoadImageToPrevShowcase(control, control.DataContext<UserPreview>(), 0);
        }

        private void UserPrevImage1_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage((Image) sender);
        }

        private void UserPrevImage2_OnLoaded(object sender, RoutedEventArgs e)
        {
            var control = (Image) sender;
            LoadImageToPrevShowcase(control, control.DataContext<UserPreview>(), 1);
        }

        private void UserPrevImage2_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage((Image) sender);
        }

        private void UserPrevImage3_OnLoaded(object sender, RoutedEventArgs e)
        {
            var control = (Image) sender;
            LoadImageToPrevShowcase(control, control.DataContext<UserPreview>(), 2);
        }

        private void UserPrevImage3_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReleaseImage((Image) sender);
        }

        private static async void LoadImageToPrevShowcase(Image img, UserPreview viewModel, int index)
        {
            if (viewModel.Thumbnails.Length >= index + 1)
            {
                img.Source = await PixivImage.GetAndCreateOrLoadFromCache(Settings.Global.CachingThumbnail, viewModel.Thumbnails[index], $"{viewModel.UserId}_{index}");
            }
        }

        #endregion

        private static void ReleaseImage(Image img)
        {
            img.Source = null;
        }
    }
}