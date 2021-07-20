using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using JetBrains.Annotations;
using Mako.Net;
using Mako.Util;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            App.EventChannel.Subscribe<LoginCompletedEvent>(evt  =>
            {
                Debug.WriteLine(evt.Session.ToJson());
            });
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
        public async void DownloadAvatar()
        {
            // get byte array of avatar
            var makoClient = App.MakoClient!;
            var byteArray = (await makoClient
                    .GetMakoHttpClient(MakoApiKind.ImageApi)
                    .DownloadMemoryStreamAsync(makoClient.Session.AvatarUrl!))
                .GetOrThrow();
            // set to the bitmap image
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(byteArray.AsRandomAccessStream());
            Avatar = bitmapImage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}