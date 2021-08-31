﻿using System;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Events;
using Pixeval.Util;
using Pixeval.Util.UI;
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
                EventChannel.Default.Publish(new LoginCompletedEvent(this, App.MakoClient!.Session));
            }
            catch (Exception exception)
            {
	            await MessageDialogBuilder.CreateAcknowledgement(
		            LoginPageResources.ErrorWhileLoggingInTitle,
		            LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception.Message));
                Application.Current.Exit();
            }
        }

        private async Task EnsureCertificateIsInstalled()
        {
            if (!await _viewModel.CheckFakeRootCertificateInstallationAsync())
            {
                var dialogResult = await MessageDialogBuilder.CreateOkCancel(
                    LoginPageResources.RootCertificateInstallationRequiredTitle,
                    LoginPageResources.RootCertificateInstallationRequiredContent);
                if (dialogResult)
                {
                    await _viewModel.InstallFakeRootCertificateAsync();
                }
                else
                {
                    await App.ExitWithPushedNotification();
                }
            }
        }

        private async Task EnsureWebView2IsInstalled()
        {
            if (!_viewModel.CheckWebView2Installation())
            {
                var dialogResult = await MessageDialogBuilder.CreateOkCancel(
                    LoginPageResources.WebView2InstallationRequiredTitle,
                    LoginPageResources.WebView2InstallationRequiredContent);
                if (dialogResult)
                {
                    await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
                }
                await App.ExitWithPushedNotification();
            }
        }
    }
}