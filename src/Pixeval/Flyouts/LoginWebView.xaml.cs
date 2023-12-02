#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LoginWebView.xaml.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Pixeval.Flyouts;

public sealed partial class LoginWebView
{
    public LoginWebView() => InitializeComponent();

    public readonly TaskCompletionSource<(string, string)> CookieCompletion =
        new();

    private async void LoginWebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (args.Uri.StartsWith("pixiv://"))
        {
            var cookies = await LoginWebView2.CoreWebView2.CookieManager.GetCookiesAsync("https://pixiv.net");
            var cookieString = string.Join(';', cookies.Select(c => $"{c.Name}={c.Value}"));
            CookieCompletion.SetResult((args.Uri, cookieString));
        }
    }

    private async void LoginWebView_OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        _ = await LoginWebView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Security.setIgnoreCertificateErrors", "{ \"ignore\": true }");
    }
}
