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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
// using System.Reflection;
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
using Pixeval.Controls.DialogContent;
// using Pixeval.Bypass;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Preference;
using Pixeval.Logging;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Login;

[SettingsViewModel<LoginContext>(nameof(LoginContext))]
public partial class LoginPageViewModel(UIElement owner) : ObservableObject
{
    /// <summary>
    /// 表示要不要展示<see cref="WebView"/>
    /// </summary>
    [ObservableProperty] private bool _isFinished = true;

    /// <summary>
    /// 表示右侧按钮是否可用
    /// </summary>
    [ObservableProperty] private bool _isEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProcessingRingVisible))]
    private LoginPhaseEnum _loginPhase;

    public LoginContext LoginContext => App.AppViewModel.LoginContext;

    public bool EnableDomainFronting
    {
        get => App.AppViewModel.AppSettings.EnableDomainFronting;
        set => App.AppViewModel.AppSettings.EnableDomainFronting = value;
    }

    public Visibility ProcessingRingVisible => LoginPhase is LoginPhaseEnum.WaitingForUserInput ? Visibility.Collapsed : Visibility.Visible;

    public void AdvancePhase(LoginPhaseEnum newPhase) => LoginPhase = newPhase;

    public void CloseWindow() => WindowFactory.GetWindowForElement(owner).Close();

    #region Token (Common use)

    public async Task<bool> RefreshAsync(string refreshToken)
    {
        AdvancePhase(LoginPhaseEnum.Refreshing);
        var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
        var client = await MakoClient.TryGetMakoClientAsync(refreshToken, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger);
        if (client is not null)
        {
            App.AppViewModel.MakoClient = client;
            RefreshToken = client.Session.RefreshToken;
            return true;
        }

        _ = await owner.CreateAcknowledgementAsync(LoginPageResources.RefreshingSessionFailedTitle,
            LoginPageResources.RefreshingSessionFailedContent);
        return false;
    }

    #endregion

    #region WebView

    [ObservableProperty] private WebView2? _webView;

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
        return cert.Query(StoreName.Root, StoreLocation.CurrentUser);
    }

    public async Task InstallFakeRootCertificateAsync()
    {
        AdvancePhase(LoginPhaseEnum.InstallingCertificate);
        using var cert = await AppInfo.GetFakeCaRootCertificateAsync();
        cert.Install(StoreName.Root, StoreLocation.CurrentUser);
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

    public async Task WebView2LoginAsync(ulong hWnd, bool useNewAccount, Action navigated)
    {
        var arguments = "";
        var port = NegotiatePort();

        var proxyServer = null as PixivAuthenticationProxyServer;
        if (EnableDomainFronting)
        {
            if (await EnsureCertificateIsInstalled(hWnd) is not { } cert)
                return;
            proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port, cert);
            arguments += $" --ignore-certificate-errors --proxy-server=127.0.0.1:{port}";
        }

        if (!await EnsureWebView2IsInstalled(hWnd))
            return;
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", arguments);
        WebView = new();
        await WebView.EnsureCoreWebView2Async();
        IsEnabled = IsFinished = false;
        var verifier = PixivAuth.GetCodeVerify();
        WebView.NavigationStarting += async (sender, e) =>
        {
            if (e.Uri.StartsWith("pixiv://"))
            {
                IsFinished = true;
                var code = HttpUtility.ParseQueryString(new Uri(e.Uri).Query)["code"]!;
                Session session;
                try
                {
                    session = await PixivAuth.AuthCodeToSessionAsync(code, verifier);
                    proxyServer?.Dispose();
                }
                catch
                {
                    _ = await owner.CreateAcknowledgementAsync(LoginPageResources.FetchingSessionFailedTitle,
                        LoginPageResources.FetchingSessionFailedContent);
                    CloseWindow();
                    return;
                }
                var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
                App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger);
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
                              await fill("form>fieldset:nth-child(2)>label>input", "{{UserName}}");
                              await fill("form>fieldset:nth-child(3)>label>input", "{{Password}}");
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
        WebView.Source = new Uri(PixivAuth.GenerateWebPageUrl(verifier));
    }

    private async Task<X509Certificate2?> EnsureCertificateIsInstalled(ulong hWnd)
    {
        if (!await CheckFakeRootCertificateInstallationAsync())
        {
            var content = new CertificateRequiredDialog();

            var cd = hWnd.CreateContentDialog(
                LoginPageResources.RootCertificateInstallationRequiredTitle,
                content,
                LoginPageResources.RootCertificateInstallationRequiredPrimaryButtonText,
                LoginPageResources.RootCertificateInstallationRequiredSecondaryButtonText,
                MessageContentDialogResources.CancelButtonContent);
            cd.PrimaryButtonClick += (_, e) => e.Cancel = !content.CheckCertificate();
            var dialogResult = await cd.ShowAsync();

            switch (dialogResult)
            {
                case ContentDialogResult.Primary:
                    return content.X509Certificate2;
                case ContentDialogResult.Secondary:
                    await InstallFakeRootCertificateAsync();
                    break;
                default:
                    return null;
            }
        }
        return await AppInfo.GetFakeServerCertificateAsync();
    }

    private async Task<bool> EnsureWebView2IsInstalled(ulong hWnd)
    {
        if (!CheckWebView2Installation())
        {
            var dialogResult = await hWnd.CreateOkCancelAsync(LoginPageResources.WebView2InstallationRequiredTitle,
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

    /// <summary>
    /// 退出App时关闭<see cref="WebView"/>可以保证不抛异常
    /// </summary>
    public void Dispose()
    {
        WebView?.Close();
        WebView = null;
    }

    #endregion

    #region Browser

    private static string ChooseBrowser()
    {
        using var startMenuInternetKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
        var browsers = new Dictionary<string, string>();
        if (startMenuInternetKey != null)
        {
            var subKeyNames = startMenuInternetKey.GetSubKeyNames();
            foreach (var subKeyName in subKeyNames)
            {
                using var subKey =
                    startMenuInternetKey.OpenSubKey($@"{subKeyName}\Capabilities\URLAssociations");
                var httpsValue = subKey?.GetValue("https");
                var shellKey = startMenuInternetKey.OpenSubKey($@"{subKeyName}\shell\open\command");
                var location = shellKey?.GetValue(null);
                if (httpsValue is string httpValueString)
                {
                    browsers[httpValueString] = (string)location!;
                }
            }
        }
        browsers = browsers.Join(["ChromeHTML", "MSEdgeHTM"], outer => outer.Key, inner => inner, ((pair, s) => pair))
            .ToDictionary();
        var userChoiceKey = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");
        if (userChoiceKey?.GetValue("ProgId") is not string id || !browsers.TryGetValue(id, out var ret))
        {
            return browsers.Values.First();
        }

        return ret;
    }

    public void BrowserLogin()
    {
        var verifier = PixivAuth.GetCodeVerify();
        var url = PixivAuth.GenerateWebPageUrl(verifier);
        var browserPath = ChooseBrowser();
        var userDataDir = Path.Combine(Path.GetTempPath(), "Pixeval", "browser-user-data");
        var commonArgs = $"--disable-sync --no-default-browser-check --no-first-run --user-data-dir={userDataDir} {url}";
        var startInfo = new ProcessStartInfo(browserPath)
        {
            Arguments = EnableDomainFronting ? $"--no-proxy-server --dns-prefetch-disable {commonArgs}" : $"{commonArgs}"
        };
        var process = Process.Start(startInfo);
        if (EnableDomainFronting)
        {
            //    var pid = process!.Id;
            //    var dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "runtimes",
            //        "win-x64", "native", "bypass.dll");
            //    var injection = Injector.Inject((uint)pid, dllPath);
            //    Injector.InstallChromeHook(injection, true, dllPath);
        }
    }

    #endregion
}

[LocalizationMetadata(typeof(LoginPageResources))]
public enum LoginPhaseEnum
{
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
