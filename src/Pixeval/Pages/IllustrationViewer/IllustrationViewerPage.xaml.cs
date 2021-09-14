using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Events;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    // TODO add context menu and comments section
    public sealed partial class IllustrationViewerPage
    {
        private IllustrationViewerPageViewModel _viewModel = null!;

        public IllustrationViewerPage()
        {
            InitializeComponent();
            var dataTransferManager = UIHelper.GetDataTransferManager();
            dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;
        }

        public override void Dispose(NavigatingCancelEventArgs e)
        {
            foreach (var imageViewerPageViewModel in _viewModel.ImageViewerPageViewModels)
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

            EventChannel.Default.Publish(new MainPageFrameSetConnectedAnimationTargetEvent(_viewModel.IllustrationGrid.GetItemContainer(_viewModel.IllustrationViewModelInTheGridView)));
        }

        private async void OnDataTransferManagerOnDataRequested(DataTransferManager _, DataRequestedEventArgs args)
        {
            // Remarks: all the illustrations in _viewModels only differ in different image sources
            var vm = _viewModel.Current.IllustrationViewModel;
            if (_viewModel.Current.LoadingOriginalSourceTask is not {IsCompletedSuccessfully: true})
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
            var file = await AppContext.CreateTemporaryFileWithRandomNameAsync(_viewModel.IsUgoira ? "gif" : "png");

            if (_viewModel.Current.OriginalImageStream is { } stream)
            {
                await stream.SaveToFile(file);

                props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnailStream);

                request.Data.SetStorageItems(Enumerates.ArrayOf(file), true);
                request.Data.SetWebLink(webLink);
                request.Data.SetApplicationLink(AppContext.GenerateAppLinkToIllustration(vm.Id));
            }

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
        
        private void NextIllustration()
        {
            var illustrationViewModel = (IllustrationViewModel) _viewModel.ContainerGridViewModel.IllustrationsView[_viewModel.IllustrationIndex + 1];
            var viewModel = illustrationViewModel.GetMangaIllustrationViewModels().ToArray();

            App.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(_viewModel.IllustrationGrid, viewModel), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            });
        }

        private void PrevIllustration()
        {
            var illustrationViewModel = (IllustrationViewModel)_viewModel.ContainerGridViewModel.IllustrationsView[_viewModel.IllustrationIndex - 1];
            var viewModel = illustrationViewModel.GetMangaIllustrationViewModels().ToArray();

            App.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(_viewModel.IllustrationGrid, viewModel), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            });
        }

        private void NextImageAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            NextImage();
        }

        private void PrevImageAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            PrevImage();
        }

        private void NextIllustrationAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            NextIllustration();
        }

        private void PrevIllustrationAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            PrevIllustration();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                animation.TryStart(IllustrationImageShowcaseFrame);
            }
        }

        private void BackButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", IllustrationImageShowcaseFrame);
            EventChannel.Default.Publish(new NavigatingBackToMainPageEvent(_viewModel.IllustrationViewModelInTheGridView));
            
            App.AppWindowRootFrame.BackStack.RemoveAll(entry => entry.SourcePageType == typeof(IllustrationViewerPage));
            if (App.AppWindowRootFrame.CanGoBack)
            {
                App.AppWindowRootFrame.GoBack(new DrillInNavigationTransitionInfo());
            }
        }

        private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
        {
            _viewModel.IsGenerateLinkTeachingTipOpen = false;
            App.AppSetting.DisplayTeachingTipWhenGeneratingAppLink = false;
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

        public Visibility CalculateNextImageButtonVisibility(int index)
        {
            return index < _viewModel.ImageViewerPageViewModels.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CalculatePrevImageButtonVisibility(int index)
        {
            return index > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CalculateNextIllustrationButtonVisibility(int index)
        {
            return _viewModel.ContainerGridViewModel.IllustrationsView.Count > _viewModel.IllustrationIndex + 1 
                ? CalculateNextImageButtonVisibility(index).Inverse() 
                : Visibility.Collapsed;
        }

        public Visibility CalculatePrevIllustrationButtonVisibility(int index)
        {
            return _viewModel.IllustrationIndex > 0 
                ? CalculatePrevImageButtonVisibility(index).Inverse()
                : Visibility.Collapsed;
        }

        #endregion
    }
}
