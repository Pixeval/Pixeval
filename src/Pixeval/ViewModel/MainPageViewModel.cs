using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Mako.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.Events;

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

        public void DownloadAvatar()
        {
            
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}