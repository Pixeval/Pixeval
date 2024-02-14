#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LoginPageViewModel.cs
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Preference;
using Pixeval.Logging;
using Pixeval.Misc;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages.Login;

public partial class LoginPageViewModel(UIElement owner) : ObservableObject
{
    public enum LoginPhaseEnum
    {
        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingRefreshAvailable))]
        CheckingRefreshAvailable,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseRefreshing))]
        Refreshing,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseWaitingForUserInput))]
        WaitingForUserInput,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingCertificateInstallation))]
        CheckingCertificateInstallation,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseInstallingCertificate))]
        InstallingCertificate,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingWebView2Installation))]
        CheckingWebView2Installation,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseSuccessNavigating))]
        SuccessNavigating
    }

    public bool IsFinished { get; set; }

    [ObservableProperty] private bool _isEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProcessingRingVisible))]
    private LoginPhaseEnum _loginPhase;

    [ObservableProperty] private WebView2? _webView;

    public bool DisableDomainFronting
    {
        get => App.AppViewModel.AppSettings.DisableDomainFronting;
        set => App.AppViewModel.AppSettings.DisableDomainFronting = value;
    }

    public string UserName
    {
        get => App.AppViewModel.AppSettings.UserName;
        set => App.AppViewModel.AppSettings.UserName = value;
    }

    public string Password
    {
        get => App.AppViewModel.AppSettings.Password;
        set => App.AppViewModel.AppSettings.Password = value;
    }

    public Visibility ProcessingRingVisible => LoginPhase is LoginPhaseEnum.WaitingForUserInput ? Visibility.Collapsed : Visibility.Visible;

    public void AdvancePhase(LoginPhaseEnum newPhase)
    {
        LoginPhase = newPhase;
    }

    public bool CheckWebView2Installation()
    {
        AdvancePhase(LoginPhaseEnum.CheckingWebView2Installation);
        var regKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\{(Environment.Is64BitOperatingSystem ? @"WOW6432Node\" : "")}Microsoft\EdgeUpdate\Clients\{{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}}");
        return !string.IsNullOrEmpty(regKey?.GetValue("pv") as string);
    }

    public async Task<bool> CheckFakeRootCertificateInstallationAsync()
    {
        AdvancePhase(LoginPhaseEnum.CheckingCertificateInstallation);
        using var cert = await AppInfo.GetFakeCaRootCertificateAsync();
        var fakeCertMgr = new CertificateManager(cert);
        return fakeCertMgr.Query(StoreName.Root, StoreLocation.CurrentUser);
    }

    public async Task InstallFakeRootCertificateAsync()
    {
        AdvancePhase(LoginPhaseEnum.InstallingCertificate);
        using var cert = await AppInfo.GetFakeCaRootCertificateAsync();
        var fakeCertMgr = new CertificateManager(cert);
        fakeCertMgr.Install(StoreName.Root, StoreLocation.CurrentUser);
    }

    /// <summary>
    /// Check if the session file exists and satisfies the following four conditions: <br/>
    /// 1. The <see cref="Session" /> object deserialized from the file is not null <br/>
    /// 2. The <see cref="Session.RefreshToken" /> is not null <br/>
    /// 3. The <see cref="Session.Cookie" /> is not null <br/>
    /// 4. The <see cref="Session.CookieCreation" /> is within last fifteen days
    /// </summary>
    /// <returns></returns>
    public bool CheckRefreshAvailable()
    {
        AdvancePhase(LoginPhaseEnum.CheckingRefreshAvailable);

        return AppInfo.LoadSession() is { } session && CheckRefreshAvailableInternal(session);
    }

    private static bool CheckRefreshAvailableInternal(Session? session)
    {
        return session is not null && session.RefreshToken.IsNotNullOrEmpty() &&
               session.Cookie.IsNotNullOrEmpty() && CookieNotExpired(session);

        static bool CookieNotExpired(Session session) =>
            DateTimeOffset.Now - session.CookieCreation <=
            TimeSpan.FromDays(15); // check if the cookie is created within the last one week
    }

    public async Task RefreshAsync()
    {
        AdvancePhase(LoginPhaseEnum.Refreshing);
        if (AppInfo.LoadSession() is { } session && CheckRefreshAvailableInternal(session))
        {
            using var scope = App.AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger, new RefreshTokenSessionUpdate());
            await App.AppViewModel.MakoClient.RefreshSessionAsync();
        }
        else
        {
            _ = await owner.CreateAcknowledgementAsync(LoginPageResources.RefreshingSessionIsNotPresentTitle,
                    LoginPageResources.RefreshingSessionIsNotPresentContent);
            await AppKnownFolders.Local.ClearAsync();
            Application.Current.Exit();
        }
    }

    private static int NegotiatePort(int preferPort = 49152)
    {
        var unavailable = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(t => t.LocalEndPoint.Port).ToHashSet();
        if (unavailable.Contains(preferPort))
            for (var i = 49152; i <= ushort.MaxValue; i++)
            {
                if (!unavailable.Contains(i))
                {
                    preferPort = i;
                    break;
                }
            }

        return preferPort;
    }

    public async Task<Session> AuthCodeToSessionAsync(string code, string verifier, string cookie)
    {
        // HttpClient is designed to be used through whole application lifetime, create and
        // dispose it in a function is a commonly misused anti-pattern, but this function
        // is intended to be called only once (at the start time) during the entire application's
        // lifetime, so the overhead is acceptable

        // LoginProxyOption.SpecifyProxy => new HttpClientHandler { Proxy = new WebProxy { Address = new Uri(ProxyString) }, UseProxy = true },
        var handler = new DelegatedHttpMessageHandler(MakoHttpOptions.CreateHttpMessageInvoker(DisableDomainFronting
            ? new LocalMachineNameResolver()
            : new PixivApiNameResolver()));

        using var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
        var result = await httpClient.PostFormAsync("http://oauth.secure.pixiv.net/auth/token",
            ("code", code),
            ("code_verifier", verifier),
            ("client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"),
            ("client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"),
            ("grant_type", "authorization_code"),
            ("include_policy", "true"),
            ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"));
        _ = result.EnsureSuccessStatusCode();
        var session = (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession() with
        {
            Cookie = cookie,
            CookieCreation = DateTimeOffset.Now
        };
        return session;
    }

    public async Task WebView2LoginAsync(UserControl userControl, Action navigated)
    {
        var remoteDebuggingPort = NegotiatePort(9222);
        var arguments = $"--remote-debugging-port={remoteDebuggingPort}";
        var port = NegotiatePort();

        var proxyServer = null as PixivAuthenticationProxyServer;
        if (!DisableDomainFronting)
        {
            await EnsureCertificateIsInstalled(userControl);
            proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port,
                await AppInfo.GetFakeServerCertificateAsync());
            arguments += $" --ignore-certificate-errors --proxy-server=127.0.0.1:{port}";
        }

        await EnsureWebView2IsInstalled(userControl);
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", arguments);
        WebView = new() { Visibility = Visibility.Visible };
        await WebView.EnsureCoreWebView2Async();
        var playWrightHelper = new PlayWrightHelper(remoteDebuggingPort);
        var verifier = PixivAuthSignature.GetCodeVerify();
        IsEnabled = IsFinished = false;
        await playWrightHelper.Initialize();

        var page = playWrightHelper.Page;
        page.Request += async (o, e) =>
        {
            if (!IsFinished && e.Url.Contains("code="))
            {
                // 成功时不需要让控件IsEnabled
                _ = userControl.DispatcherQueue.TryEnqueue(() => IsFinished = true);
                var code = HttpUtility.ParseQueryString(new Uri(e.Url).Query)["code"]!;
                var cookies = await page.Context.CookiesAsync(["https://pixiv.net"]);
                var cookie = string.Join(';', cookies.Select(c => $"{c.Name}={c.Value}"));
                var session = await AuthCodeToSessionAsync(code, verifier, cookie);
                using var scope = App.AppViewModel.AppServicesScope;
                var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
                App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger, new RefreshTokenSessionUpdate());
                await playWrightHelper.DisposeAsync();
                proxyServer?.Dispose();
                navigated();
            }
        };
        try
        {
            _ = await page.GotoAsync(PixivAuthSignature.GenerateWebPageUrl(verifier));
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            const string prefix = "//html/body/div[1]/div/div/div[4]/div[1]/div";
            const string prefix2 = prefix + "[2]/div/div/div/form/";
            var buttonLocator = page.Locator(prefix + "[1]/div[5]/button[2]");
            var userNameLocator = page.Locator(prefix2 + "fieldset[1]/label/input");
            var passwordLocator = page.Locator(prefix2 + "fieldset[2]/label/input");
            var submitLocator = page.Locator(prefix2 + "button");
            if (await buttonLocator.CountAsync() is not 0)
            {
                await buttonLocator.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            }

            if (await userNameLocator.CountAsync() is not 0
                && await passwordLocator.CountAsync() is not 0
                && await submitLocator.CountAsync() is not 0)
            {
                await userNameLocator.FillAsync(UserName);
                await passwordLocator.FillAsync(Password);
                await submitLocator.ClickAsync();
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            if (IsFinished || !await submitLocator.IsEnabledAsync())
            {
                // 需要输入reCAPTCHA
            }
        }
        catch (PlaywrightException)
        {
            // 可能还没加载完页面就登录成功跳转了，导致异常
            if (!IsFinished)
                ;
            // visible
        }
    }

    private async Task EnsureCertificateIsInstalled(UIElement userControl)
    {
        if (!await CheckFakeRootCertificateInstallationAsync())
        {
            var dialogResult = await userControl.CreateOkCancelAsync(LoginPageResources.RootCertificateInstallationRequiredTitle,
                LoginPageResources.RootCertificateInstallationRequiredContent);
            if (dialogResult is ContentDialogResult.Primary)
            {
                await InstallFakeRootCertificateAsync();
            }
            else
            {
                Application.Current.Exit();
            }
        }
    }

    private async Task EnsureWebView2IsInstalled(UIElement userControl)
    {
        if (!CheckWebView2Installation())
        {
            var dialogResult = await userControl.CreateOkCancelAsync(LoginPageResources.WebView2InstallationRequiredTitle,
                LoginPageResources.WebView2InstallationRequiredContent);
            if (dialogResult is ContentDialogResult.Primary)
            {
                _ = await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
            }

            Application.Current.Exit();
        }
    }
}
