using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Pages;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    // TODO loading thumbnail on-demand
    public sealed partial class IllustrationGrid
    {
        public IllustrationGridViewModel ViewModel { get; set; }

        public Page? Owner { get; set; }

        public const string ConnectedAnimationKey = "IllustrationGridForwardAnimation";

        public IllustrationGrid()
        {
            InitializeComponent();
            ViewModel = new IllustrationGridViewModel();
        }

        private async void RemoveBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var viewModel = sender.GetDataContext<IllustrationViewModel>();
            await viewModel!.RemoveBookmarkAsync();
        }

        private async void PostBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var viewModel = sender.GetDataContext<IllustrationViewModel>();
            await viewModel!.PostPublicBookmarkAsync();
        }

        private void Thumbnail_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ConnectedAnimationKey, sender as Image);
            animation.Configuration = new DirectConnectedAnimationConfiguration();
            // TODO use App.Window.RootFrame
            var viewModel = sender.GetDataContext<IllustrationViewModel>().Illustration.GetMangaIllustrations().Select(p => new IllustrationViewModel(p)).ToArray();
            Owner!.Frame.Navigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModel), new SuppressNavigationTransitionInfo());
        }
    }
}
