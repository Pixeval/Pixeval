#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Pixeval.Objects;
using Pixeval.Objects.Generic;
using Pixeval.Persisting;
using Pixeval.UI.UserControls;

namespace Pixeval.UI
{
    public partial class SignIn
    {
        private static ProxyServer _proxyServer;

        private readonly TaskCompletionSource<(string, string)> webViewCompletion = new TaskCompletionSource<(string, string)>();

        public SignIn()
        {
            InitializeComponent();
        }

        private static bool RefreshAvailable()
        {
            if (Session.Current == null)
                return false;
            return Session.Current.RefreshToken != null && DateTime.Now - Session.Current.CookieCreation <= TimeSpan.FromDays(7);
        }

        private async Task CheckUpdate()
        {
            if (await PixevalContext.UpdateAvailable() && await MessageDialog.Show(MessageDialogHost,Pixeval.Resources.Resources.PixevalUpdateAvailable, Pixeval.Resources.Resources.PixevalUpdateAvailableTitle, true) == MessageDialogResult.Yes)
            {
                Process.Start(@"updater\Pixeval.Updater.exe");
                Environment.Exit(0);
            }
            MessageDialogHost.Visibility = Visibility.Hidden;
        }

        private async Task LoginAndClose(string token, string cookie, string codeVer = null, bool refresh = false)
        {
            LoginWebView.Visibility = Visibility.Hidden;
            LoggingInGrid.Visibility = Visibility.Visible;
            LoggingInHintTextBlock.Text = refresh ? Pixeval.Resources.Resources.SignInUpdatingSession : Pixeval.Resources.Resources.SignInLoggingIn;
            if (refresh)
            {
                await Authentication.Refresh(token);
            }
            else
            {
                await Authentication.AuthorizationCodeToToken(token, codeVer);
            }
            Session.Current.Cookie = cookie;
            Session.Current.CookieCreation = DateTime.Now;

            new MainWindow().Show();
            Close();
        }

        private async Task PerformLogin()
        {
            if (RefreshAvailable() && GetCookiesFromSettings() is { } cookies)
            {
                await LoginAndClose(Session.Current.RefreshToken, cookies, refresh: true);
                return;
            }

            LoginWebView.Visibility = Visibility.Visible;
            var port = ProxyServer.NegotiatePort();
            _proxyServer = ProxyServer.Create("127.0.0.1", port, await CertificateManager.GetFakeServerCertificate());
            await LoginWebView.EnsureCoreWebView2Async(
                await CoreWebView2Environment.CreateAsync(
                    null, Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "pwData"), new CoreWebView2EnvironmentOptions
                    {
                        AdditionalBrowserArguments = $"--proxy-server=127.0.0.1:{port}"
                    }
                )
            );
            await LoginWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Security.setIgnoreCertificateErrors", "{ \"ignore\": true }");
            LoginWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            LoginWebView.CoreWebView2.WebResourceRequested += (o, args) =>
            {
                args.Request.Headers.SetHeader("Accept-Language", "zh-cn");
            };
            var codeVerifier = Authentication.GetCodeVerify();
            LoginWebView.Source = new Uri(Authentication.GenerateWebPageUrl(codeVerifier));

            var (url, cookie) = await webViewCompletion.Task;
            var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"];

            await LoginAndClose(code, cookie, codeVerifier);
        }

        private async void SignIn_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await CheckUpdate();
                await PerformLogin();
            }
            catch (Exception exception)
            {
                LoginWebView.Visibility = Visibility.Hidden;
                if (await MessageDialog.Warning(MessageDialogHost, exception.Message) == MessageDialogResult.Yes)
                {
                    Environment.Exit(-1);
                }
            }
        }

        private void SignIn_OnClosed(object sender, EventArgs e)
        {
            LoginWebView.Dispose();
            _proxyServer?.Dispose();
        }

        private async void LoginWebView_OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("pixiv://"))
            {
                webViewCompletion.SetResult((e.Uri, await GetCookiesFromWebView()));
            }
        }

        private static string GetCookiesFromSettings()
        {
            return Session.Current?.Cookie;
        }
        
        private async Task<string> GetCookiesFromWebView()
        {
            return (await LoginWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.pixiv.net"))?.AsString();
        }
    }
}