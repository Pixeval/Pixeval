// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Objects;

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
            ((INotifyCollectionChanged) DownloadItemsQueue.Items).CollectionChanged += (sender, args) =>
                EmptyNotifier.Visibility = DownloadItemsQueue.Items.Count == 0 ? Visibility.Visible : Visibility.Hidden;
            UiHelper.SetItemsSource(DownloadItemsQueue, AppContext.Downloading);
        }

        private async void DownloadItemThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            var url = sender.GetDataContext<DownloadableIllustrationViewModel>().DownloadContent.Thumbnail;
            UiHelper.SetImageSource(sender, await PixivIO.FromUrl(url));
        }

        private void RetryButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var model = sender.GetDataContext<DownloadableIllustrationViewModel>();
            model.Restart();
        }
    }
}