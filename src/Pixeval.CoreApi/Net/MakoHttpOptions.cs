#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/MakoHttpOptions.cs
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
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Utilities;

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
            return new HttpMessageInvoker(new SocketsHttpHandler
            {
                ConnectCallback = BypassedConnectCallback(nameResolver)
            });
        }

        public static HttpMessageInvoker CreateDirectHttpMessageInvoker()
        {
            return new HttpMessageInvoker(new SocketsHttpHandler());
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