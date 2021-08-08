using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.UserControls;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class IllustrationViewerPage
    {
        private IllustrationViewerPageViewModel _viewModel = null!;

        public IllustrationViewerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel = (IllustrationViewerPageViewModel) e.Parameter;
            IllustrationImageShowcaseFrame.Navigate(typeof(ImageViewerPage), _viewModel.Current);

            if (ConnectedAnimationService.GetForCurrentView().GetAnimation(IllustrationGrid.ConnectedAnimationKey) is { } animation)
            {
                var image = IllustrationImageShowcaseFrame.FindChild(typeof(Image)); // See ImageViewerPage
                animation.TryStart(image);
                animation.TryStart(image);
            }
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
    }
}
