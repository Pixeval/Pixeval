using System;
using Microsoft.UI.Xaml;
using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Pages
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
                _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.CheckingRefreshAvailable);
                if (await _viewModel.CheckRefreshAvailable())
                {
                    _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.Refreshing);
                    await _viewModel.Refresh();
                    Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo());
                }
                else
                {
                    _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.NegotiatingPort);
                    var port = IOHelper.NegotiatePort();
                    _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.ExecutingLoginProxy);
                    LoginPageViewModel.CallLoginProxy(CultureInfo.CurrentCulture.Name, port);
                    var (cookie, token) = await LoginPageViewModel.WhenLoginTokenRequestedAsync(port);
                }
            }
            catch (Exception exception)
            {
                _ = await MessageDialogBuilder.Create()
                    .WithTitle(LoginPageResources.ErrorWhileLoggingInTitle)
                    .WithContent(string.Format(LoginPageResources.ErrorWhileLogginInContentFormatted, exception.Message))
                    .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                    .WithDefaultButton(ContentDialogButton.Primary)
                    .Build(this)
                    .ShowAsync();
                Application.Current.Exit();
            }
        }
    }
}
