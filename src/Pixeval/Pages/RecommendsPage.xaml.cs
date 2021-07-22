using Mako.Engine;
using Mako.Model;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Pages
{
    public sealed partial class RecommendsPage
    {
        public RecommendsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await IllustrationGrid.ViewModel.Fill(e.Parameter as IFetchEngine<Illustration?>);
        }
    }
}
