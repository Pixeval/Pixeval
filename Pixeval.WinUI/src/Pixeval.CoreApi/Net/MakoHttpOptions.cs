using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Net
{
    internal static class MakoHttpOptions
    {
        public const string AppApiBaseUrl = "https://app-api.pixiv.net";
        
        public const string SauceNaoBaseUrl = "https://saucenao.com";
        
        public const string OAuthBaseUrl = "https://oauth.secure.pixiv.net";
        
        public const string WebApiBaseUrl = "https://www.pixiv.net";
        
        public const string ImageHost = "i.pximg.net";

        public const string WebApiHost = "www.pixiv.net"; // experiments revealed that the secondary domain 'www' is required 

        public const string AppApiHost = "app-api.pixiv.net";

        public static readonly Regex BypassRequiredHost = "^app-api\\.pixiv\\.net$|^www\\.pixiv\\.net$".ToRegex();

        public static string AppApiUrl(string query) => $"{AppApiBaseUrl}/{query}";

        public static void UseHttpScheme(HttpRequestMessage request)
        {
            if (request.RequestUri != null)
            {
                request.RequestUri = new UriBuilder(request.RequestUri)
                {
                    Scheme = "http"
                }.Uri;
            }
        }

        public static HttpMessageInvoker CreateHttpMessageInvoker(INameResolver nameResolver)
        {
            return new(new SocketsHttpHandler
            {
                ConnectCallback = BypassedConnectCallback(nameResolver)
            });
        }
        
        private static Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> BypassedConnectCallback(INameResolver nameResolver)
        {
            return async (context, token) =>
            {
                var sockets = new Socket(SocketType.Stream, ProtocolType.Tcp); // disposed by networkStream
                await sockets.ConnectAsync(await nameResolver.Lookup(context.InitialRequestMessage.RequestUri!.Host), 443, token);
                var networkStream = new NetworkStream(sockets, true); // disposed by sslStream
                var sslStream = new SslStream(networkStream, false, (_, _, _, _) => true);
                await sslStream.AuthenticateAsClientAsync(string.Empty);
                return sslStream;
            };
        }
    }
}