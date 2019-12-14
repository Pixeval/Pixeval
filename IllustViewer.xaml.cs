using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Pixeval.Core;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;

namespace Pixeval
{
    /// <summary>
    /// Interaction logic for IllustViewer.xaml
    /// </summary>
    public partial class IllustViewer
    {
        private readonly IList<Illustration> displayList;

        private readonly SnackbarMessageQueue messageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(500))
        {
            IgnoreDuplicate = true
        };

        private Illustration currentModel;

        private int currentIndex;

        public IllustViewer(Illustration defaultIllust, IEnumerable<Illustration> displayList = null)
        {
            this.displayList = displayList == null ? new List<Illustration> { defaultIllust } : displayList.ToList();

            currentModel = defaultIllust;
            currentIndex = this.displayList.IndexOf(currentModel);

            if (currentIndex == -1)
            {
                throw new InvalidOperationException();
            }

            InitializeComponent();

            RefreshIllust();
            IllustViewerSnackBar.MessageQueue = messageQueue;
        }

        public static void Show(Illustration defaultIllust, IEnumerable<Illustration> displayList = null)
        {
            new IllustViewer(defaultIllust, displayList).Show();
        }

        private async void RefreshIllust()
        {
            SwitchDataContext(currentModel);
            gifPlaying = false;

            UiHelper.ShowControl(ProgressRing);
            IllustFadeIn();

            UiHelper.ReleaseImage(DisplayIllustration);

            if (currentModel.IsUgoira)
            {
                gifPlaying = true;

                var data = await HttpClientFactory.AppApiService.GetUgoiraMetadata(currentModel.Id);
                var url = PixivImage.FormatGifZipUrl(data.UgoiraMetadataInfo.ZipUrls.Medium);

                var list = await PixivImage.ReadGifZipBitmapImages(await PixivImage.FromUrlInternal(url)).ToListAsync();

                if (gifPlaying)
                {
                    PlayGif(list, data.UgoiraMetadataInfo.Frames.Select(f => f.Delay));
                }
            }
            else
            {
                UiHelper.SetImageSource(DisplayIllustration, await PixivImage.GetAndCreateOrLoadFromCache(false, currentModel.Origin, currentModel.Id));
            }

            IllustFadeOut();
            UiHelper.HideControl(ProgressRing);
            
            SetTags();
            SetIllustratorAvatar(currentModel.UserId);
        }

        private bool gifPlaying;

        private void PlayGif(IList<BitmapImage> images, IEnumerable<long> delay)
        {
            var delayArr = delay.ToArray();
            Task.Run(async () =>
            {
                while (true)
                {
                    for (var i = 0; i < images.Count; i++)
                    {
                        var i1 = i;
                        Dispatcher?.Invoke(() => UiHelper.SetImageSource(DisplayIllustration, images[i1]));
                        await Task.Delay((int) delayArr[i]);

                        if (!gifPlaying)
                            return;
                    }
                }
            });
        }

        private async void SetIllustratorAvatar(string id)
        {
            var userInfo = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest {Id = id});
            UiHelper.SetImageSource(IllustratorAvatar, await PixivImage.GetAndCreateOrLoadFromCacheInternal(userInfo.UserEntity.ProfileImageUrls.Medium, $"{userInfo.UserEntity.Id}_avatar"));
        }

        private void SetTags()
        {
            var collection = UiHelper.NewItemsSource<string>(TagsListView);
            collection.AddRange(currentModel.Tags);
        }

        private void IllustFadeIn()
        {
            (Resources["ImageFadeIn"] as Storyboard)?.Begin();
        }

        private void IllustFadeOut()
        {
            (Resources["ImageFadeOut"] as Storyboard)?.Begin();
        }

        private void SwitchDataContext(Illustration model)
        {
            DataContext = model;
        }

        private bool dragging;

        private Point dragPos;

        private void ImageContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
            if (DisplayIllustration != null)
            {
                dragPos = e.GetPosition(ImageContainer);
            }
        }

        private void ImageContainer_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (DisplayIllustration != null && dragging)
            {
                var p = e.GetPosition(ImageContainer);
                var off = p - dragPos;

                dragPos = p;
                Canvas.SetLeft(DisplayIllustration, Canvas.GetLeft(DisplayIllustration) + off.X);
                Canvas.SetTop(DisplayIllustration, Canvas.GetTop(DisplayIllustration) + off.Y);
            }
        }

        private void ImageContainer_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
        }

        private void ImageContainer_OnMouseLeave(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void ScaleSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DisplayIllustration != null)
            {
                if (!Canvas.GetLeft(DisplayIllustration).Equals(0) || !Canvas.GetTop(DisplayIllustration).Equals(0))
                {
                    Canvas.SetLeft(DisplayIllustration, 0);
                    Canvas.SetTop(DisplayIllustration, 0);
                }
            }
        }

        private void Prev_OnClick(object sender, RoutedEventArgs e)
        {
            ResetImagePlacement();

            var toDisplayIndex = currentIndex - 1;
            if (toDisplayIndex >= 0)
            {
                currentModel = displayList[--currentIndex];
                RefreshIllust();
            }
        }

        private void Next_OnClick(object sender, RoutedEventArgs e)
        {
            ResetImagePlacement();

            var toDisplayIndex = currentIndex + 1;
            if (toDisplayIndex <= displayList.Count - 1)
            {
                currentModel = displayList[++currentIndex];
                RefreshIllust();
            }
        }

        private void FavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.PostFavoriteAsync(currentModel);
        }

        private void RemoveFavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            PixivClient.Instance.RemoveFavoriteAsync(currentModel);
        }

        private void ResetImagePlacement()
        {
            (Resources["ResetImagePlacement"] as Storyboard)?.Begin();
        }

        private void IllustViewer_OnDeactivated(object sender, EventArgs e)
        {
            SliderPopUp.IsOpen = false;
        }

        private void ScaleButton_OnClick(object sender, RoutedEventArgs e)
        {
            SliderPopUp.IsOpen = true;
        }

        private void ViewAllContentButton_OnClick(object sender, RoutedEventArgs e)
        {
            var list = currentModel.MangaMetadata;
            Show(list[0], list);
        }

        private void AddToDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadList.Add(currentModel);
            messageQueue.Enqueue(Externally.AddedAllToDownloadList);
        }

        private async void DownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var model = currentModel;

            await PixivImage.DownloadIllustInternal(model);
            messageQueue.Enqueue(Externally.DownloadComplete(model));
        }

        private void TagItem_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.NavigatorList.SelectedItem = MainWindow.Instance.MenuTab;
            MainWindow.Instance.KeywordTextBox.Text = sender.GetDataContext<string>();

            MainWindow.Instance.Activate();
        }
    }
}
