// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net;

internal class PixivImageHttpMessageHandler(MakoClient makoClient) : MakoClientSupportedHttpMessageHandler(makoClient)
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (MakoClient.Configuration.DomainFronting)
        {
            MakoHttpOptions.UseHttpScheme(request);
        }

        request.Headers.UserAgent.AddRange(MakoClient.Configuration.UserAgent);

        if (request.RequestUri is { Host: MakoHttpOptions.ImageHost } requestUri
            && MakoClient.Configuration.MirrorHost is { } mirror
            && !string.IsNullOrWhiteSpace(mirror))
        {
            request.RequestUri = mirror switch
            {
                _ when Uri.CheckHostName(mirror) is not UriHostNameType.Unknown => new UriBuilder(requestUri) { Host = mirror }.Uri,
                _ when Uri.IsWellFormedUriString(mirror, UriKind.Absolute) => new Uri(mirror).Let(mirrorUri => new UriBuilder(requestUri) { Host = mirrorUri.Host, Scheme = mirrorUri.Scheme }).Uri,
                _ => ThrowUtils.UriFormat<Uri>("Expecting a valid Host or URI")
            };
        }

        return GetHttpMessageInvoker(MakoClient.Configuration.DomainFronting)
            .SendAsync(request, cancellationToken);
    }
}
