#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LoginPage.xaml.cs
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
using Windows.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.AppManagement;
using Pixeval.Logging;
using Pixeval.Settings.Models;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.Login;

public sealed partial class LoginPage
{
    private readonly LoginPageViewModel _viewModel;
    public static LoginPage? Current { get; private set; }

    private const string RefreshToken = nameof(RefreshToken);
    private const string Browser = nameof(Browser);
    private const string WebView = nameof(WebView);

    public LoginPage()
    {
        Current = this;
        _viewModel = new(this);
        InitializeComponent();
    }

    private void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel.LogoutExit)
        {
            _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.WaitingForUserInput);
            _viewModel.IsEnabled = true;
        }
        else
        {
            Refresh(App.AppViewModel.LoginContext.RefreshToken);
        }

        // 首页语言
        foreach (var (displayName, name) in LanguageAppSettingsEntry.AvailableLanguages)
        {
            var radioMenuFlyoutItem = new RadioMenuFlyoutItem
            {
                Text = displayName,
                Tag = name
            };
            if (name == ApplicationLanguages.PrimaryLanguageOverride)
                radioMenuFlyoutItem.IsChecked = true;
            radioMenuFlyoutItem.Click += RadioMenuFlyoutItemClick;
            MenuFlyout.Items.Add(radioMenuFlyoutItem);
        }

        return;

        static void RadioMenuFlyoutItemClick(object sender2, RoutedEventArgs e2)
        {
            var item = sender2.To<RadioMenuFlyoutItem>();
            ApplicationLanguages.PrimaryLanguageOverride = item.Tag.To<string>();
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        MenuFlyout.ShowAt(sender.To<FrameworkElement>());
    }

    private async void Refresh(string refreshToken)
    {
        try
        {
            if (refreshToken.IsNotNullOrEmpty() && await _viewModel.RefreshAsync(refreshToken))
            {
                SuccessNavigating();
            }
            else
            {
                _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.WaitingForUserInput);
                _viewModel.IsEnabled = true;
            }
        }
        catch (Exception exception)
        {
            _ = await this.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle,
                LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception.StackTrace));
            _viewModel.CloseWindow();
        }
    }

    public static void SuccessNavigating()
    {
        if (Current is null || App.AppViewModel.MakoClient == null!)
            ThrowHelper.Exception();
        Current._viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.SuccessNavigating);
        Current.NavigateParent<MainPage>(null, new DrillInNavigationTransitionInfo());
        Current._viewModel.LogoutExit = false;
        AppInfo.SaveContext();
    }

    private void SwitchPresenterButton_OnClicked(object sender, RoutedEventArgs e) => SwitchPresenter.Value = sender.To<FrameworkElement>().GetTag<string>();

    #region Token

    private void TokenLogin_OnClicked(object sender, object e) => Refresh(_viewModel.RefreshToken);

    #endregion

    #region Browser

    private void BrowserLogin_OnClicked(object sender, RoutedEventArgs e) => _viewModel.BrowserLogin();

    #endregion

    #region WebView

    private async void WebViewLogin_OnClicked(object sender, object e) => await WebView2LoginAsync(false);

    private async void WebViewLoginNewAccount_OnClicked(object sender, RoutedEventArgs e) => await WebView2LoginAsync(true);

    private async Task WebView2LoginAsync(bool useNewAccount)
    {
        try
        {
            await _viewModel.WebView2LoginAsync(HWnd, useNewAccount, () => DispatcherQueue.TryEnqueue(SuccessNavigating));
            _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.WaitingForUserInput);
        }
        catch (Exception exception)
        {
            _ = await HWnd.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle,
                LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception + "\n" + exception.StackTrace));
            _viewModel.CloseWindow();
        }
    }

    private void LoginPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
    }

    #endregion
}
