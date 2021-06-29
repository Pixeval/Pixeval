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

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Dragablz;
using Pixeval.Core;
using Pixeval.Core.Persistent;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for DownloadQueue.xaml
    /// </summary>
    public partial class DownloadQueue
    {
        public DownloadQueue()
        {
            InitializeComponent();
            UiHelper.SetItemsSource(DownloadItemsQueue, DownloadManager.Downloading);
            UiHelper.SetItemsSource(DownloadedItemsQueue, DownloadManager.Downloaded);
            UiHelper.SetItemsSource(FavoriteSpotlightsQueue, FavoriteSpotlightAccessor.GlobalLifeTimeScope.Get());
        }

        private void DownloadItemThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var url = sender.GetDataContext<DownloadableIllustration>().DownloadContent.Thumbnail;
            Task.Run(async () => UiHelper.SetImageSource(sender, await PixivIO.FromUrl(url)));
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
            if (!model.Path.IsNullOrEmpty() && Path.GetDirectoryName(model.Path) != string.Empty)
            {
                var processInfo = new ProcessStartInfo { FileName = "explorer", Arguments = $"/select, \"{model.Path}\"" };
                Process.Start(processInfo);
            }
            else
            {
                MainWindow.MessageQueue.Enqueue(Pixeval.Resources.Resources.PathNotExist);
            }
        }

        private void ShowDownloadIllustration(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.DownloadQueueDialogHost.CloseControl();
            var model = sender.GetDataContext<DownloadableIllustration>();
            MainWindow.Instance.OpenIllustBrowser(model.DownloadContent.IsManga ? model.DownloadContent.MangaMetadata[0] : model.DownloadContent);
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
            var img = (Image) sender;
            if (browsing.IsReferToUser)
            {
                img.Effect = new BlurEffect { KernelType = KernelType.Gaussian, Radius = 50, RenderingBias = RenderingBias.Quality };
            }
            UiHelper.SetImageSource(img, await PixivIO.FromUrl(browsing.BrowseObjectThumbnail));
        }

        private async void BrowsingHistoryAvatarImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var browsing = sender.GetDataContext<BrowsingHistory>();
            if (browsing.IsReferToUser)
            {
                UiHelper.SetImageSource(sender, await PixivIO.FromUrl(browsing.BrowseObjectThumbnail));
            }
        }

        private async void BrowsingHistoryMainImage_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.DownloadQueueDialogHost.CurrentSession.Close();
            var ctx = sender.GetDataContext<BrowsingHistory>();
            switch (ctx.Type)
            {
                case "spotlight":
                    MainWindow.MessageQueue.Enqueue(Pixeval.Resources.Resources.SearchingSpotlight);

                    var tasks = await Tasks<string, Illustration>.Of(await PixivClient.GetArticleWorks(ctx.BrowseObjectId)).Mapping(PixivHelper.IllustrationInfo).Construct().WhenAll();
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
            MainWindow.Instance.SetUserBrowserContext(new User { Id = sender.GetDataContext<BrowsingHistory>().BrowseObjectId });
        }

        private void CopyDownloadPathMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(sender.GetDataContext<DownloadableIllustration>().Path);
        }

        private void RemoveFromListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            (DownloadQueueTabControl.SelectedItem switch
            {
                var x when x.Equals(BrowsingHistoryTab)   => BrowsingHistoryAccessor.GlobalLifeTimeScope.Get(),
                var x when x.Equals(FavoriteSpotlightTab) => FavoriteSpotlightAccessor.GlobalLifeTimeScope.Get(),
                _                                         => null
            })?.Apply(list => list.Remove(sender.GetDataContext<BrowsingHistory>()));
        }

        private void RemoveAllFromListMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            (DownloadQueueTabControl.SelectedItem switch
            {
                var x when x.Equals(BrowsingHistoryTab)   => BrowsingHistoryAccessor.GlobalLifeTimeScope.Get(),
                var x when x.Equals(FavoriteSpotlightTab) => FavoriteSpotlightAccessor.GlobalLifeTimeScope.Get(),
                _                                         => null
            })?.Apply(list => list.Clear());
        }

        private void DownloadQueueTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.OriginalSource.Equals(DownloadQueueTabControl)) // tab control's SelectionChanged event will fired when it's elements' SelectionChanged event is triggered
            {
                return;
            }
            
            if (sender is TabablzControl tabControl)
            {
                switch (tabControl.SelectedItem)
                {
                    case var x when x.Equals(BrowsingHistoryTab):
                        RefreshBrowsingHistory();
                        break;
                    case var x when x.Equals(FavoriteSpotlightTab):
                        RefreshFavoriteSpotlight();
                        break;
                }
                
            }
        }
        
        // Performance consideration
        public async void RefreshBrowsingHistory()
        {
            BrowsingHistoryQueue.ItemsSource = null;
            var collection = UiHelper.NewItemsSource<BrowsingHistory>(BrowsingHistoryQueue);
            foreach (var browsingHistory in BrowsingHistoryAccessor.GlobalLifeTimeScope.Get())
            {
                await Task.Delay(5);
                collection.Add(browsingHistory);
            }
        }

        public async void RefreshFavoriteSpotlight()
        {
            FavoriteSpotlightsQueue.ItemsSource = null;
            var collection = UiHelper.NewItemsSource<BrowsingHistory>(FavoriteSpotlightsQueue);
            foreach (var browsingHistory in FavoriteSpotlightAccessor.GlobalLifeTimeScope.Get())
            {
                await Task.Delay(5);
                collection.Add(browsingHistory);
            }
        }
    }
}