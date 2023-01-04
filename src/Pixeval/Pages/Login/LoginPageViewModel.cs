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

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Preference;
using Pixeval.Misc;
using Pixeval.Popups;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Login;

public partial class LoginPageViewModel : AutoActivateObservableRecipient
{
    public enum LoginPhaseEnum
    {
        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingRefreshAvailable))]
        CheckingRefreshAvailable,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseRefreshing))]
        Refreshing,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseNegotiatingPort))]
        NegotiatingPort,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseExecutingWebView2))]
        ExecutingWebView2,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingCertificateInstallation))]
        CheckingCertificateInstallation,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseInstallingCertificate))]
        InstallingCertificate,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingWebView2Installation))]
        CheckingWebView2Installation
    }

    [ObservableProperty]
    private LoginPhaseEnum _loginPhase;

    public void AdvancePhase(LoginPhaseEnum newPhase)
    {
        LoginPhase = newPhase;
    }

    public bool CheckWebView2Installation()
    {
        AdvancePhase(LoginPhaseEnum.CheckingWebView2Installation);
        var regKey = Registry.LocalMachine.OpenSubKey(
            Environment.Is64BitOperatingSystem
                ? @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
                : @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
        return regKey != null && !(regKey.GetValue("pv") as string).IsNullOrEmpty();
    }

    public async Task<bool> CheckFakeRootCertificateInstallationAsync()
    {
        AdvancePhase(LoginPhaseEnum.CheckingCertificateInstallation);
        using var cert = await AppContext.GetFakeCaRootCertificateAsync();
        var fakeCertMgr = new CertificateManager(cert);
        return fakeCertMgr.Query(StoreName.Root, StoreLocation.CurrentUser);
    }

    public async Task InstallFakeRootCertificateAsync()
    {
        AdvancePhase(LoginPhaseEnum.InstallingCertificate);
        using var cert = await AppContext.GetFakeCaRootCertificateAsync();
        var fakeCertMgr = new CertificateManager(cert);
        fakeCertMgr.Install(StoreName.Root, StoreLocation.CurrentUser);
    }

    /// <summary>
    ///     Check if the session file exists and satisfies the following four conditions: <br></br>
    ///     1. The <see cref="Session" /> object deserialized from the file is not null <br></br>
    ///     2. The <see cref="Session.RefreshToken" /> is not null <br></br>
    ///     3. The <see cref="Session.Cookie" /> is not null <br></br>
    ///     4. The <see cref="Session.CookieCreation" /> is within last fifteen days <br></br>
    /// </summary>
    /// <returns></returns>
    public bool CheckRefreshAvailable()
    {
        AdvancePhase(LoginPhaseEnum.CheckingRefreshAvailable);

        return AppContext.LoadSession() is { } session && CheckRefreshAvailableInternal(session);
    }

    private static bool CheckRefreshAvailableInternal(Session? session)
    {
        return session is not null && session.RefreshToken.IsNotNullOrEmpty() &&
               session.Cookie.IsNotNullOrEmpty() && CookieNotExpired(session);

        static bool CookieNotExpired(Session session)
        {
            return DateTimeOffset.Now - session.CookieCreation <=
                   TimeSpan.FromDays(15); // check if the cookie is created within the last one week
        }
    }

    public async Task RefreshAsync()
    {
        AdvancePhase(LoginPhaseEnum.Refreshing);
        if (AppContext.LoadSession() is { } session && CheckRefreshAvailableInternal(session))
        {
            App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSetting.ToMakoClientConfiguration(),
                new RefreshTokenSessionUpdate());
            await App.AppViewModel.MakoClient.RefreshSessionAsync();
        }
        else
        {
            await MessageDialogBuilder.CreateAcknowledgement(
                    App.AppViewModel.Window,
                    LoginPageResources.RefreshingSessionIsNotPresentTitle,
                    LoginPageResources.RefreshingSessionIsNotPresentContent)
                .ShowAsync();
            await AppKnownFolders.Local.ClearAsync();
            Application.Current.Exit();
        }
    }

    private static int NegotiatePort()
    {
        var unavailable = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(t => t.LocalEndPoint.Port).ToArray();
        var rd = new Random();
        var proxyPort = rd.Next(3000, 65536);
        while (Array.BinarySearch(unavailable, proxyPort) >= 0)
        {
            proxyPort = rd.Next(3000, 65536);
        }

        return proxyPort;
    }

    public async Task WebLoginAsync()
    {
        AdvancePhase(LoginPhaseEnum.NegotiatingPort);
        var port = NegotiatePort();
        using var proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port, await AppContext.GetFakeServerCertificateAsync());
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", $"--proxy-server=127.0.0.1:{port} --ignore-certificate-errors");

        var content = new LoginWebViewPopup();
        var webViewPopup = PopupManager.CreatePopup(content, widthMargin: 150, heightMargin: 100, minHeight: 400, minWidth: 600);

        AdvancePhase(LoginPhaseEnum.ExecutingWebView2);
        PopupManager.ShowPopup(webViewPopup);

        await content.LoginWebView.EnsureCoreWebView2Async();
        content.LoginWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        content.LoginWebView.CoreWebView2.WebResourceRequested += (_, args) =>
        {
            args.Request.Headers.SetHeader("Accept-Language", args.Request.Uri.Contains("recaptcha") ? "zh-cn" : CultureInfo.CurrentUICulture.Name);
        };

        var verifier = PixivAuthSignature.GetCodeVerify();
        content.LoginWebView.Source = new Uri(PixivAuthSignature.GenerateWebPageUrl(verifier));

        var (url, cookie) = await content.CookieCompletion.Task;
        PopupManager.ClosePopup(webViewPopup);

        var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"]!;

        var session = await AuthCodeToSessionAsync(code, verifier, cookie);
        App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSetting.ToMakoClientConfiguration(), new RefreshTokenSessionUpdate());
    }

    public static async Task<Session> AuthCodeToSessionAsync(string code, string verifier, string cookie)
    {
        // HttpClient is designed to be used through whole application lifetime, create and
        // dispose it in a function is a commonly misused anti-pattern, but this function
        // is intended to be called only once (at the start time) during the entire application's
        // lifetime, so the overhead is acceptable
        using var httpClient = new HttpClient(new DelegatedHttpMessageHandler(MakoHttpOptions.CreateHttpMessageInvoker(new PixivApiNameResolver())));
        httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
        var result = await httpClient.PostFormAsync("http://oauth.secure.pixiv.net/auth/token",
            ("code", code),
            ("code_verifier", verifier),
            ("client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"),
            ("client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"),
            ("grant_type", "authorization_code"),
            ("include_policy", "true"),
            ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"));
        result.EnsureSuccessStatusCode();
        var session = (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession() with
        {
            Cookie = cookie,
            CookieCreation = DateTimeOffset.Now
        };
        return session;
    }
}