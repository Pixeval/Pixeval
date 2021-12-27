#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/LoginWebViewPopup.xaml.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace Pixeval.Popups
{
    public sealed partial class LoginWebViewPopup : IAppPopupContent
    {
        public LoginWebViewPopup()
        {
            InitializeComponent();
            UniqueId = Guid.NewGuid();
        }

        public readonly TaskCompletionSource<(string, string)> CookieCompletion = new();

        public Guid UniqueId { get; }

        public FrameworkElement UIContent => this;

        private async void LoginWebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (args.Uri.StartsWith("pixiv://"))
            {
                CookieCompletion.SetResult((args.Uri, await LoginWebView.CoreWebView2.ExecuteScriptAsync("document.cookie")));
            }
        }

        private async void LoginWebView_OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            await LoginWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Security.setIgnoreCertificateErrors", "{ \"ignore\": true }");
        }
    }
}
