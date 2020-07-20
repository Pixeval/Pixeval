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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf.Transitions;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects.Primitive;
using PropertyChanged;
using static Pixeval.Objects.Primitive.UiHelper;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for IllustPresenter.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class IllustPresenter
    {
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public IllustPresenter(Illustration illustration)
        {
            Illust = illustration;
            InitializeComponent();
            LoadThumbnail();
            LoadOrigin();
        }

        public ImageSource ImgSource { get; set; }

        public Illustration Illust { get; set; }

        public bool ProcessingGif { get; private set; }

        public bool PlayingGif { get; private set; }

        public bool LoadingOrigin { get; private set; }

        public double LoadingIndicator { get; set; }

        public bool PlayButtonVisible => !ProcessingGif && !PlayingGif;

        private async void LoadThumbnail()
        {
            var imgSource = await PixivIO.FromUrl(Illust.Thumbnail);
            if (ImgSource == null) Dispatcher.Invoke(() => ImgSource = imgSource);
        }

        private async void LoadOrigin()
        {
            LoadingOrigin = true;
            var progress = new Progress<double>(p => Dispatcher.Invoke(() => LoadingIndicator = p));
            await using var mem =
                await PixivIO.Download(Illust.GetDownloadUrl(), progress, _cancellationTokenSource.Token);
            ImgSource = InternalIO.CreateBitmapImageFromStream(mem);
            LoadingOrigin = false;
            ((BlurEffect)ContentImage.Effect).Radius = 0;
        }

        private void MovePrevButton_OnClick(object sender, RoutedEventArgs e)
        {
            Transitioner.MovePreviousCommand.Execute(null, null);
            ChangeSource();
        }

        private void MoveNextButton_OnClick(object sender, RoutedEventArgs e)
        {
            Transitioner.MoveNextCommand.Execute(null, null);
            ChangeSource();
        }

        private void ChangeSource()
        {
            Task.Run(() =>
            {
                (Dispatcher ?? throw new InvalidOperationException()).Invoke(async () =>
                {
                    var userInfo = await HttpClientFactory.AppApiService()
                        .GetUserInformation(new UserInformationRequest { Id = Illust.UserId });
                    if (Illust.UserId ==
                        MainWindow.Instance.IllustBrowserDialogHost.GetDataContext<Illustration>()
                            .UserId)
                        SetImageSource(MainWindow.Instance.IllustBrowserUserAvatar,
                                       await PixivIO.FromUrl(userInfo.UserEntity.ProfileImageUrls.Medium));
                    MainWindow.Instance.IllustBrowserDialogHost.DataContext = Illust;
                });
            });
        }

        private async void PlayGif_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessingGif = true;
            var metadata = await HttpClientFactory.AppApiService().GetUgoiraMetadata(Illust.Id);
            var ugoiraZip = metadata.UgoiraMetadataInfo.ZipUrls.Medium;
            var delay = metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToArray();
            var streams = InternalIO.ReadGifZipEntries(await PixivIO.GetBytes(ugoiraZip)).ToArray();

            ProcessingGif = false;
            PlayingGif = true;

            _ = Task.Run(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                    for (var i = 0; i < streams.Length && !_cancellationToken.IsCancellationRequested; i++)
                    {
                        streams[i].Position = 0;
                        ImgSource = InternalIO.CreateBitmapImageFromStream(streams[i]);
                        await Task.Delay((int)delay[i], _cancellationToken.Token);
                    }

                foreach (var stream in streams)
                {
                    _ = stream.DisposeAsync();
                }
            });
        }

        private void IllustPresenter_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _cancellationToken.Cancel();
            _cancellationTokenSource.Cancel();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapImage)ImgSource);
        }

        private void MovePrevButton_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContentImage.ContextMenu != null) ContentImage.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void MoveNextButton_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContentImage.ContextMenu != null) ContentImage.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}
