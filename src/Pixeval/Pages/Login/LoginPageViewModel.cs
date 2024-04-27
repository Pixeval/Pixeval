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
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Bypass;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Preference;
using Pixeval.Logging;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Login;

[SettingsViewModel<LoginContext>(nameof(LoginContext))]
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

        [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseSuccessNavigating))]
        SuccessNavigating
    }

    public bool IsFinished { get; set; }

    [ObservableProperty] private bool _isEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProcessingRingVisible))]
    private LoginPhaseEnum _loginPhase;

    public LoginContext LoginContext => App.AppViewModel.LoginContext;

    public bool DisableDomainFronting
    {
        get => App.AppViewModel.AppSettings.DisableDomainFronting;
        set => App.AppViewModel.AppSettings.DisableDomainFronting = value;
    }

    public Visibility ProcessingRingVisible =>
        LoginPhase is LoginPhaseEnum.WaitingForUserInput ? Visibility.Collapsed : Visibility.Visible;

    public void AdvancePhase(LoginPhaseEnum newPhase)
    {
        LoginPhase = newPhase;
    }

    public async Task<bool> RefreshAsync(string refreshToken)
    {
        AdvancePhase(LoginPhaseEnum.Refreshing);
        using var scope = App.AppViewModel.AppServicesScope;
        var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
        var client = await MakoClient.TryGetMakoClientAsync(refreshToken,
            App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger);
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

    public async Task<Session> AuthCodeToSessionAsync(string code, string verifier)
    {
        // HttpClient is designed to be used through whole application lifetime, create and
        // dispose it in a function is a commonly misused anti-pattern, but this function
        // is intended to be called only once (at the start time) during the entire application's
        // lifetime, so the overhead is acceptable

        var httpClient = DisableDomainFronting
            ? new()
            : new HttpClient(new DelegatedHttpMessageHandler(MakoHttpOptions.CreateHttpMessageInvoker()));
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
        var session = (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession();
        RefreshToken = session.RefreshToken;
        return session;
    }

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
                var httpValueString = (string)httpsValue;
                if (httpValueString != null)
                {
                    browsers[httpValueString] = (string)location!;
                }
            }
        }
        browsers = browsers.Join(["ChromeHTML", "MSEdgeHTM"], outer => outer.Key, inner => inner, ((pair, s) => pair))
            .ToDictionary();
        var userChoiceKey = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");
        var id = userChoiceKey?.GetValue("ProgId") as string;
        if (id == null || !browsers.TryGetValue(id, out var ret))
        {
            return browsers.Values.First();
        }

        return ret!;
    }

    public async Task LoginAsync(UserControl userControl, bool useNewAccount, Action navigated)
    {
        var verifier = PixivAuthSignature.GetCodeVerify();
        var url = PixivAuthSignature.GenerateWebPageUrl(verifier);
        var browserPath = ChooseBrowser();
        var userDataDir = Path.Combine(Path.GetTempPath(), "Pixeval", "browser-user-data-dir");
        var commonArgs = $"--disable-sync --no-default-browser-check --no-first-run --user-data-dir={userDataDir} {url}";
        var startInfo = new ProcessStartInfo(browserPath)
        {
            Arguments = !DisableDomainFronting ? $"--no-proxy-server --dns-prefetch-disable {commonArgs}" : $"{commonArgs}"
        };
        var process = Process.Start(startInfo);
        if (!DisableDomainFronting)
        {
            var pid = process!.Id;
            var dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "runtimes",
                "win-x64", "native", "bypass.dll");
            var injection = Injector.Inject((uint)pid, dllPath);
            Injector.InstallChromeHook(injection, true, dllPath);
        }
    }

    public void CloseWindow() => WindowFactory.GetWindowForElement(owner).Close();
}
