using Microsoft.UI.Xaml;
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
            App.EventChannel.SubscribeOnUIThread<LoginCompletedEvent>(RearrangeToDefaultWindowSize);
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
    }
}
