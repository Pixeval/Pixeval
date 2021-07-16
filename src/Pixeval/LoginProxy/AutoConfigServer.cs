using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Mako.Util;

namespace Pixeval.LoginProxy
{
    public class AutoConfigServer : IDisposable
    {
        private readonly int _port;
        private readonly HttpListener _httpListener;

        public static string AutoConfigUrl(string port)
        {
            return $"http://localhost:{port}/pac";
        }

        public AutoConfigServer(int port)
        {
            _port = port;

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{_port}/");
        }

        public async void Start()
        {
            _httpListener.Start();
            while (_httpListener.IsListening)
            {
                var context = await _httpListener.GetContextAsync();
                if (context.Request.Url?.PathAndQuery is "/pac" && context.Request.HttpMethod == "GET")
                {
                    context.Response.ContentType = "application/x-ns-proxy-autoconfig";
                    await context.Response.OutputStream.WriteAsync((await BuildPacString()).GetBytes(Encoding.ASCII));
                    SendResponse(context, 200);
                }
                SendResponse(context, 400);
            }

            // ReSharper disable once SuggestBaseTypeForParameter ! R# static analysis issue
            static void SendResponse(HttpListenerContext context, int code)
            {
                context.Response.StatusCode = code;
                context.Response.SendChunked = false;
                context.Response.Close();
            }
        }

        private async Task<string> BuildPacString()
        {
            return string.Format((await AppContext.GetAssetBytes("Texts/PACTemplate.template")).GetString(Encoding.ASCII), _port);
        }

        public void Dispose()
        {
            _httpListener.Stop();
        }
    }
}