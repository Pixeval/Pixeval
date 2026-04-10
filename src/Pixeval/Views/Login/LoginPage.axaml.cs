using System;
using System.Net;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Mako.Net;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.Views.Capability;

namespace Pixeval.Views.Login;

public partial class LoginPage : UserControl
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var token = TextBox.Text;
        if (string.IsNullOrWhiteSpace(token))
            return;

        App.AppViewModel.MakoClient.SetToken(token);
        if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
            LoginNavigate(token);
    }

    private void OpenWebView_OnClick(object? sender, RoutedEventArgs e)
    {
        var port = PixivAuth.NegotiatePort();

        var proxyServer = null as PixivAuthenticationProxyServer;
        WebView.EnvironmentRequested += (o, args) =>
        {
            if (!App.AppViewModel.AppSettings.EnableDomainFronting)
                return;
            switch (args)
            {
                case WindowsWebView2EnvironmentRequestedEventArgs webView2Args:
                    proxyServer = PixivAuthenticationProxyServer.Create(IPAddress.Loopback, port,
                        t => App.AppViewModel.MakoClient.CreateConnectionAsync(t));
                    webView2Args.AdditionalBrowserArguments = $"--ignore-certificate-errors --proxy-server=127.0.0.1:{port}";
                    break;
                case AppleWKWebViewEnvironmentRequestedEventArgs appleArgs:
                case GtkWebViewEnvironmentRequestedEventArgs gtkArgs:
                    // TODO proxy
                    break;
            }
        };
        WebView.IsVisible = true;
        var verifier = PixivAuth.GetCodeVerify();
        WebView.NavigationStarted += async (o, args) =>
        {
            if (args.Request is { } uri && uri.OriginalString.StartsWith("pixiv://"))
            {
                WebView.IsVisible = false;
                var code = HttpUtility.ParseQueryString(uri.Query)["code"]!;
                try
                {
                    if (await App.AppViewModel.MakoClient.RequestSessionAsync(code, verifier) is { } refreshToken)
                        Avalonia.Threading.Dispatcher.UIThread.Invoke(() => LoginNavigate(refreshToken));
                    // TODO else
                    //_ = await TopLevel.GetTopLevel(this)?.ViewContainer?.CreateAcknowledgementAsync(LoginPageResources.FetchingSessionFailedTitle,
                    //    LoginPageResources.FetchingSessionFailedContent);
                }
                finally
                {
                    proxyServer?.Dispose();
                }
            }
        };
        WebView.Source = new Uri(PixivAuth.GenerateWebPageUrl(verifier));
    }

    public void LoginNavigate(string refreshToken)
    {
        var loginContext = App.AppViewModel.LoginContext;
        loginContext.CurrentRefreshToken = refreshToken;
        AppInfo.SaveLoginContext(loginContext);

        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
        viewContainer?.NavigateTo<RecommendWorksPage>(true);
    }
}
