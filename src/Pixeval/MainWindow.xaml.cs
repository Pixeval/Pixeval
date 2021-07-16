using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Pixeval.Events;
using Pixeval.Pages;
using Pixeval.ViewModel;

namespace Pixeval
{
    public sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "Pixeval";
        }

        private void PixevalAppRootFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            PixevalAppRootFrame.Navigate(typeof(LoginPage));
        }

        private void MainWindow_OnClosed(object sender, WindowEventArgs args)
        {
            // This trick sucks, I doubt that the coupling here is inevitable
            LoginPageViewModel.Cleanup();
        }
    }
}
