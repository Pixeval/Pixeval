using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class IllustrationGrid
    {
        public IllustrationGridPageViewModel ViewModel { get; }

        public IllustrationGrid()
        {
            InitializeComponent();
            ViewModel = new IllustrationGridPageViewModel();
        }

        private async void RemoveBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var viewModel = sender.GetDataContext<IllustrationViewModel>();
            await viewModel!.RemoveBookmarkAsync();
        }

        private async void PostBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var viewModel = sender.GetDataContext<IllustrationViewModel>();
            await viewModel!.PostPublicBookmarkAsync();
        }
    }
}
