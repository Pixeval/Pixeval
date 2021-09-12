using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class IllustrationViewerPageViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// The view model of the GridView that the <see cref="ImageViewerPageViewModels"/> comes from
        /// </summary>
        public IllustrationGridViewModel ContainerGridViewModel { get; }

        public StandardUICommand CopyCommand { get; } = new(StandardUICommandKind.Copy);

        public StandardUICommand PlayGifCommand { get; } = new(StandardUICommandKind.Play)
        {
            // The gif will be played as soon as its loaded, so the default state is playing and thus we need the button to be pause
            Label = IllustrationViewerPageResources.PauseGif,
            IconSource = new SymbolIconSource
            {
                Symbol = Symbol.Stop
            }
        };

        public XamlUICommand ZoomOutCommand { get; } = new()
        {
            Label = IllustrationViewerPageResources.ZoomOut,
            IconSource = new SymbolIconSource
            {
                Symbol = Symbol.ZoomOut
            }
        };

        public XamlUICommand ZoomInCommand { get; } = new()
        {
            Label = IllustrationViewerPageResources.ZoomIn,
            IconSource = new SymbolIconSource
            {
                Symbol = Symbol.ZoomIn
            }
        };

        public XamlUICommand BookmarkCommand { get; private set; } = null!; // the null-state is transient

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
            InitializeCommands();
            _ = LoadUserProfileImage();
        }

        private void InitializeCommands()
        {
            BookmarkCommand = new XamlUICommand
            {
                KeyboardAccelerators =
                {
                    new KeyboardAccelerator
                    {
                        Modifiers = VirtualKeyModifiers.Control,
                        Key = VirtualKey.D
                    }
                },
                Label = FirstIllustrationViewModel.IsBookmarked ? IllustrationViewerPageResources.RemoveBookmark : IllustrationViewerPageResources.Bookmark,
                IconSource = GetBookmarkButtonIcon()
            };

            BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

            CopyCommand.CanExecuteRequested += CopyCommandOnCanExecuteRequested;
            CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;

            PlayGifCommand.CanExecuteRequested += PlayGifCommandOnCanExecuteRequested;
            PlayGifCommand.ExecuteRequested += PlayGifCommandOnExecuteRequested;

            ZoomInCommand.ExecuteRequested += (sender, args) =>
            {
                
            }
        }

        private void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        { 
            FirstImageViewerPageViewModel.SwitchBookmarkState();
            // update manually
            BookmarkCommand.Label = FirstIllustrationViewModel.IsBookmarked ? IllustrationViewerPageResources.RemoveBookmark : IllustrationViewerPageResources.Bookmark;
            BookmarkCommand.IconSource = GetBookmarkButtonIcon();
        }

        private IconSource GetBookmarkButtonIcon()
        {
            var systemThemeFontFamily = new FontFamily("Segoe MDL2 Assets");
            return FirstIllustrationViewModel.IsBookmarked
                ? new FontIconSource
                {
                    Glyph = "\xEB52", // HeartFill
                    Foreground = new SolidColorBrush(Colors.Crimson),
                    FontFamily = systemThemeFontFamily
                }
                : new FontIconSource
                {
                    Glyph = "\xEB51", // Heart
                    FontFamily = systemThemeFontFamily
                };
        }

        private void PlayGifCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = IsUgoira && (Current.LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false);
        }

        private void PlayGifCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            var bitmap = (BitmapImage) Current.OriginalImageSource!;
            if (bitmap.IsPlaying)
            {
                bitmap.Stop();
                PlayGifCommand.Label = IllustrationViewerPageResources.PlayGif;
                PlayGifCommand.IconSource = new SymbolIconSource
                {
                    Symbol = Symbol.Play
                };
            }
            else
            {
                bitmap.Play();
                PlayGifCommand.Label = IllustrationViewerPageResources.PauseGif;
                PlayGifCommand.IconSource = new SymbolIconSource
                {
                    Symbol = Symbol.Stop
                };
            }
        }

        private void CopyCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = Current.LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false;
        }

        private async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            await UIHelper.SetClipboardContentAsync(async package =>
            {
                package.RequestedOperation = DataPackageOperation.Copy;
                var file = await AppContext.CreateTemporaryFileWithNameAsync(GetCopyContentFileName(), IsUgoira ? "gif" : "png");
                await Current.OriginalImageStream!.SaveToFile(file);
                package.SetStorageItems(Enumerates.ArrayOf(file), true);
            });

            string GetCopyContentFileName()
            {
                return $"{IllustrationId}{(IsUgoira ? string.Empty : IsManga ? $"_p{CurrentIndex}" : string.Empty)}";
            }
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
        // If the UserProfileImageSource is in IllustrationViewModel and the illustration
        // itself is a manga then all of the IllustrationViewModel in Illustrations will
        // request the same user profile image which is pointless and will (inevitably) causing
        // the waste of system resource
        public SoftwareBitmapSource? UserProfileImageSource
        {
            get => _userProfileImageSource;
            set => SetProperty(ref _userProfileImageSource, value);
        }

        public string IllustrationId => FirstIllustrationViewModel.Illustration.Id.ToString();

        public string? IllustratorName => FirstIllustrationViewModel.Illustration.User?.Name;

        public string? IllustratorUid => FirstIllustrationViewModel.Illustration.User?.Id.ToString();

        public bool IsManga => ImageViewerPageViewModels.Length > 1;

        public bool IsUgoira => Current.IllustrationViewModel.Illustration.IsUgoira();

        public IllustrationViewModel FirstIllustrationViewModel => FirstImageViewerPageViewModel.IllustrationViewModel;

        public ImageViewerPageViewModel FirstImageViewerPageViewModel => ImageViewerPageViewModels[0];

        private bool _isInfoPaneOpen;

        public bool IsInfoPaneOpen
        {
            get => _isInfoPaneOpen;
            set => SetProperty(ref _isInfoPaneOpen, value);
        }

        public void UpdateCommandCanExecute()
        {
            PlayGifCommand.NotifyCanExecuteChanged();
            CopyCommand.NotifyCanExecuteChanged();
        }

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
            return FirstIllustrationViewModel.PostPublicBookmarkAsync();
        }

        public Task RemoveBookmarkAsync()
        {
            IllustrationViewModelInTheGridView.IsBookmarked = false;
            return FirstIllustrationViewModel.RemoveBookmarkAsync();
        }

        private async Task LoadUserProfileImage()
        {
            if (FirstIllustrationViewModel.Illustration.User?.ProfileImageUrls?.Medium is { } profileImage)
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