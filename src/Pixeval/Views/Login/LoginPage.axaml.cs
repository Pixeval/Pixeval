using System;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Pixeval.Utilities;
using Pixeval.Views.Capability;

namespace Pixeval.Views.Login;

public partial class LoginPage : ContentPage
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
            LoginNavigate();
    }

    private void OpenWebView_OnClick(object? sender, RoutedEventArgs e)
    {
        WebView.EnvironmentRequested += (o, args) =>
        {
            if (!App.AppViewModel.AppSettings.EnableDomainFronting)
                return;
            switch (args)
            {
                case WindowsWebView2EnvironmentRequestedEventArgs webView2Args:
                case AppleWKWebViewEnvironmentRequestedEventArgs appleArgs:
                case GtkWebViewEnvironmentRequestedEventArgs gtkArgs:
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
                App.AppViewModel.MakoClient.SetCode(code, verifier);
                if (await App.AppViewModel.MakoClient.IdentifyTokenAsync())
                    Avalonia.Threading.Dispatcher.UIThread.Invoke(LoginNavigate);
                // TODO else
                //_ = await TopLevel.GetTopLevel(this)?.ViewContainer?.CreateAcknowledgementAsync(LoginPageResources.FetchingSessionFailedTitle,
                //    LoginPageResources.FetchingSessionFailedContent);
            }
        };
        WebView.Source = new Uri(PixivAuth.GenerateWebPageUrl(verifier));
    }

    public void LoginNavigate()
    {
        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
        viewContainer?.NavigateTo(new RecommendWorksPage(), true);
    }
}
