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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Data.Model.ViewModel;
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
        private IPixivIterator currentIterator;

        public MainWindow()
        {
            InitializeComponent();
            NavigatorList.SelectedItem = MenuTab;

            if (Dispatcher != null) Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
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

        private void SettingDialog_OnDialogClosing(object sender, DialogClosingEventArgs e) => SettingsTab.IsSelected = false;

        private async void DoQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (KeywordTextBox.Text.IsNullOrEmpty())
            {
                Notice("请在输入搜索关键字后再进行搜索~");
                return;
            }

            MenuTab.IsSelected = false;

            terminate = false;
            HomeContainerMoveDown();

            var pages = await PixivHelper.GetQueryPagesCount(KeywordTextBox.Text);
            NoticeProgress(pages);

            currentIterator = new QueryIterator(KeywordTextBox.Text, pages, Settings.Global.QueryStart);
            
            var container = new ObservableCollection<Illustration>();
            ImageListView.ItemsSource = container;

            Query(currentIterator, container);
        }

        private void Query(IPixivIterator pixivIterator, Collection<Illustration> container)
        {
            var counter = 1;
            while (pixivIterator.HasNext() && counter++ < Settings.Global.QueryPages && !terminate)
            {
                AddToItemSource(pixivIterator.MoveNextAsync(), container);
            }
        }

        private void NoticeProgress(int pages)
        {
            ProgressDialog.IsOpen = true;
            ProgressNoticeTextBlock.Text = $"正在为您查找第{Settings.Global.QueryStart}到第{Settings.Global.QueryPages + Settings.Global.QueryStart}页, 您所查找的关键字总共有{pages}页";
        }

        private async void AddToItemSource(IAsyncEnumerable<Illustration> illustrations, Collection<Illustration> container)
        { 
            var exceptTag = Settings.Global.ExceptTags.ToImmutableSet(p => p.ToLower());
            var containsTag = Settings.Global.ContainsTags.ToImmutableSet(p => p.ToLower());

            try
            {
                await foreach (var illust in illustrations)
                {
                    if (terminate)
                    {
                        TerminateTask();
                        ProgressDialog.IsOpen = false;
                        break;
                    }

                    var tags = illust.Tags.Select(p => p.ToLower());
                    if (TagsNotMatchCondition(exceptTag, containsTag, tags))
                        continue;

                    if (Settings.Global.SortOnInserting)
                        container.AddSorted(illust, IllustrationComparator.Instance);
                    else
                        container.Add(illust);
                }
            }
            catch (QueryNotRespondingException)
            {
                ProgressDialog.IsOpen = false;
                Notice("抱歉, Pixeval无法根据当前的设置找到任何作品, 或许是您的页码设置过大/关键字不存在, 换个关键字或者调整页码再试一次吧~");
                MenuTab.IsSelected = true;
                TerminateTask();
            }
        }

        private bool terminate;

        private void TerminateTask()
        {
            ImageListView.ItemsSource = null;
            terminate = false;
        }

        private static bool TagsNotMatchCondition(IImmutableSet<string> exceptTag, IImmutableSet<string> containsTag, IEnumerable<string> tags)
        {
            return exceptTag.Any() && exceptTag.Any(x => !x.IsNullOrEmpty() && tags.Contains(x)) ||
                containsTag.Any() && containsTag.Any(x => !x.IsNullOrEmpty() && !tags.Contains(x));
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
            if (!e.AddedItems.Contains(MenuTab))
            {
                TerminateTask();
            }
            terminate = true;
            ScheduleHomePage();
        }

        private void ScheduleHomePage()
        {
            if (NavigatorList.SelectedItem is ListViewItem current)
            {
                var translateTransform = (TranslateTransform) HomeDisplayContainer.RenderTransform;
                if (current == MenuTab && !translateTransform.Y.Equals(0))
                {
                    HomeContainerMoveUp();
                }
                else if (current != MenuTab && translateTransform.Y.Equals(0))
                {
                    HomeContainerMoveDown();
                }
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
            TerminateTask();
        }

        private void OpenFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog("选择存储位置")
            {
                InitialDirectory = Settings.Global.DownloadLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DownloadLocationTextBox.Text = fileDialog.FileName;
            }
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
            ((Image) sender).Source = null;
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            Settings.Global.ExceptTags = new HashSet<string>(new []{ "R-18" , "R-18G" });
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

        private void Notice(string message)
        {
            NoticeDialog.IsOpen = true;
            NoticeMessage.Text = message;
        }
    }
}
