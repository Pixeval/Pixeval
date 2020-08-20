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

using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Pixeval.Wpf.Core;
using Pixeval.Wpf.Objects.Generic;
using Pixeval.Wpf.Objects.I18n;
using Pixeval.Wpf.Objects.Primitive;
using Pixeval.Wpf.View;
using Pixeval.Wpf.ViewModel;

namespace Pixeval.Wpf.UserControls
{
    /// <summary>
    ///     Interaction logic for DownloadQueue.xaml
    /// </summary>
    public partial class DownloadQueue
    {
        public DownloadQueue()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)DownloadItemsQueue.Items).CollectionChanged += (sender, args) =>
               EmptyNotifier1.Visibility =
                   DownloadItemsQueue.Items.Count == 0 ? Visibility.Visible : Visibility.Hidden;
            ((INotifyCollectionChanged)DownloadedItemsQueue.Items).CollectionChanged += (sender, args) =>
               EmptyNotifier2.Visibility =
                   DownloadedItemsQueue.Items.Count == 0 ? Visibility.Visible : Visibility.Hidden;
            ((INotifyCollectionChanged)BrowsingHistoryQueue.Items).CollectionChanged += (sender, args) =>
               EmptyNotifier3.Visibility =
                   BrowsingHistoryQueue.Items.Count == 0 ? Visibility.Visible : Visibility.Hidden;
            UiHelper.SetItemsSource(DownloadItemsQueue, DownloadManager.Downloading);
            UiHelper.SetItemsSource(DownloadedItemsQueue, DownloadManager.Downloaded);
            UiHelper.SetItemsSource(BrowsingHistoryQueue, BrowsingHistoryAccessor.GlobalLifeTimeScope.Get());
        }

        private async void DownloadItemThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var url = sender.GetDataContext<DownloadableIllustration>().DownloadContent.Thumbnail;
            UiHelper.SetImageSource(sender, await PixivIO.FromUrl(url));
        }

        private void RetryButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var model = sender.GetDataContext<DownloadableIllustration>();
            model.Restart();
        }

        private void CancelButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var model = sender.GetDataContext<DownloadableIllustration>();
            model.Cancel();
        }

        private void ViewDownloadLocationButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var model = sender.GetDataContext<DownloadableIllustration>();
            if (!model.GetPath().IsNullOrEmpty() && Path.GetDirectoryName(model.GetPath()) is var _)
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"/select, \"{model.GetPath()}\""
                };
                Process.Start(processInfo);
            }
            else
            {
                MainWindow.MessageQueue.Enqueue(AkaI18N.PathNotExist);
            }
        }

        private void ShowDownloadIllustration(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.DownloadQueueDialogHost.CloseControl();
            var model = sender.GetDataContext<DownloadableIllustration>();
            MainWindow.Instance.OpenIllustBrowser(model.IsFromManga
                                                      ? model.DownloadContent.MangaMetadata[0]
                                                      : model.DownloadContent);
        }

        private void RemoveFromDownloaded(object sender, RoutedEventArgs e)
        {
            DownloadManager.Downloaded.Remove(sender.GetDataContext<DownloadableIllustration>());
        }

        private void RemoveFromDownloading(object sender, RoutedEventArgs e)
        {
            DownloadManager.Downloading.Remove(sender.GetDataContext<DownloadableIllustration>());
        }

        private async void BrowsingHistoryMainImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var browsing = sender.GetDataContext<BrowsingHistory>();
            var img = (Image)sender;
            if (browsing.IsReferToUser)
                img.Effect = new BlurEffect
                {
                    KernelType = KernelType.Gaussian,
                    Radius = 50,
                    RenderingBias = RenderingBias.Quality
                };
            UiHelper.SetImageSource(img, await PixivIO.FromUrl(browsing.BrowseObjectThumbnail));
        }

        private async void BrowsingHistoryAvatarImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var browsing = sender.GetDataContext<BrowsingHistory>();
            if (browsing.IsReferToUser) UiHelper.SetImageSource(sender, await PixivIO.FromUrl(browsing.BrowseObjectThumbnail));
        }

        private async void BrowsingHistoryMainImage_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.DownloadQueueDialogHost.CurrentSession.Close();
            var ctx = sender.GetDataContext<BrowsingHistory>();
            switch (ctx.Type)
            {
                case "spotlight":
                    MainWindow.MessageQueue.Enqueue(AkaI18N.SearchingSpotlight);

                    var tasks = await Tasks<string, Illustration>
                        .Of(await PixivClient.Instance.GetArticleWorks(ctx.BrowseObjectId))
                        .Mapping(PixivHelper.IllustrationInfo)
                        .Construct()
                        .WhenAll();
                    var result = tasks.Peek(i =>
                    {
                        i.IsManga = true;
                        i.FromSpotlight = true;
                        i.SpotlightTitle = ctx.BrowseObjectState;
                    }).ToArray();

                    PixivHelper.RecordTimelineInternal(new BrowsingHistory
                    {
                        BrowseObjectId = ctx.BrowseObjectId,
                        BrowseObjectState = ctx.BrowseObjectState,
                        BrowseObjectThumbnail = ctx.BrowseObjectThumbnail,
                        IsReferToSpotlight = true,
                        Type = "spotlight"
                    });

                    MainWindow.Instance.OpenIllustBrowser(result[0].Apply(r => r.MangaMetadata = result.ToArray()));
                    break;
                case "illust":
                    MainWindow.Instance.OpenIllustBrowser(await PixivHelper.IllustrationInfo(ctx.BrowseObjectId));
                    break;
            }
        }

        private void ReferUser_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.DownloadQueueDialogHost.CurrentSession.Close();
            MainWindow.Instance.OpenUserBrowser();
            MainWindow.Instance.SetUserBrowserContext(new User
            {
                Id = sender.GetDataContext<BrowsingHistory>().BrowseObjectId
            });
        }
    }
}
