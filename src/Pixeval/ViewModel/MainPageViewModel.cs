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
        public double MainPageRootNavigationViewOpenPanelLength => 250;

        private ImageSource? _avatar;

        public ImageSource? Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }

        /// <summary>
        /// Download user's avatar and set to the Avatar property.
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

        public void Receive(LoginCompletedMessage message)
        {
            DownloadAndSetAvatar();
        }
    }
}