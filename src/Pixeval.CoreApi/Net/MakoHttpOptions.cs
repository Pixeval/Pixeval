// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Net;

public static partial class MakoHttpOptions
{
    public const string AppApiBaseUrl = "https://app-api.pixiv.net";

    public const string WebApiBaseUrl = "https://www.pixiv.net";

    public const string OAuthBaseUrl = "https://oauth.secure.pixiv.net";

    public const string ImageHost = "i.pximg.net";

    public const string ImageHost2 = "s.pximg.net";

    public const string WebApiHost = "www.pixiv.net"; // experiments revealed that the secondary domain 'www' is required 

    public const string AppApiHost = "app-api.pixiv.net";

    public const string AccountHost = "accounts.pixiv.net";

    public const string OAuthHost = "oauth.secure.pixiv.net";

    public static Dictionary<string, IPAddress[]> NameResolvers { get; } = new()
    {
        [ImageHost] = [],
        [WebApiHost] = [],
        [AccountHost] = [],
        [AppApiHost] = [],
        [ImageHost2] = [],
        [OAuthHost] = []
    };

    public static void SetNameResolver(string host, string[] nameResolvers)
    {
        NameResolvers[host] = nameResolvers.Select(IPAddress.Parse).ToArray();
    }

    public static readonly Regex DomainFrontingRequiredHost = MyRegex();

    [GeneratedRegex(@"^app-api\.pixiv\.net$|^www\.pixiv\.net$")]
    private static partial Regex MyRegex();

    public static void UseHttpScheme(HttpRequestMessage request)
    {
        if (request.RequestUri is not null)
        {
            request.RequestUri = new UriBuilder(request.RequestUri)
            {
                Scheme = "http"
            }.Uri;
        }
    }

    public static HttpMessageInvoker CreateHttpMessageInvoker()
    {
        return new HttpMessageInvoker(new SocketsHttpHandler
        {
            ConnectCallback = DomainFrontingConnectCallback
        });
    }

    public static HttpMessageInvoker CreateDirectHttpMessageInvoker(MakoClient makoClient)
    {
        var useProxy = makoClient.CurrentSystemProxy is not null;
        return new HttpMessageInvoker(new SocketsHttpHandler
        {
            UseProxy = useProxy,
            Proxy = makoClient.CurrentSystemProxy
        });
    }

    public static async Task<IPAddress[]> GetAddressesAsync(string host, CancellationToken token)
    {
        if (!NameResolvers.TryGetValue(host, out var ips))
            ips = await Dns.GetHostAddressesAsync(host, token);
        return ips;
    }

    private static async ValueTask<Stream> DomainFrontingConnectCallback(SocketsHttpConnectionContext context, CancellationToken token)
    {
        var sockets = new Socket(SocketType.Stream, ProtocolType.Tcp); // disposed by networkStream
        var host = context.InitialRequestMessage.RequestUri!.Host;
        await sockets.ConnectAsync(await GetAddressesAsync(host, token), 443, token).ConfigureAwait(false);
        var networkStream = new NetworkStream(sockets, true); // disposed by sslStream
        var sslStream = new SslStream(networkStream, false, (_, _, _, _) => true);
        await sslStream.AuthenticateAsClientAsync("").ConfigureAwait(false);
        return sslStream;
    }
}
