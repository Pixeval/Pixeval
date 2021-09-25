using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Net;
using Pixeval.Messages;
using Pixeval.Util.IO;

namespace Pixeval.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            WeakReferenceMessenger.Default.Register<MainPageViewModel, LoginCompletedMessage>(this, (recipient, _) => recipient.DownloadAndSetAvatar());
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
            var makoClient = App.AppViewModel.MakoClient!;
            // get byte array of avatar
            // and set to the bitmap image
            Avatar = await (await makoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(makoClient.Session.AvatarUrl!))
                .GetOrThrow()
                .GetBitmapImageAsync(true);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}