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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf.Transitions;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Helpers;
using Pixeval.Models;
using Pixeval.Views;
using PropertyChanged;
using static Pixeval.Objects.UiHelper;

namespace Pixeval.UserControls
{
    /// <summary>
    ///     Interaction logic for IllustPresenter.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class IllustPresenter
    {
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public IllustPresenter(ImageSource imgSource, Illustration illustration)
        {
            ImgSource = imgSource;
            Illust = illustration;
            InitializeComponent();
        }

        public ImageSource ImgSource { get; set; }

        public Illustration Illust { get; set; }

        public bool ProcessingGif { get; private set; }

        public bool PlayingGif { get; private set; }

        public bool PlayButtonVisible => !ProcessingGif && !PlayingGif;

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
                    MainWindow.Instance.IllustBrowserDialogHost.DataContext = Illust;
                    var userInfo = await HttpClientFactory.AppApiService().GetUserInformation(new UserInformationRequest {Id = Illust.UserId});
                    SetImageSource(MainWindow.Instance.IllustBrowserUserAvatar, await PixivIOHelper.FromUrl(userInfo.UserEntity.ProfileImageUrls.Medium));
                });
            });
        }

        private async void PlayGif_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessingGif = true;
            var metadata = await HttpClientFactory.AppApiService().GetUgoiraMetadata(Illust.Id);
            var ugoiraZip = metadata.UgoiraMetadataInfo.ZipUrls.Medium;
            var delay = metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToArray();
            var streams = PixivIOHelper.ReadGifZipEntries(await PixivIOHelper.FromUrlInternal(ugoiraZip)).ToArray();

            ProcessingGif = false;
            PlayingGif = true;

            #pragma warning disable 4014
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                    for (var i = 0; i < streams.Length && !cancellationToken.IsCancellationRequested; i++)
                    {
                        streams[i].Position = 0;
                        ImgSource = PixivIOHelper.FromStream(streams[i]);
                        await Task.Delay((int) delay[i], cancellationToken.Token);
                    }
            });
            #pragma warning restore 4014
        }

        private void IllustPresenter_OnUnloaded(object sender, RoutedEventArgs e)
        {
            cancellationToken.Cancel();
        }
    }
}