// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Windowing;
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

    private static int NegotiatePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint) listener.LocalEndpoint).Port;
        return port;
    }

    public async Task WebView2LoginAsync(EnhancedWindow window, bool useNewAccount, Action navigated)
    {
        if (!await EnsureWebView2IsInstalled())
            return;
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
                    App.AppViewModel.MakoClient.SetCode(code, verifier);
                    if (!await App.AppViewModel.MakoClient.IdentifyTokenAsync())
                    {
                        ThrowHelper.Exception();
                        return;
                    }
                    navigated();
                }
                catch
                {
                    _ = await FrameworkElement.CreateAcknowledgementAsync(LoginPageResources.FetchingSessionFailedTitle,
                        LoginPageResources.FetchingSessionFailedContent);
                    CloseWindow();
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
    [LocalizedResource(nameof(LoginPageResources.LoginPhaseRefreshing))]
    Refreshing,

    [LocalizedResource(nameof(LoginPageResources.LoginPhaseWaitingForUserInput))]
    WaitingForUserInput,

    [LocalizedResource(nameof(LoginPageResources.LoginPhaseCheckingWebView2Installation))]
    CheckingWebView2Installation,

    [LocalizedResource(nameof(LoginPageResources.LoginPhaseSuccessNavigating))]
    SuccessNavigating
}
