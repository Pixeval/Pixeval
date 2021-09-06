using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class IllustrationViewerPageViewModel : ObservableObject, IDisposable
    {
        public IllustrationGridViewModel GridViewModel;
        public IllustrationGrid GridView;

        // Remarks:
        // illustrations should contains only one item if the illustration is a single
        // otherwise it contains the entire manga data
        public IllustrationViewerPageViewModel(IllustrationGrid gridView, params IllustrationViewModel[] illustrations)
        {
            Illustrations = illustrations.Select(i => new ImageViewerPageViewModel(i)).ToArray();
            Current = Illustrations[CurrentIndex];
            _ = LoadUserProfileImage();
            GridView = gridView;
            GridViewModel = gridView.ViewModel;
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

        public IllustrationViewModel IllustrationViewModel =>
            GridViewModel.IllustrationsView.Select(a => (IllustrationViewModel)a).First(model =>
                model.Illustration.GetMangaIllustrations().All(illustration =>
                    Illustrations.Any(x => x.IllustrationViewModel.Illustration == illustration)));

        public int IllustrationIndex => GridViewModel.IllustrationsView.IndexOf(IllustrationViewModel);

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
                UserProfileImageSource = await App.MakoClient.DownloadSoftwareBitmapSourceAsync(profileImage);
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