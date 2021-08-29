using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.System;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    // TODO add context menu and file info and comments section
    public sealed partial class IllustrationViewerPage
    {
        private IllustrationViewerPageViewModel _viewModel = null!;

        private readonly StandardUICommand _copyCommand = new(StandardUICommandKind.Copy)
        {
            KeyboardAccelerators =
            {
                new KeyboardAccelerator
                {
                    Key = VirtualKey.C,
                    Modifiers = VirtualKeyModifiers.Control
                }
            }
        };

        public IllustrationViewerPage()
        {
            InitializeComponent();
            var dataTransferManager = UIHelper.GetDataTransferManager();
            dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;
            _copyCommand.CanExecuteRequested += CopyCommandOnCanExecuteRequested;
            _copyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;
        }

        public override void Dispose(NavigatingCancelEventArgs e)
        {
            foreach (var imageViewerPageViewModel in _viewModel.Illustrations)
            {
                imageViewerPageViewModel.ImageLoadingCancellationHandle.Cancel();
            }
            _viewModel.Dispose();
        }

        public override void Prepare(NavigationEventArgs e)
        {
            _viewModel = (IllustrationViewerPageViewModel) e.Parameter;
            _illustrationInfo = new NavigationViewTag(typeof(IllustrationInfoPage), _viewModel);
            _comments = null; // TODO

            IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Current);
        }

        private async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            await UIHelper.SetClipboardContentAsync(async package =>
            {
                package.RequestedOperation = DataPackageOperation.Copy;
                if (_viewModel.IsUgoira)
                {
                    var file = await AppContext.CreateTemporaryFileWithRandomNameAsync("gif");
                    await _viewModel.Current.OriginalImageStream!.SaveToFile(file);
                    package.SetStorageItems(Enumerates.ArrayOf(file), true);
                }
                else
                {
                    _viewModel.Current.OriginalImageStream!.Seek(0);
                    // Remarks: OriginalImageStream won't work here, I don't know why for now
                    package.SetBitmap(RandomAccessStreamReference.CreateFromStream(await GetImage().GetUnderlyingStreamAsync()));
                }
            });
        }

        private void CopyCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = _viewModel.Current.LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false;
        }

        private async void OnDataTransferManagerOnDataRequested(DataTransferManager _, DataRequestedEventArgs args)
        {
            // Remarks: all the illustrations in _viewModels only differ in different image sources
            var vm = _viewModel.Current.IllustrationViewModel; 
            if (_viewModel.Current.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
            {
                return;
            }

            var request = args.Request;
            var deferral = request.GetDeferral();
            var props = request.Data.Properties;
            var webLink = MakoHelper.GetIllustrationWebUri(vm.Id);
            props.Title = IllustrationViewerPageResources.ShareTitleFormatted.Format(vm.Id);
            props.Description = vm.Illustration.Title;
            props.Square30x30Logo = RandomAccessStreamReference.CreateFromStream(await AppContext.GetAssetStreamAsync("Images/logo44x44.ico"));
            var thumbnailStream = await _viewModel.Current.IllustrationViewModel.GetThumbnail(ThumbnailUrlOption.SquareMedium);
            var originalStream = await GetImage().GetUnderlyingStreamAsync(vm.Illustration.IsUgoira());
            props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnailStream);
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(originalStream));
            request.Data.SetWebLink(webLink);
            request.Data.SetApplicationLink(AppContext.GenerateAppLinkToIllustration(vm.Id));
            deferral.Complete();
        }

        private void NextImage()
        {
            IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Next(), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            });
        }

        private void PrevImage()
        {
            IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Prev(), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            });
        }

        private void NextImageAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            NextImage();
        }

        private void PrevImageAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            PrevImage();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.AppWindowRootFrame.GoBack(new DrillInNavigationTransitionInfo());
        }

        private async void ShareButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Current.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
            {
                await MessageDialogBuilder.CreateAcknowledgement(this, IllustrationViewerPageResources.CannotShareImageForNowTitle, IllustrationViewerPageResources.CannotShareImageForNowContent)
                    .ShowAsync();
                return;
            }
            UIHelper.ShowShareUI();
        }

        private void GenerateWebLinkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var link = MakoHelper.GetIllustrationWebUri(_viewModel.Current.IllustrationViewModel.Id).ToString();
            UIHelper.SetClipboardContent(package => package.SetText(link));
            UIHelper.ShowTextToastNotification(
                IllustrationViewerPageResources.WebLinkCopiedToClipboardToastTitle,
                link);
        }

        private void ZoomInButton_OnClick(object sender, RoutedEventArgs e)
        {
            (IllustrationImageShowcaseFrame.Content as ImageViewerPage)?.Zoom(0.5);
        }

        private void ZoomOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            (IllustrationImageShowcaseFrame.Content as ImageViewerPage)?.Zoom(-0.5);
        }

        private void SaveCurrentImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SaveAsCurrentImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SaveMangaButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SaveMangaAsButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void OpenInWebBrowserButton_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(MakoHelper.GetIllustrationWebUri(_viewModel.Current.IllustrationViewModel.Id));
        }

        private void GenerateLinkToThisPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (App.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
            {
                GenerateLinkToThisPageButtonTeachingTip.IsOpen = true;
            }
            UIHelper.SetClipboardContent(package => package.SetText(AppContext.GenerateAppLinkToIllustration(_viewModel.Current.IllustrationViewModel.Id).ToString()));
        }

        private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
        {
            GenerateLinkToThisPageButtonTeachingTip.IsOpen = false;
            App.AppSetting.DisplayTeachingTipWhenGeneratingAppLink = false;
        }

        private void PlayGifButton_OnClick(object sender, RoutedEventArgs e)
        {
            var appBarButton = (AppBarButton) sender;
            var bitmap = (BitmapImage) _viewModel.Current.OriginalImageSource!;
            if (bitmap.IsPlaying)
            {
                bitmap.Stop();
                appBarButton.Icon = new SymbolIcon(Symbol.Play);
            }
            else
            {
                bitmap.Play();
                appBarButton.Icon = new SymbolIcon(Symbol.Stop);
            }
        }

        private void BookmarkButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsBookmarked)
            {
                _viewModel.RemoveBookmarkAsync();
            }
            else
            {
                _viewModel.PostPublicBookmarkAsync();
            }
        }

        private void IllustrationInfoAndCommentsNavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            IllustrationInfoAndCommentsSplitView.IsPaneOpen = false;
        }

        private void IllustrationInfoAndCommentsNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag })
            {
                IllustrationInfoAndCommentsFrame.Navigate(tag.NavigateTo, tag.Parameter, new SlideNavigationTransitionInfo
                {
                    Effect = tag switch
                    {
                        var x when x == _illustrationInfo => SlideNavigationTransitionEffect.FromLeft,
                        var x when x == _comments => SlideNavigationTransitionEffect.FromRight,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                });
            }
        }

        #region Helper Functions

        // Tags for IllustrationInfoAndCommentsNavigationView

        private NavigationViewTag? _illustrationInfo;

        private NavigationViewTag? _comments;

        private Image GetImage()
        {
            return (Image) IllustrationImageShowcaseFrame.FindChild(typeof(Image))!;
        }

        public Visibility CalculateNextImageButtonVisibility(int index)
        {
            return index < _viewModel.Illustrations.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CalculatePrevImageButtonVisibility(int index)
        {
            return index > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public static IconElement GetBookmarkButtonIcon(bool isBookmarked)
        {
            var systemThemeFontFamily = new FontFamily("Segoe MDL2 Assets");
            return isBookmarked
                ? new FontIcon
                {
                    Glyph = "\xEB52", // HeartFill
                    Foreground = new SolidColorBrush(Colors.Crimson),
                    FontFamily = systemThemeFontFamily
                }
                : new FontIcon
                {
                    Glyph = "\xEB51", // Heart
                    FontFamily = systemThemeFontFamily
                };
        }

        public static string GetBookmarkButtonLabel(bool isBookmarked)
        {
            return isBookmarked ? IllustrationViewerPageResources.RemoveBookmark : IllustrationViewerPageResources.Bookmark;
        }

        #endregion
    }
}
