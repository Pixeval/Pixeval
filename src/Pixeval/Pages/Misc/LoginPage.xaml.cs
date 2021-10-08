using System;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Pages.Misc
{
    public sealed partial class LoginPage
    {
        private readonly LoginPageViewModel _viewModel = new();

        public LoginPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.CheckRefreshAvailable())
                {
                    await _viewModel.RefreshAsync();
                    Frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                }
                else
                {
                    await EnsureCertificateIsInstalled();
                    await EnsureWebView2IsInstalled();
                    await _viewModel.WebLoginAsync();
                    Frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                }

                AppContext.SaveContext();
                WeakReferenceMessenger.Default.Send(new LoginCompletedMessage(this, App.AppViewModel.MakoClient.Session));
            }
            catch (Exception exception)
            {
                _ = await MessageDialogBuilder.CreateAcknowledgement(
                        this,
                        LoginPageResources.ErrorWhileLoggingInTitle,
                        LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception.Message))
                    .ShowAsync();
                Application.Current.Exit();
            }
        }

        private async Task EnsureCertificateIsInstalled()
        {
            if (!await _viewModel.CheckFakeRootCertificateInstallationAsync())
            {
                var dialogResult = await MessageDialogBuilder.CreateOkCancel(this,
                    LoginPageResources.RootCertificateInstallationRequiredTitle,
                    LoginPageResources.RootCertificateInstallationRequiredContent).ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                {
                    await _viewModel.InstallFakeRootCertificateAsync();
                }
                else
                {
                    App.AppViewModel.ExitWithPushedNotification();
                }
            }
        }

        private async Task EnsureWebView2IsInstalled()
        {
            if (!_viewModel.CheckWebView2Installation())
            {
                var dialogResult = await MessageDialogBuilder.CreateOkCancel(this,
                    LoginPageResources.WebView2InstallationRequiredTitle,
                    LoginPageResources.WebView2InstallationRequiredContent).ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                {
                    await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
                }

                App.AppViewModel.ExitWithPushedNotification();
            }
        }

        public override void OnPageDeactivated(NavigatingCancelEventArgs e)
        {
            _viewModel.Deactivate();
        }
    }
}