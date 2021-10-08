using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Misc;
using Pixeval.Pages.Misc;

namespace Pixeval
{
    public sealed partial class MainWindow : INavigationModeInfo
    {
        // Remarks: The parameter of OnNavigatedTo is always NavigationMode.New
        public NavigationMode? NavigationMode { get; private set; }

        public NavigationMode? GetNavigationModeAndReset()
        {
            var mode = NavigationMode;
            NavigationMode = null;
            return mode;
        }

        public NavigationTransitionInfo? DefaultNavigationTransitionInfo { get; internal set; } =
            new SuppressNavigationTransitionInfo();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PixevalAppRootFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            PixevalAppRootFrame.Navigate(typeof(LoginPage));
        }

        private void MainWindow_OnClosed(object sender, WindowEventArgs args)
        {
            AppContext.SaveContext();
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
