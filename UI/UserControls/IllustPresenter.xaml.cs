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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf.Transitions;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using PropertyChanged;
using static Pixeval.Objects.UiHelper;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for IllustPresenter.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class IllustPresenter
    {
        public IllustPresenter(ImageSource imgSource, Illustration illustration)
        {
            ImgSource = imgSource;
            Illust = illustration;
            InitializeComponent();
        }

        public ImageSource ImgSource { get; set; }

        public Illustration Illust { get; set; }

        private async void CopyImageItem_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(await PixivEx.FromUrl(Illust.Origin.IsNullOrEmpty() ? Illust.Large : Illust.Origin));
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
                    MainWindow.Instance.IllustBrowserDialogHost.DataContext = Illust;
                    var userInfo = await HttpClientFactory.AppApiService.GetUserInformation(new UserInformationRequest {Id = Illust.UserId});
                    SetImageSource(MainWindow.Instance.IllustBrowserUserAvatar, await PixivEx.FromUrl(userInfo.UserEntity.ProfileImageUrls.Medium));
                });
            });
        }
    }
}