#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/MakoHttpOptions.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

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
    public static class MakoHttpOptions
    {
        public const string AppApiBaseUrl = "https://app-api.pixiv.net";

        public const string WebApiBaseUrl = "https://www.pixiv.net";

        public const string OAuthBaseUrl = "https://oauth.secure.pixiv.net";

        public const string ImageHost = "i.pximg.net";

        public const string WebApiHost = "www.pixiv.net"; // experiments revealed that the secondary domain 'www' is required 

        public const string AppApiHost = "app-api.pixiv.net";

        public const string OAuthHost = "oauth.secure.pixiv.net";

        public static readonly Regex BypassRequiredHost = "^app-api\\.pixiv\\.net$|^www\\.pixiv\\.net$".ToRegex();

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

        public static HttpMessageInvoker CreateDirectHttpMessageInvoker()
        {
            return new(new SocketsHttpHandler());
        }


        private static Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> BypassedConnectCallback(INameResolver nameResolver)
        {
            return async (context, token) =>
            {
                var sockets = new Socket(SocketType.Stream, ProtocolType.Tcp); // disposed by networkStream
                await sockets.ConnectAsync(await nameResolver.Lookup(context.InitialRequestMessage.RequestUri!.Host).ConfigureAwait(false), 443, token).ConfigureAwait(false);
                var networkStream = new NetworkStream(sockets, true); // disposed by sslStream
                var sslStream = new SslStream(networkStream, false, (_, _, _, _) => true);
                await sslStream.AuthenticateAsClientAsync(string.Empty).ConfigureAwait(false);
                return sslStream;
            };
        }
    }
}