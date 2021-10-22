#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MainPageViewModel.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Net;
using Pixeval.Messages;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class MainPageViewModel : AutoActivateObservableRecipient, IRecipient<LoginCompletedMessage>
    {
        private ImageSource? _avatar;
        public double MainPageRootNavigationViewOpenPanelLength => 250;

        public ImageSource? Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }

        public void Receive(LoginCompletedMessage message)
        {
            DownloadAndSetAvatar();
        }

        /// <summary>
        ///     Download user's avatar and set to the Avatar property.
        /// </summary>
        public async void DownloadAndSetAvatar()
        {
            var makoClient = App.AppViewModel.MakoClient;
            // get byte array of avatar
            // and set to the bitmap image
            Avatar = await (await makoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(makoClient.Session.AvatarUrl!))
                .GetOrThrow()
                .GetBitmapImageAsync(true);
        }
    }
}