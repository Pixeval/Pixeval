// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Net;
using Pixeval.Pages.Login;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Windows.System;
using WinUIEx;

namespace Pixeval;

[LocalizedStringResources]
internal partial class LoginWindow
{
    private readonly TaskCompletionSource<string> _loginCompletionSource = new();
    private readonly TaskCompletionSource<TokenResponse> _loginTaskCompletionSource = new();

    public LoginWindow()
    {
        InitializeComponent();
        this.SetWindowSize(400, 600);
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(LoginWindowTitleBar);
        LoginWindowTitleBarText.Text = "Login";
        LoginWebView.NavigationStarting += LoginWebView_NavigationStarting;
    }

    public Task<TokenResponse> LoginTask => _loginTaskCompletionSource.Task;

    private void LoginWebView_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (args.Uri.StartsWith("pixiv://"))
        {
            _loginCompletionSource.SetResult(args.Uri);
        }
    }


    [RelayCommand]
    private async Task LoginAsync()
    {
        await EnsureWebView2IsInstalledAsync();
        await LoginWebView.EnsureCoreWebView2Async();
        await SetIgnoreCertificateErrorsAsync();
        LoginWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        LoginWebView.CoreWebView2.WebResourceRequested += (_, args) =>
        {
            args.Request.Headers.SetHeader("Accept-Language",
                args.Request.Uri.Contains("recaptcha") ? "zh-cn" : CultureInfo.CurrentUICulture.Name);
        };

        var verifier = PixivAuthSignature.GetCodeVerify();
        LoginWebView.Source = new Uri(PixivAuthSignature.GenerateWebPageUrl(verifier));

        var url = await _loginCompletionSource.Task;

        var cookies =
            await LoginWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://accounts.pixiv.net/");

        var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"]!;

        var tokenResponse = await AuthCodeToTokenAsync(code, verifier, cookies);
        _loginTaskCompletionSource.SetResult(tokenResponse!);

    }

    private async Task EnsureWebView2IsInstalledAsync()
    {
        if (!CheckWebView2Installation())
        {
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                DefaultButton = ContentDialogButton.Primary,
                Title = SR.WebView2InstallationRequiredTitle,
                Content = SR.WebView2InstallationRequiredContent,
                PrimaryButtonText = "确认",
                SecondaryButtonText = "取消"
            };
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    await Launcher.LaunchUriAsync(
                        new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
                    break;
                default:
                    App.ExitWithPushedNotification();
                    break;
            }
        }

        static bool CheckWebView2Installation()
        {
            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }

    private Task SetIgnoreCertificateErrorsAsync()
    {
        return LoginWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Security.setIgnoreCertificateErrors", "{ \"ignore\": true }").AsTask();
    }


    public static async Task<TokenResponse?> AuthCodeToTokenAsync(string code, string verifier, IEnumerable<CoreWebView2Cookie> cookies)
    {
        // HttpClient is designed to be used through whole application lifetime, create and
        // dispose it in a function is a commonly misused anti-pattern, but this function
        // is intended to be called only once (at the start time) during the entire application's
        // lifetime, so the overhead is acceptable
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
        var result = await httpClient.PostAsync("https://oauth.secure.pixiv.net/auth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"code", code},
                {"code_verifier", verifier},
                {"client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"},
                {"client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"},
                {"grant_type", "authorization_code"},
                {"include_policy", "true"},
                { "redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"}
            })
        );
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<TokenResponse>();
    }
}