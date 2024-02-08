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
using System.Diagnostics;
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
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

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

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseExecutingBrowser))]
        ExecutingBrowser,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingCertificateInstallation))]
        CheckingCertificateInstallation,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseInstallingCertificate))]
        InstallingCertificate,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingWebView2Installation))]
        CheckingWebView2Installation,

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseSuccessNavigating))]
        SuccessNavigating
    }

    [ObservableProperty] private bool _isFinished;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowProcessingRing))]
    private LoginPhaseEnum _loginPhase;

    [ObservableProperty] private WebView2? _webView;

    public LoginProxyOption LoginProxyOption
    {
        get => App.AppViewModel.AppSetting.LoginProxyOption;
        set => App.AppViewModel.AppSetting.LoginProxyOption = value;
    }

    public string ProxyString
    {
        get => App.AppViewModel.AppSetting.ProxyString;
        set => App.AppViewModel.AppSetting.ProxyString = value;
    }

    public Visibility ShowProcessingRing => LoginPhase is LoginPhaseEnum.WaitingForUserInput ? Visibility.Collapsed : Visibility.Visible;

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

        return AppContext.LoadSession() is { } session && CheckRefreshAvailableInternal(session);
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
        if (AppContext.LoadSession() is { } session && CheckRefreshAvailableInternal(session))
        {
            using var scope = App.AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSetting.ToMakoClientConfiguration(), logger,
                new RefreshTokenSessionUpdate());
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
        _ = result.EnsureSuccessStatusCode();
        var session = (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession() with
        {
            Cookie = cookie,
            CookieCreation = DateTimeOffset.Now
        };
        return session;
    }

    public Task<string?> BrowserLoginAsync(BrowserInfo info, UserControl userControl, Action navigated)
    {
        return LoginAsync(info.Type,
            userControl,
            arguments =>
            {
                if (!info.IsAvailable)
                    return Task.FromResult<string?>(LoginPageResources.BrowserNotFound);
                AdvancePhase(LoginPhaseEnum.ExecutingBrowser);
                _ = Process.Start(info.BrowserPath, arguments);
                return Task.FromResult<string?>(null);
            },
            navigated);
    }

    public Task<string?> WebView2LoginAsync(UserControl userControl, Action navigated)
    {
        return LoginAsync(AvailableBrowserType.WebView2,
            userControl,
            async arguments =>
            {
                await EnsureWebView2IsInstalled(userControl);
                Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", arguments);
                WebView = new WebView2();
                await WebView.EnsureCoreWebView2Async();
                return null;
            },
            navigated);
    }

    private async Task<string?> LoginAsync(AvailableBrowserType type, UserControl userControl, Func<string, Task<string?>> init, Action final)
    {
        var remoteDebuggingPort = NegotiatePort(9222);
        var arguments = $"--remote-debugging-port={remoteDebuggingPort}";
        var port = NegotiatePort();

        var proxyServer = null as PixivAuthenticationProxyServer;
        switch (LoginProxyOption)
        {
            case LoginProxyOption.UseDirect:
                await EnsureCertificateIsInstalled(userControl);
                proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port, await AppContext.GetFakeServerCertificateAsync());
                arguments += $" --ignore-certificate-errors --proxy-server=127.0.0.1:{port}";
                break;
            case LoginProxyOption.SpecifyProxy:
                var lastIndexOf = ProxyString.LastIndexOf(':');
                if (lastIndexOf == -1)
                    return LoginPageResources.IncorrectSocketFormat;
                var ip = ProxyString[..lastIndexOf];
                var portString = ProxyString[(lastIndexOf + 1)..];
                if (IPAddress.TryParse(ip, out _) && ushort.TryParse(portString, out _))
                    arguments += $" --proxy-server={ProxyString}";
                else
                    return LoginPageResources.IncorrectSocketFormat;
                break;
        }
        if (await init(arguments) is { } error)
            return error;
        var playWrightHelper = new PlayWrightHelper(type, remoteDebuggingPort);
        var verifier = PixivAuthSignature.GetCodeVerify();
        IsFinished = false;
        try
        {
            await playWrightHelper.Initialize();
            playWrightHelper.Browser.Disconnected += async (_, _) =>
            {
                if (IsFinished)
                    return;
                await playWrightHelper.DisposeAsync();
                proxyServer?.Dispose();
                _ = userControl.DispatcherQueue.TryEnqueue(() =>
                {
                    IsFinished = true;
                    AdvancePhase(LoginPhaseEnum.WaitingForUserInput);
                    _ = userControl.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle, LoginPageResources.BrowserConnnectionLost);
                });
            };
        }
        catch (PlaywrightException)
        {
            IsFinished = true;
            return LoginPageResources.TryAfterShuttingDownTheBrowser;
        }

        var page = playWrightHelper.Page;
        page.Request += async (o, e) =>
        {
            if (!IsFinished && e.Url.Contains("code="))
            {
                // 成功时不需要让控件IsEnabled
                // _ = userControl.DispatcherQueue.TryEnqueue(() => IsFinished = true);
                var code = HttpUtility.ParseQueryString(new Uri(e.Url).Query)["code"]!;
                var cookies = await page.Context.CookiesAsync(["https://pixiv.net"]);
                var cookie = string.Join(';', cookies.Select(c => $"{c.Name}={c.Value}"));
                var session = await AuthCodeToSessionAsync(code, verifier, cookie);
                using var scope = App.AppViewModel.AppServicesScope;
                var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
                App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSetting.ToMakoClientConfiguration(), logger, new RefreshTokenSessionUpdate());
                await playWrightHelper.DisposeAsync();
                proxyServer?.Dispose();
                final();
            }
        };
        try
        {
            _ = await page.GotoAsync(PixivAuthSignature.GenerateWebPageUrl(verifier));
        }
        catch (PlaywrightException)
        {
            // 可能还没加载完页面就登录成功跳转了，导致异常
            // if (!IsFinished)
            //     throw;
        }

        return null;
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
