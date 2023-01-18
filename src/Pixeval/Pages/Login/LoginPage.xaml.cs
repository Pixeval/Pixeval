#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/LoginPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Login;

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
                    LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception.StackTrace))
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
                App.ExitWithPushNotification();
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

            App.ExitWithPushNotification();
        }
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        _viewModel.Deactivate();
    }
}
