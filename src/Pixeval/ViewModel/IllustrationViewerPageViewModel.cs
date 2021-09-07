using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.UserControls;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class IllustrationViewerPageViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// The view model of the GridView that the <see cref="ImageViewerPageViewModels"/> comes from
        /// </summary>
        public IllustrationGridViewModel ContainerGridViewModel { get; }

        /// <summary>
        /// The <see cref="IllustrationGrid"/> that owns <see cref="ContainerGridViewModel"/>
        /// </summary>
        public IllustrationGrid IllustrationGrid { get; }

        // Remarks:
        // illustrations should contains only one item if the illustration is a single
        // otherwise it contains the entire manga data
        public IllustrationViewerPageViewModel(IllustrationGrid gridView, params IllustrationViewModel[] illustrations)
        {
            ImageViewerPageViewModels = illustrations.Select(i => new ImageViewerPageViewModel(this, i)).ToArray();
            Current = ImageViewerPageViewModels[CurrentIndex];
            IllustrationGrid = gridView;
            ContainerGridViewModel = gridView.ViewModel;
            IllustrationViewModelInTheGridView = ContainerGridViewModel.IllustrationsView.Cast<IllustrationViewModel>().First(model => model.Id == Current.IllustrationViewModel.Id);
            _ = LoadUserProfileImage();
        }

        public ImageViewerPageViewModel[] ImageViewerPageViewModels { get; }

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

        /// <summary>
        /// The <see cref="IllustrationViewModelInTheGridView"/> in <see cref="IllustrationGrid"/> that corresponds to current
        /// <see cref="IllustrationViewerPageViewModel"/>
        /// </summary>
        public IllustrationViewModel IllustrationViewModelInTheGridView { get; }

        /// <summary>
        /// The index of current illustration in <see cref="IllustrationGrid"/>
        /// </summary>
        public int IllustrationIndex => ContainerGridViewModel.IllustrationsView.IndexOf(IllustrationViewModelInTheGridView);

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

        public string? IllustratorName => FirstImageViewerPageViewModel.Illustration.User?.Name;

        public string? IllustratorUid => FirstImageViewerPageViewModel.Illustration.User?.Id.ToString();

        public bool IsManga => ImageViewerPageViewModels.Length > 1;

        public bool IsUgoira => Current.IllustrationViewModel.Illustration.IsUgoira();

        /// <summary>
        /// The first <see cref="ImageViewerPageViewModel"/> among <see cref="ImageViewerPageViewModels"/>
        /// </summary>
        public IllustrationViewModel FirstImageViewerPageViewModel => ImageViewerPageViewModels[0].IllustrationViewModel;

        public ImageViewerPageViewModel Next()
        {
            Current = ImageViewerPageViewModels[++CurrentIndex];
            return Current;
        }

        public ImageViewerPageViewModel Prev()
        {
            Current = ImageViewerPageViewModels[--CurrentIndex];
            return Current;
        }

        public Task PostPublicBookmarkAsync()
        {
            // changes the IsBookmarked property of the item that of in the thumbnail list
            // so the thumbnail item will also receives state update 
            IllustrationViewModelInTheGridView.IsBookmarked = true;
            return FirstImageViewerPageViewModel.PostPublicBookmarkAsync();
        }

        public Task RemoveBookmarkAsync()
        {
            IllustrationViewModelInTheGridView.IsBookmarked = false;
            return FirstImageViewerPageViewModel.RemoveBookmarkAsync();
        }

        private async Task LoadUserProfileImage()
        {
            if (FirstImageViewerPageViewModel.Illustration.User?.ProfileImageUrls?.Medium is { } profileImage)
            {
                UserProfileImageSource = await App.MakoClient.DownloadSoftwareBitmapSourceAsync(profileImage);
            }
        }

        public void Dispose()
        {
            foreach (var imageViewerPageViewModel in ImageViewerPageViewModels)
            {
                imageViewerPageViewModel.Dispose();
            }
            _userProfileImageSource?.Dispose();
        }
    }
}