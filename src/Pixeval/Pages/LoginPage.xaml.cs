using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Web.WebView2.Core;
using Pixeval.Util;
using Pixeval.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages
{
    public sealed partial class LoginPage
    {
        private readonly LoginPageViewModel _viewModel = new();

        private PixivProxyServer? _proxyServer;

        public LoginPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshAvailable = await _viewModel.CheckRefreshAvailable();
            if (_viewModel.RefreshAvailable)
            {
                await _viewModel.Refresh();
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo());
            }
            else
            {
                await PrepareForLogin();
            }
        }

        private async Task PrepareForLogin()
        {
            await LoginWebView.EnsureCoreWebView2Async();
            LoginWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            LoginWebView.CoreWebView2.WebResourceRequested += (o, args) =>
            {
                if (args.Request.Uri == "https://www.baidu.com/")
                {
                    args.Request.Uri = "http://www.bilibili.com/";
                }
            };
            LoginWebView.Source = new Uri("https://www.baidu.com");
            Debug.WriteLine(LoginWebView.Source);
            var port = PixivProxyServer.NegotiatePort();
            // _proxyServer = PixivProxyServer.Create(IPAddress.Loopback, port, await CertificateManager.GetFakeServerCertificate());
        }
    }
}
