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

using System.Windows;
using System.Windows.Media.Imaging;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects.Primitive;

namespace Pixeval.UI.UserControls
{
    public partial class UserPreviewPopupContent
    {
        public UserPreviewPopupContent()
        {
            InitializeComponent();
        }

        public async void SetImages(BitmapImage left, BitmapImage center, BitmapImage right)
        {
            UiHelper.SetImageSource(ImgLeft, left);
            UiHelper.SetImageSource(ImgCenter, center);
            UiHelper.SetImageSource(ImgRight, right);
            UiHelper.SetImageSource(UserAvatar, await PixivIO.FromUrl(this.GetDataContext<User>().Avatar));
            try
            {
                UiHelper.SetImageSource(Banner, await PixivIO.FromUrl((await HttpClientFactory.WebApiService().GetWebApiUserDetail(this.GetDataContext<User>().Id)).ResponseBody.UserDetails.CoverImage.ProfileCoverImage.The720X360));
            }
            catch
            {
                /* ignore */
            }
        }

        private async void FollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.FollowArtist(usr, RestrictPolicy.Public);
        }

        private async void UnFollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.UnFollowArtist(usr);
        }
    }
}
