using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Capability
{
    public sealed partial class FollowingsPage
    {
        private FollowingsPageViewModel _viewModel = new();


        public FollowingsPage()
        {
            InitializeComponent();
        }


        private async void FollowingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadFollowings();
        }
    }
}
