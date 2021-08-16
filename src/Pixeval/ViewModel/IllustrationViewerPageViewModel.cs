using System;
using System.Linq;
using System.Threading.Tasks;
using Mako.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class IllustrationViewerPageViewModel : ObservableObject, IDisposable
    {
        // Remarks:
        // illustrations should contains only one item iff the illustration is a single
        // otherwise it contains the entire manga data
        public IllustrationViewerPageViewModel(params IllustrationViewModel[] illustrations)
        {
            Illustrations = illustrations.Select(i => new ImageViewerPageViewModel(i)).ToArray();
            Current = Illustrations[CurrentIndex];
            _ = LoadUserProfileImage();
        }

        public ImageViewerPageViewModel[] Illustrations { get; }

        private int _currentIndex;

        public int CurrentIndex
        {
            get => _currentIndex;
            private set => SetProperty(ref _currentIndex, value);
        }

        private ImageViewerPageViewModel _current = null!;

        public ImageViewerPageViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        private SoftwareBitmapSource? _userProfileImageSource;

        // Remarks:
        // The reason why we don't put UserProfileImageSource into IllustrationViewModel
        // is because the whole array of Illustrations is just representing the same 
        // illustration's different manga pages, so all of them have the same illustrator
        // if the UserProfileImageSource is in IllustrationViewModel and the illustration
        // itself is a manga then all of the IllustrationViewModel in Illustrations will
        // request the same user profile image which is pointless and will (inevitably) causing
        // the waste of system resource
        public SoftwareBitmapSource? UserProfileImageSource
        {
            get => _userProfileImageSource;
            set => SetProperty(ref _userProfileImageSource, value);
        }

        public string? IllustratorName => First.Illustration.User?.Name;

        public string? IllustratorUid => First.Illustration.User?.Id.ToString();

        public bool IsManga => Illustrations.Length > 1;

        public bool IsUgoira => Current.IllustrationViewModel.Illustration.IsUgoira();

        public IllustrationViewModel First => Illustrations[0].IllustrationViewModel;

        public ImageViewerPageViewModel Next()
        {
            Current = Illustrations[++CurrentIndex];
            return Current;
        }

        public ImageViewerPageViewModel Prev()
        {
            Current = Illustrations[--CurrentIndex];
            return Current;
        }

        public Task PostPublicBookmarkAsync()
        {
            return Illustrations[0].IllustrationViewModel.PostPublicBookmarkAsync();
        }

        public Task RemoveBookmarkAsync()
        {
            return Illustrations[0].IllustrationViewModel.RemoveBookmarkAsync();
        }

        private async Task LoadUserProfileImage()
        {
            if (First.Illustration.User?.ProfileImageUrls?.Medium is { } profileImage)
            {
                using var stream = (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                    .DownloadAsIRandomAccessStreamAsync(profileImage)).GetOrThrow();
                UserProfileImageSource = await stream.GetSoftwareBitmapSourceAsync();
            }
        }

        public bool IsBookmarked => Illustrations[0].IllustrationViewModel.IsBookmarked;

        public void Dispose()
        {
            foreach (var imageViewerPageViewModel in Illustrations)
            {
                imageViewerPageViewModel.Dispose();
            }
            _userProfileImageSource?.Dispose();
        }
    }
}