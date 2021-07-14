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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Mako.Util;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Web.WebView2.Core;
using Pixeval.Util;
using Pixeval.ViewModel;
using Functions = Mako.Util.Functions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages
{
    internal class LoginTokenResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; set; }

        [JsonPropertyName("cookie")]
        public string? Cookie { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; } 
    }

    public sealed partial class LoginPage
    {
        private readonly LoginPageViewModel _viewModel = new();

        public LoginPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (await _viewModel.CheckRefreshAvailable())
            {
                await _viewModel.Refresh();
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo());
            }
            else
            {
                var token = await WhenLoginTokenRequestedAsync();
            }
        }

        private static async Task<(string cookie, string token)> WhenLoginTokenRequestedAsync()
        {
            // shit code
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:6745/");
            httpListener.Start();
            while (true)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.Url?.PathAndQuery is "/login/token"
                    && context.Request.HasEntityBody
                    && context.Request.HttpMethod == "POST")
                {
                    using var streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding, leaveOpen: false);
                    var content = await streamReader.ReadToEndAsync();
                    var json = content.FromJson<LoginTokenResponse>();
                    if (json?.Cookie is { } cookie && json?.Code is { } token)
                    {
                        SendResponse(context, 200);
                        return (cookie, token);
                    }
                    SendResponse(context, 400);
                }
            }

            static void SendResponse(HttpListenerContext context, int code)
            {
                context.Response.StatusCode = code;
                context.Response.SendChunked = false;
                context.Response.Headers.Clear();
                context.Response.Close();
            }
        }
    }
}
