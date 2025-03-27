// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.DialogContent;
using Pixeval.Controls.Windowing;
using Mako;
using Pixeval.Logging;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.UI;
using Windows.System;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Login;

[SettingsViewModel<LoginContext>(nameof(LoginContext))]
public partial class LoginPageViewModel(FrameworkElement frameworkElement) : UiObservableObject(frameworkElement)
{
    /// <summary>
    /// 表示要不要展示<see cref="WebView"/>
    /// </summary>
    [ObservableProperty]
    public partial bool IsFinished { get; set; } = true;

    /// <summary>
    /// 表示右侧按钮是否可用
    /// </summary>
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProcessingRingVisible))]
    public partial LoginPhaseEnum LoginPhase { get; set; }

    public LoginContext LoginContext => App.AppViewModel.LoginContext;

    public bool EnableDomainFronting
    {
        get => App.AppViewModel.AppSettings.EnableDomainFronting;
        set => App.AppViewModel.AppSettings.EnableDomainFronting = value;
    }

    public Visibility ProcessingRingVisible => LoginPhase is LoginPhaseEnum.WaitingForUserInput ? Visibility.Collapsed : Visibility.Visible;

    public void AdvancePhase(LoginPhaseEnum newPhase) => LoginPhase = newPhase;

    public void CloseWindow() => Window.Close();

    #region WebView

    [ObservableProperty]
    public partial WebView2? WebView { get; set; }

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
            for (var i = 49152; i <= ushort.MaxValue; ++i)
            {
                if (!unavailable.Contains(i))
                {
                    preferPort = i;
                    break;
                }
            }

        return preferPort;
    }

    public async Task WebView2LoginAsync(EnhancedWindow window, bool useNewAccount, Action navigated)
    {
        var arguments = "";
        var port = NegotiatePort();

        var proxyServer = null as PixivAuthenticationProxyServer;
        if (EnableDomainFronting)
        {
            if (await EnsureCertificateIsInstalled() is not { } cert)
                return;
            proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port, cert);
            arguments += $" --ignore-certificate-errors --proxy-server=127.0.0.1:{port}";
        }

        if (!await EnsureWebView2IsInstalled())
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
                try
                {
                    if (await PixivAuth.AuthCodeToTokenResponseAsync(code, verifier) is not { } tokenResponse)
                    {
                        ThrowHelper.Exception();
                        return;
                    }
                    var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
                    App.AppViewModel.MakoClient = new MakoClient(tokenResponse, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger);
                    navigated();
                }
                catch
                {
                    _ = await FrameworkElement.CreateAcknowledgementAsync(LoginPageResources.FetchingSessionFailedTitle,
                        LoginPageResources.FetchingSessionFailedContent);
                    CloseWindow();
                }
                finally
                {
                    proxyServer?.Dispose();
                }
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

    private async Task<X509Certificate2?> EnsureCertificateIsInstalled()
    {
        if (!await CheckFakeRootCertificateInstallationAsync())
        {
            var content = new CertificateRequiredDialog();

            var cd = FrameworkElement.CreateDialog(
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

    private async Task<bool> EnsureWebView2IsInstalled()
    {
        if (!CheckWebView2Installation())
        {
            var dialogResult = await FrameworkElement.CreateOkCancelAsync(LoginPageResources.WebView2InstallationRequiredTitle,
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

    public string BrowserLogin()
    {
        var verifier = PixivAuth.GetCodeVerify();
        var url = PixivAuth.GenerateWebPageUrl(verifier);
        _ = Launcher.LaunchUriAsync(new Uri(url));
        return verifier;
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
