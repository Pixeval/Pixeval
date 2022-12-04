#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/LoginPageViewModel.cs
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


using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Net;
using Pixeval.Misc;
using Pixeval.Storage;
using Pixeval.Util;
using Pixeval.Util.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Windows.System;

namespace Pixeval.Pages.Login;

internal partial class LoginPageViewModel : ObservableObject
{
    //private readonly TaskCompletionSource<string> _loginCompletionSource = new();
    //private readonly AbstractSessionStorage _sessionStorage;
    //private readonly SettingStorage _settingStorage;

    //public LoginPageViewModel(
    //    LoginPage loginPage,
    //    AbstractSessionStorage sessionStorage,
    //    SettingStorage settingStorage)
    //{
    //    LoginPage = loginPage;
    //    _settingStorage = settingStorage;
    //    _sessionStorage = sessionStorage;
    //    LoginPage.ViewModel = this;
    //    LoginPage.DataContext = this;
    //    LoginPage.InitializeComponent();
    //}

    //public LoginPage LoginPage { get; }

    //[RelayCommand]
    //private async Task NavigationStartingAsync(CoreWebView2NavigationStartingEventArgs args)
    //{
    //    if (args.Uri.StartsWith("pixiv://"))
    //    {
    //        _loginCompletionSource.SetResult(args.Uri);
    //    }
    //}

    //[RelayCommand]
    //private async Task StartLoginAsync()
    //{
    //    await EnsureWebView2IsInstalledAsync();
    //    if (await CheckFakeRootCertificateInstallationAsync())
    //    {
    //        await InstallFakeRootCertificateAsync();
    //    }
    //    await WebLoginAsync();
    //}



    //private async Task EnsureWebView2IsInstalledAsync()
    //{
    //    //if (!CheckWebView2Installation())
    //    //{
    //    //    var dialogResult = await MessageDialogBuilder.CreateOkCancel(LoginPage,
    //    //        LoginPageResources.WebView2InstallationRequiredTitle,
    //    //        LoginPageResources.WebView2InstallationRequiredContent).ShowAsync();
    //    //    if (dialogResult == ContentDialogResult.Primary)
    //    //    {
    //    //        await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
    //    //    }

    //    //    App.ExitWithPushedNotification();
    //    //}

    //    //static bool CheckWebView2Installation()
    //    //{
    //    //    try
    //    //    {
    //    //        CoreWebView2Environment.GetAvailableBrowserVersionString();
    //    //        return true;
    //    //    }
    //    //    catch
    //    //    {
    //    //        return false;
    //    //    }
    //    //}

    //}

    //private Task SetIgnoreCertificateErrorsAsync()
    //{
    //    return LoginPage.LoginWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Security.setIgnoreCertificateErrors", "{ \"ignore\": true }").AsTask();
    //}


    //public async Task<bool> CheckFakeRootCertificateInstallationAsync()
    //{
    //    using var cert = await ResourcesHelpers.GetFakeCaRootCertificateAsync();
    //    var fakeCertMgr = new CertificateManager(cert);
    //    return fakeCertMgr.Query(StoreName.Root, StoreLocation.CurrentUser);
    //}

    //public async Task InstallFakeRootCertificateAsync()
    //{
    //    using var cert = await ResourcesHelpers.GetFakeCaRootCertificateAsync();
    //    var fakeCertMgr = new CertificateManager(cert);
    //    fakeCertMgr.Install(StoreName.Root, StoreLocation.CurrentUser);
    //}

    //public async Task RefreshAsync()
    //{
    //    //if (_configurationManager.PixivApiSession is { } session && CheckRefreshAvailableInternal(session))
    //    //{
    //    //    _appViewModel.MakoClient = new PixivApiService(session, _configurationManager.Setting.ToMakoClientConfiguration(),
    //    //        new RefreshTokenSessionUpdater());
    //    //    await _appViewModel.MakoClient.GetAccessTokenAsync();
    //    //}
    //    //else
    //    //{
    //    //    await MessageDialogBuilder.CreateAcknowledgement(
    //    //            _appViewModel.Window,
    //    //            LoginPageResources.RefreshingSessionIsNotPresentTitle,
    //    //            LoginPageResources.RefreshingSessionIsNotPresentContent)
    //    //        .ShowAsync();
    //    //    Application.Current.Exit();
    //    //}
    //}

    //[RelayCommand]
    //public async Task WebLoginAsync()
    //{
    //    await LoginPage.LoginWebView.EnsureCoreWebView2Async();
    //    await SetIgnoreCertificateErrorsAsync();
    //    LoginPage.LoginWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
    //    LoginPage.LoginWebView.CoreWebView2.WebResourceRequested += (_, args) =>
    //    {
    //        args.Request.Headers.SetHeader("Accept-Language", args.Request.Uri.Contains("recaptcha") ? "zh-cn" : CultureInfo.CurrentUICulture.Name);
    //    };

    //    var verifier = PixivAuthSignature.GetCodeVerify();
    //    LoginPage.LoginWebView.Source = new Uri(PixivAuthSignature.GenerateWebPageUrl(verifier));

    //    var url = await _loginCompletionSource.Task;

    //    var cookies =
    //        await LoginPage.LoginWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://accounts.pixiv.net/");

    //    var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"]!;

    //    var tokenResponse = await AuthCodeToTokenAsync(code, verifier, cookies);
    //    await _sessionStorage.SetSessionAsync(tokenResponse.User!.Id!, tokenResponse.RefreshToken, tokenResponse.AccessToken);
    //}

    //public static async Task<TokenResponse?> AuthCodeToTokenAsync(string code, string verifier, IEnumerable<CoreWebView2Cookie> cookies)
    //{
    //    // HttpClient is designed to be used through whole application lifetime, create and
    //    // dispose it in a function is a commonly misused anti-pattern, but this function
    //    // is intended to be called only once (at the start time) during the entire application's
    //    // lifetime, so the overhead is acceptable
    //    using var httpClient = new HttpClient(new DelegatedHttpMessageHandler(PixivHttpOptions.CreateHttpMessageInvoker(new PixivApiNameResolver())));
    //    httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
    //    var result = await httpClient.PostFormAsync("http://oauth.secure.pixiv.net/auth/token",
    //        ("code", code),
    //        ("code_verifier", verifier),
    //        ("client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"),
    //        ("client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"),
    //        ("grant_type", "authorization_code"),
    //        ("include_policy", "true"),
    //        ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"));
    //    result.EnsureSuccessStatusCode();
    //    return await result.Content.ReadFromJsonAsync<TokenResponse>();
    //}
}