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
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions.Logger;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using Refit;

namespace Pixeval.UI
{
    public partial class SignIn
    {
        private static ProxyServer _proxyServer;

        private readonly TaskCompletionSource<string> webViewCompletion = new TaskCompletionSource<string>();

        public SignIn()
        {
            InitializeComponent();
        }

        private async void SignIn_OnLoaded(object sender, RoutedEventArgs e)
        {
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
                args.Request.Headers.SetHeader("Accept-Language", Settings.Global.DirectConnect ? "zh-cn" : Settings.Global.Culture);
            };
            var codeVerifier = Authentication.GetCodeVer();
            LoginWebView.Source = new Uri(await Authentication.GenerateWebPageUrl(codeVerifier));

            var url = await webViewCompletion.Task;
            var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"];
            
            // TODO send request
            
            var mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }

        private async void SignIn_OnInitialized(object sender, EventArgs e)
        {
            /*if (Session.ConfExists())
            {
                try
                {
                    UpdatingSessionDialogHost.OpenControl();
                    await Session.RefreshIfRequired();
                }
                catch (Exception exception)
                {
                    SetErrorHint(exception);
                    ExceptionDumper.WriteException(exception);
                    UpdatingSessionDialogHost.CurrentSession.Close();
                    return;
                }

                UpdatingSessionDialogHost.CurrentSession.Close();

                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }*/
        }

        private static async ValueTask<bool> IsPasswordOrAccountError(ApiException exception)
        {
            var eMess = await exception.GetContentAsAsync<dynamic>();
            return eMess.errors.system.code == 1508;
        }

        private void SignIn_OnClosed(object sender, EventArgs e)
        {
            _proxyServer.Dispose();
        }

        private void LoginWebView_OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("pixiv://"))
            {
                webViewCompletion.SetResult(e.Uri);
            }
        }
    }
}