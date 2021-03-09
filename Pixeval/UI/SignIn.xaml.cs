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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Pixeval.Objects;
using Pixeval.Objects.Generic;
using Pixeval.Persisting;

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
        
        private async void SignIn_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (RefreshAvailable())
            {
                new SessionRefreshing(Session.Current.RefreshToken, await GetCookies(), refreshing: true).Show();
                Close();
                return;
            }
            
            var port = ProxyServer.NegotiatePort();
            _proxyServer = ProxyServer.Create("127.0.0.1", port, await CertificateManager.GetFakeServerCertificate());
            await LoginWebView.EnsureCoreWebView2Async(
                await CoreWebView2Environment.CreateAsync(
                    null, null, new CoreWebView2EnvironmentOptions
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
            var codeVerifier = Authentication.GetCodeVer();
            LoginWebView.Source = new Uri(Authentication.GenerateWebPageUrl(codeVerifier));

            var (url, cookie) = await webViewCompletion.Task;
            var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"];

            new SessionRefreshing(code, cookie, codeVerifier).Show();

            Close();
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
                webViewCompletion.SetResult((e.Uri, await GetCookies()));
            }
        }

        private async Task<string> GetCookies()
        {
            return Session.Current?.Cookie ?? (await LoginWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.pixiv.net"))?.AsString();
        }
    }
}