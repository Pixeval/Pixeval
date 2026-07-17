// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Home;

namespace Pixeval.Views.Login;

public partial class LoginPage : IconContentPage
{
    private CancellationTokenSource? _loadUsersCancellationTokenSource;

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!TryBeginLogin())
            return;

        try
        {
            if (DataContext is not LoginPageViewModel viewModel)
                return;

            var token = viewModel.RefreshToken;
            if (string.IsNullOrWhiteSpace(token))
                return;

            await App.AppViewModel.MakoClient.SetTokenAsync(token);
            if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
                LoginNavigate();
        }
        finally
        {
            EndLogin();
        }
    }

    private async void OpenWebView_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!TryBeginLogin())
            return;

        try
        {
            var verifier = PixivAuth.GetCodeVerify();
            if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer } topLevel)
                return;

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
            await App.AppViewModel.MakoClient.SetCodeAsync(code, verifier);
            if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
                LoginNavigate();
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception exception)
        {
            if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            {
                viewContainer.ShowError(exception.GetType().ToString(), exception.Message);
                _ = await viewContainer.CreateAcknowledgementAsync(
                    I18NManager.GetResource(LoginPageResources.FetchingSessionFailedTitle),
                    I18NManager.GetResource(LoginPageResources.FetchingSessionFailedContent));
            }
        }
        finally
        {
            EndLogin();
        }
    }

    public void LoginNavigate()
    {
        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
        viewContainer?.NavigateTo(new HomePage(), true);
        App.AppViewModel.QueueWorkSubscriptionSyncAll();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _loadUsersCancellationTokenSource?.Cancel();
        _loadUsersCancellationTokenSource?.Dispose();
        var cancellationTokenSource = new CancellationTokenSource();
        _loadUsersCancellationTokenSource = cancellationTokenSource;
        try
        {
            if (DataContext is LoginPageViewModel viewModel)
                await viewModel.LoadUsersAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(LoginPageViewModel.LoadUsersAsync), exception);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _loadUsersCancellationTokenSource?.Cancel();
        _loadUsersCancellationTokenSource?.Dispose();
        _loadUsersCancellationTokenSource = null;
        base.OnUnloaded(e);
    }

    private void RefreshTokenBox_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is AutoCompleteBox box)
            box.IsDropDownOpen = true;
    }

    private bool TryBeginLogin()
    {
        if (DataContext is not LoginPageViewModel viewModel)
            return false;

        viewModel.IsLoginInProgress = true;
        return true;
    }

    private void EndLogin()
    {
        if (DataContext is not LoginPageViewModel viewModel)
            return;

        viewModel.IsLoginInProgress = false;
    }
}
