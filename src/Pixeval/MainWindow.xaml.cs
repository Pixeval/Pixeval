using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Events;
using Pixeval.Pages;
using Pixeval.Util;

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
            EventChannel.Default.SubscribeOnUIThread<LoginCompletedEvent>(RearrangeToDefaultWindowSize);
        }

        private async void MainWindow_OnClosed(object sender, WindowEventArgs args)
        {
            await App.ExitWithPushedNotification();
        }

        #region Helper Functions

        private static void RearrangeToDefaultWindowSize()
        {
            App.Window.SetWindowSize(1200, 800);
        }

        #endregion

        private void PixevalAppRootFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            throw e.Exception;
        }
    }
}
