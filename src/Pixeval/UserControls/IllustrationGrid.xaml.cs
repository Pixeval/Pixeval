using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class IllustrationGrid
    {
        public IllustrationGridViewModel ViewModel { get; set; }

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
            var viewModel = sender.GetDataContext<IllustrationViewModel>().Illustration.GetMangaIllustrations().Select(p => new IllustrationViewModel(p)).ToArray();
            App.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModel), new SuppressNavigationTransitionInfo());
        }

        private void IllustrationThumbnailContainerItem_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            var context = sender.GetDataContext<IllustrationViewModel>();
            if (args.BringIntoViewDistanceY <= sender.ActualHeight) // one element ahead
            {
                _ = context.LoadThumbnailIfRequired();
                return;
            }

            switch (context)
            {
                case { LoadingThumbnail: true }:
                    context.LoadingThumbnailCancellationHandle.Cancel();
                    break;
                case { ThumbnailSource: not null }:
                    var source = context.ThumbnailSource;
                    context.ThumbnailSource = null;
                    source.Dispose();
                    break;
            }
        }
    }
}
