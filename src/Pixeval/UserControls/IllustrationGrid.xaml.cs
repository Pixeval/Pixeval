using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{

    // TODO determine why the ThumbnailSource is null after navigation
    public sealed partial class IllustrationGrid
    {
        public IllustrationGridViewModel ViewModel { get; set; }

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
            var viewModel = sender.GetDataContext<IllustrationViewModel>().Illustration.GetMangaIllustrations().Select(p => new IllustrationViewModel(p)).ToArray();
            App.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModel), new DrillInNavigationTransitionInfo());
        }

        private void IllustrationThumbnailContainerItem_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            var context = sender.GetDataContext<IllustrationViewModel>();
            if (args.BringIntoViewDistanceY <= sender.ActualHeight) // one element ahead
            {
                _ = context.LoadThumbnailIfRequired();
                return;
            }

            // small tricks to reduce memory consumption
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
