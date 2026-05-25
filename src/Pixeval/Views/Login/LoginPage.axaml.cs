// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Home;

namespace Pixeval.Views.Login;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not LoginPageViewModel viewModel)
            return;

        var token = viewModel.RefreshToken;
        if (string.IsNullOrWhiteSpace(token))
            return;

        App.AppViewModel.MakoClient.SetToken(token);
        if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
            LoginNavigate();
    }

    private async void OpenWebView_OnClick(object? sender, RoutedEventArgs e)
    {
        var verifier = PixivAuth.GetCodeVerify();
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer } topLevel)
            return;
        try
        {
            var result = await WebAuthenticationBroker.AuthenticateAsync(
                topLevel,
                new(
                    new(PixivAuth.GenerateWebPageUrl(verifier)),
                    new("pixiv://account/login"))
                {
                    PreferNativeWebDialog = true,
                    NonPersistent = true
                });

            if (result.CallbackUri is not { } callbackUri)
                return;
            var code = HttpUtility.ParseQueryString(callbackUri.Query)["code"];
            if (string.IsNullOrWhiteSpace(code))
                return;
            App.AppViewModel.MakoClient.SetCode(code, verifier);
            if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
                LoginNavigate();
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception exception)
        {
            viewContainer.ShowError(exception.GetType().ToString(), exception.Message);
            _ = await viewContainer.CreateAcknowledgementAsync(
                I18NManager.GetResource(LoginPageResources.FetchingSessionFailedTitle),
                I18NManager.GetResource(LoginPageResources.FetchingSessionFailedContent));
        }
    }

    public void LoginNavigate()
    {
        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
        viewContainer?.NavigateTo(new HomePage(), true);
        App.AppViewModel.QueueWorkSubscriptionSyncAll();
    }

    private void RefreshTokenBox_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is AutoCompleteBox box)
            box.IsDropDownOpen = true;
    }
}
