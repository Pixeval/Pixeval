using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace Pixeval.LoginProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _culture;
        private readonly int _port;
        private PixivProxyServer? _proxyServer;
        private readonly TaskCompletionSource<(string, string)> _webViewCompletion = new();

        public MainWindow(string culture, int port)
        {
            _culture = culture;
            _port = port;
            InitializeComponent();
        }

        private async void LoginWebView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var port = PixivProxyServer.NegotiatePort();
            _proxyServer = PixivProxyServer.Create(IPAddress.Loopback, port, await GetFakeServerCertificate());
            await LoginWebView.EnsureCoreWebView2Async(
                await CoreWebView2Environment.CreateAsync(null, null, new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = $"--proxy-server=127.0.0.1:{port}"
                })
            );
            await LoginWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Security.setIgnoreCertificateErrors", @"{ ""ignore"": true }");
            LoginWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            LoginWebView.CoreWebView2.WebResourceRequested += (_, args) =>
            {
                args.Request.Headers.SetHeader("Accept-Language", "zh-cn");
            };
            var codeVerifier = PixivAuth.GetCodeVerify();
            LoginWebView.Source = new Uri(PixivAuth.GenerateWebPageUrl(codeVerifier));

            var (url, cookie) = await _webViewCompletion.Task;
            var code = HttpUtility.ParseQueryString(new Uri(url).Query)["code"];
            MessageBox.Show(code);
        }

        private static async Task<X509Certificate2> GetFakeServerCertificate()
        {
            if (Application.GetResourceStream(new Uri("pack://application:,,,/Pixeval.LoginProxy;component/Assets/Certs/pixeval_server_cert.pfx")) is { } streamResource)
            {
                await using (streamResource.Stream)
                {
                    return new X509Certificate2(await ToBytes(streamResource.Stream), "pixeval", X509KeyStorageFlags.UserKeySet);
                }
            }

            throw new PixivWebLoginException(Reason.CertificateNotFound);
        }

        private static async Task<byte[]> ToBytes(Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return ms.ToArray();
            }

            var mStream = new MemoryStream();
            await stream.CopyToAsync(mStream);
            return mStream.ToArray();
        }

        private async void LoginWebView_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("pixiv://"))
            {
                _webViewCompletion.SetResult((e.Uri, await GetCookiesFromWebView()));
            }
        }

        private async Task<string> GetCookiesFromWebView()
        {
            return AsString(await LoginWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.pixiv.net"));
        }

        private static string AsString(IEnumerable<CoreWebView2Cookie> cookies)
        {
            return cookies.Aggregate("", (s, cookie) => s + $"{cookie.Name}={cookie.Value};");
        }


        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            _proxyServer?.Dispose();
            LoginWebView.Dispose();
        }
    }
}
