using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Pages;

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

        private async void MainWindow_OnClosed(object sender, WindowEventArgs args)
        {
            await App.ExitWithPushedNotification();
        }

        private void PixevalAppRootFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            throw e.Exception;
        }
    }
}
