using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using PInvoke;
using Pixeval.Pages;
using Pixeval.Util;

namespace Pixeval
{
    public sealed partial class MainWindow : INavigationModeInfo
    {
        // The parameter of OnNavigatedTo is always NavigationMode.New
        public NavigationMode NavigationMode { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Title = "Pixeval";
            // TODO awaiting WinUI3 fix: see https://github.com/microsoft/microsoft-ui-xaml/issues/3689
            LoadIcon();
        }

        // TODO awaiting WinUI3 fix: see https://github.com/microsoft/microsoft-ui-xaml/issues/4056
        private static async void LoadIcon()
        {
            var icon = User32.LoadImage(IntPtr.Zero, await AppContext.GetIconAbsolutePath(), User32.ImageType.IMAGE_ICON, 44, 44, User32.LoadImageFlags.LR_LOADFROMFILE);
            User32.SendMessage(App.GetMainWindowHandle(), User32.WindowMessage.WM_SETICON, IntPtr.Zero, icon);
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

        private void PixevalAppRootFrame_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            NavigationMode = e.NavigationMode;
        }
    }
}
