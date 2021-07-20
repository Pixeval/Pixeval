using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Mako.Net;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            EventChannel.Default.SubscribeOnUIThread<LoginCompletedEvent>(DownloadAndSetAvatar);
        }

        public double MainPageRootNavigationViewOpenPanelLength => 200;

        public double MainPageAutoSuggestionBoxWidth => 300;

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

        public Thickness RearrangeMainPageAutoSuggestionBoxMargin(double windowWidth, double leftControlWidth)
        {
            return new(windowWidth / 2 - leftControlWidth - MainPageAutoSuggestionBoxWidth / 2, 0, 0, 0);
        }

        /// <summary>
        /// Download user's avatar and set to the Avatar property.
        /// </summary>
        public async void DownloadAndSetAvatar()
        {
            var makoClient = App.MakoClient!;
            // get byte array of avatar
            // and set to the bitmap image
            var imageStream = await makoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                .DownloadAsIRandomAccessStreamAsync(makoClient.Session.AvatarUrl!);
            Avatar = await imageStream.GetOrThrow().GetImageSourceAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}