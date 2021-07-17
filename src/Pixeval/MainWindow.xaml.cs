using Microsoft.UI.Xaml;
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
            // This trick sucks, I doubt that the coupling here is inevitable unless one day in the future the 
            // WebView2 of WinUI will support proxy
            // We cannot use the event channel here, the closed event is transient, it won't wait for the event channel
            // to publish the event and the subscriber to execute the callback, if we publish an event here, it is impossible
            // to guarantee the subscriber will receive this event, because the whole CLR might already shutdown before that
            // happens, the same logic also applies to the next line
            args.Handled = true;
            await App.ExitWithPushedNotification();
        }
    }
}
