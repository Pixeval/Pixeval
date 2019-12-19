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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;

namespace Pixeval
{
    public partial class IllustViewer
    {
        private readonly IList<Illustration> displayList;

        private readonly SnackbarMessageQueue messageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(500))
        {
            IgnoreDuplicate = true
        };

        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private int currentIndex;

        private Illustration currentModel;

        private bool dragging;

        private Point dragPos;

        private bool gifPlaying;

        public IllustViewer(Illustration defaultIllust, IEnumerable<Illustration> displayList = null)
        {
            this.displayList = displayList == null ? new List<Illustration> {defaultIllust} : displayList.ToList();

            currentModel = defaultIllust;
            currentIndex = this.displayList.IndexOf(currentModel);

            if (currentIndex == -1) throw new InvalidOperationException();

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
            cancellationToken.Cancel();
            cancellationToken = new CancellationTokenSource();

            SwitchDataContext(currentModel);
            gifPlaying = false;

            UiHelper.ShowControl(ProgressRing);
            IllustFadeIn();

            UiHelper.ReleaseImage(DisplayIllustration);

            SetTags();
            SetIllustratorAvatar(currentModel.UserId);

            if (currentModel.IsUgoira)
            {
                gifPlaying = true;

                var data = await HttpClientFactory.AppApiService.GetUgoiraMetadata(currentModel.Id);
                var url = PixivEx.FormatGifZipUrl(data.UgoiraMetadataInfo.ZipUrls.Medium);

                if (gifPlaying) PlayGif(await PixivEx.ReadGifZipBitmapImages(await PixivEx.FromUrlInternal(url)).ToListAsync(cancellationToken.Token), data.UgoiraMetadataInfo.Frames.Select(f => f.Delay));
            }
            else
            {
                UiHelper.SetImageSource(DisplayIllustration, await PixivEx.GetAndCreateOrLoadFromCache(false, currentModel.Origin, currentModel.Id, cancellationToken: cancellationToken.Token));
            }

            IllustFadeOut();
            UiHelper.HideControl(ProgressRing);
        }

        private void PlayGif(IList<BitmapImage> images, IEnumerable<long> delay)
        {
            var delayArr = delay.ToArray();
            Task.Run(async () =>
            {
                while (true)
                    for (var i = 0; i < images.Count; i++)
                    {
                        var i1 = i;
                        Dispatcher?.Invoke(() => UiHelper.SetImageSource(DisplayIllustration, images[i1]));
                        await Task.Delay((int) delayArr[i]);

                        if (!gifPlaying)
                            return;
                    }
            });
        }

        private async void SetIllustratorAvatar(string id)
        {
            var userInfo = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest {Id = id});
            UiHelper.SetImageSource(IllustratorAvatar, await PixivEx.GetAndCreateOrLoadFromCacheInternal(userInfo.UserEntity.ProfileImageUrls.Medium, $"{userInfo.UserEntity.Id}_avatar"));
        }

        private void SetTags()
        {
            var collection = UiHelper.NewItemsSource<string>(TagsListView);
            collection.AddRange(currentModel.Tags);
        }

        private void IllustFadeIn()
        {
            this.GetResources<Storyboard>("ImageFadeIn").Begin();
        }

        private void IllustFadeOut()
        {
            this.GetResources<Storyboard>("ImageFadeOut").Begin();
        }

        private void SwitchDataContext(Illustration model)
        {
            DataContext = model;
        }

        private void ImageContainer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
            if (DisplayIllustration != null) dragPos = e.GetPosition(ImageContainer);
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
            ResetImagePlacement();
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
            if (DisplayIllustration != null)
                if (!Canvas.GetLeft(DisplayIllustration).Equals(0) || !Canvas.GetTop(DisplayIllustration).Equals(0))
                {
                    Canvas.SetLeft(DisplayIllustration, 0);
                    Canvas.SetTop(DisplayIllustration, 0);
                }
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

            DownloadList.Remove(model);
            await PixivEx.DownloadIllustInternal(model);
            messageQueue.Enqueue(Externally.DownloadComplete(model));
        }

        private void TagItem_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.NavigatorList.SelectedItem = MainWindow.Instance.MenuTab;
            MainWindow.Instance.KeywordTextBox.Text = sender.GetDataContext<string>();

            MainWindow.Instance.Activate();
        }

        private void IllustratorAvatar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserViewer.Show(currentModel.UserId);
        }

        private void ViewInBrowserButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://www.pixiv.net/artworks/{currentModel.Id}") {CreateNoWindow = true});
        }
    }
}