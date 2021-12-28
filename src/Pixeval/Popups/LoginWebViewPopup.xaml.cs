using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Pixeval.Misc;
using Vanara.PInvoke;

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
            var browserProcessId = LoginWebView.CoreWebView2.BrowserProcessId;
            HookInjector.Inject(browserProcessId);
        }
    }
}
