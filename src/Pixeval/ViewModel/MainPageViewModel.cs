using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Mako.Net;
using Microsoft.UI.Xaml.Media;
using Pixeval.Events;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            EventChannel.Default.Subscribe<LoginCompletedEvent>(DownloadAndSetAvatar);
        }

        public double MainPageRootNavigationViewOpenPanelLength => 250;

        private ImageSource? _avatar;

        public ImageSource? Avatar
        {
            get => _avatar;
            set
            {
                if (Equals(value, _avatar)) return;
                _avatar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Download user's avatar and set to the Avatar property.
        /// </summary>
        public async void DownloadAndSetAvatar()
        {
            var makoClient = App.MakoClient!;
            // get byte array of avatar
            // and set to the bitmap image
            using var imageStream = (await makoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                .DownloadAsIRandomAccessStreamAsync(makoClient.Session.AvatarUrl!)).GetOrThrow();
            Avatar = await imageStream.GetBitmapImageSourceAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}