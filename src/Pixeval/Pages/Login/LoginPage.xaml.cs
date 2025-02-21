// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
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
            _viewModel.AdvancePhase(LoginPhaseEnum.WaitingForUserInput);
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
            if (refreshToken.IsNotNullOrEmpty())
            {
                _viewModel.AdvancePhase(LoginPhaseEnum.Refreshing);
                var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
                App.AppViewModel.MakoClient = new MakoClient(refreshToken, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger);

                SuccessNavigating();
            }
            else
            {
                _viewModel.AdvancePhase(LoginPhaseEnum.WaitingForUserInput);
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
        Current._viewModel.AdvancePhase(LoginPhaseEnum.SuccessNavigating);

        WindowFactory.GetWindowForElement(Current).PageContent = new MainPage();
        Current._viewModel.LogoutExit = false;
        App.AppViewModel.MakoClient.Me.IsPremium = App.AppViewModel.LoginContext.IsPremium;
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
            await _viewModel.WebView2LoginAsync(Window, useNewAccount, () => DispatcherQueue.TryEnqueue(SuccessNavigating));
            _viewModel.AdvancePhase(LoginPhaseEnum.WaitingForUserInput);
        }
        catch (Exception exception)
        {
            _ = await this.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle,
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
