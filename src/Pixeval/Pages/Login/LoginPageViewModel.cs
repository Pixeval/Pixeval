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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Windowing;
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

public partial class LoginPageViewModel(UIElement owner) : ObservableObject, IDisposable
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
    /// 4. The <see cref="Session.CookieCreation" /> is within last 15 days
    /// </summary>
    /// <returns></returns>
    public Session? CheckRefreshAvailable()
    {
        AdvancePhase(LoginPhaseEnum.CheckingRefreshAvailable);

        return AppInfo.LoadSession() is { } session && CheckRefreshAvailableInternal(session) ? session : null;
    }

    private static bool CheckRefreshAvailableInternal(Session? session)
    {
        return session is not null && session.RefreshToken.IsNotNullOrEmpty() &&
               session.Cookie.IsNotNullOrEmpty() && CookieNotExpired(session);

        static bool CookieNotExpired(Session session) =>
            DateTimeOffset.Now - session.CookieCreation <=
            TimeSpan.FromDays(15); // check if the cookie is created within the last one week
    }

    public async Task<bool> RefreshAsync(Session session)
    {
        AdvancePhase(LoginPhaseEnum.Refreshing);
        using var scope = App.AppViewModel.AppServicesScope;
        var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
        App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger, new RefreshTokenSessionUpdate());
        try
        {
            await App.AppViewModel.MakoClient.RefreshSessionAsync();
        }
        catch
        {
            _ = await owner.CreateAcknowledgementAsync(LoginPageResources.RefreshingSessionFailedTitle,
                LoginPageResources.RefreshingSessionFailedContent);
            AppInfo.ClearSession();
            await App.AppViewModel.MakoClient.DisposeAsync();
            App.AppViewModel.MakoClient = null!;
            return false;
        }

        return true;
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

        var httpClient = DisableDomainFronting ? new() : new HttpClient(new DelegatedHttpMessageHandler(MakoHttpOptions.CreateDirectHttpMessageInvoker()));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new("PixivAndroidApp", "5.0.64"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new("(Android 6.0)"));
        var scheme = DisableDomainFronting ? "https" : "http";

        using var result = await httpClient.PostFormAsync(scheme + "://oauth.secure.pixiv.net/auth/token",
            ("code", code),
            ("code_verifier", verifier),
            ("client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"),
            ("client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"),
            ("grant_type", "authorization_code"),
            ("include_policy", "true"),
            ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"));
        // using会有resharper警告，所以这里用Dispose
        httpClient.Dispose();
        _ = result.EnsureSuccessStatusCode();
        var session = (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession() with
        {
            Cookie = cookie,
            CookieCreation = DateTimeOffset.Now
        };
        return session;
    }

    public async Task WebView2LoginAsync(UserControl userControl, bool useNewAccount, Action navigated)
    {
        var arguments = "";
        var port = NegotiatePort();

        var proxyServer = null as PixivAuthenticationProxyServer;
        if (!DisableDomainFronting)
        {
            if (!await EnsureCertificateIsInstalled(userControl))
                return;
            proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port,
                await AppInfo.GetFakeServerCertificateAsync());
            arguments += $" --ignore-certificate-errors --proxy-server=127.0.0.1:{port}";
        }

        if (!await EnsureWebView2IsInstalled(userControl))
            return;
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", arguments);
        WebView = new();
        await WebView.EnsureCoreWebView2Async();
        IsEnabled = IsFinished = false;
        var verifier = PixivAuthSignature.GetCodeVerify();
        WebView.NavigationStarting += async (sender, e) =>
        {
            if (e.Uri.StartsWith("pixiv://"))
            {
                var cookies = await sender.CoreWebView2.CookieManager.GetCookiesAsync("https://pixiv.net");
                var cookie = string.Join(';', cookies.Select(c => $"{c.Name}={c.Value}"));
                var code = HttpUtility.ParseQueryString(new Uri(e.Uri).Query)["code"]!;
                Session session;
                try
                {
                    session = await AuthCodeToSessionAsync(code, verifier, cookie);
                }
                catch
                {
                    _ = await owner.CreateAcknowledgementAsync(LoginPageResources.FetchingSessionFailedTitle,
                        LoginPageResources.FetchingSessionFailedContent);
                    CloseWindow();
                    return;
                }
                IsFinished = true;
                using var scope = App.AppViewModel.AppServicesScope;
                var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
                App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger, new RefreshTokenSessionUpdate());
                proxyServer?.Dispose();
                navigated();
            }
            else if (e.Uri.Contains("accounts.pixiv.net"))
            {
                _ = await sender.ExecuteScriptAsync(
                    $$"""
                      async function login(event) {
                          async function checkElement(selector) {
                              const targetElement = document.querySelector(selector);
                              if (targetElement) {
                                  return targetElement;
                              } else {
                                  await new Promise((resolve) => setTimeout(resolve, 100));
                                  return await checkElement(selector);
                              }
                          }
                      
                          async function fill(selector, value)
                          {
                              const input = (await checkElement(selector));
                              input.value = value;
                              input._valueTracker.setValue("");
                              const ev = new Event("input", { bubbles: true });
                              input.dispatchEvent(ev);
                          }
                      
                          if ((await checkElement("button")) && document.querySelectorAll("button").length === 3) {
                              (await checkElement("button:nth-child({{(useNewAccount ? 2 : 1)}})")).click();
                          }
                          {{(UserName != "" && Password != "" ?
                    $$"""
                          else {
                              await fill("input[autocomplete='username']", "{{UserName}}");
                              await fill("input[autocomplete='current-password']", "{{Password}}");
                              document.querySelectorAll("button[type='submit']")[4].click();
                          }
                      """
                              : "")}}
                      }
                      if (document.readyState === "loading") {
                          document.addEventListener("DOMContentLoaded", login);
                      } else {
                          login(null);
                      }
                      """);
            }
        };
        WebView.Source = new Uri(PixivAuthSignature.GenerateWebPageUrl(verifier));
    }

    private async Task<bool> EnsureCertificateIsInstalled(UIElement userControl)
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
                CloseWindow();
                return false;
            }
        }
        return true;
    }

    private async Task<bool> EnsureWebView2IsInstalled(UIElement userControl)
    {
        if (!CheckWebView2Installation())
        {
            var dialogResult = await userControl.CreateOkCancelAsync(LoginPageResources.WebView2InstallationRequiredTitle,
                LoginPageResources.WebView2InstallationRequiredContent);
            if (dialogResult is ContentDialogResult.Primary)
            {
                _ = await Launcher.LaunchUriAsync(new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
            }

            CloseWindow();
            return false;
        }

        return true;
    }

    public void CloseWindow() => WindowFactory.GetWindowForElement(owner).Close();

    /// <summary>
    /// 退出App时关闭<see cref="WebView"/>可以保证不抛异常
    /// </summary>
    public void Dispose()
    {
        WebView?.Close();
        WebView = null;
    }
}
