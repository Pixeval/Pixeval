using Mako.Engine;
using Mako.Model;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class IllustrationGridPage
    {
        private readonly IllustrationGridPageViewModel _viewModel = new();

        public IllustrationGridPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _viewModel.ResetAndFill(e.Parameter as IFetchEngine<Illustration?>);
        }
    }
}
