#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/PixivImageHttpMessageHandler.cs
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pixeval.CoreApi.Services;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net;

internal class PixivImageHttpMessageHandler : HttpClientHandler
{
    private readonly IOptionsMonitor<PixivClientOptions> _clientOptions;
    private IOptionsMonitor<PixivHttpOptions> _httpOptions;

    public PixivImageHttpMessageHandler(IOptionsMonitor<PixivClientOptions> clientOptions, IOptionsMonitor<PixivHttpOptions> httpOptions)
    {
        _clientOptions = clientOptions;
        _httpOptions = httpOptions;
    }


    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_clientOptions.CurrentValue.Bypass)
        {
            PixivHttpOptions.UseHttpScheme(request);
        }

        var requestUri = request.RequestUri!;
        if (requestUri.Host == _httpOptions.CurrentValue.ImageHost && _clientOptions.CurrentValue.MirrorHost is { } mirror && mirror.IsNotNullOrBlank())
        {
            request.RequestUri = mirror switch
            {
                _ when Uri.CheckHostName(mirror) is not UriHostNameType.Unknown => new UriBuilder(requestUri) { Host = mirror }.Uri,
                _ when Uri.IsWellFormedUriString(mirror, UriKind.Absolute) => new Uri(mirror).Let(mirrorUri => new UriBuilder(requestUri) { Host = mirrorUri.Host, Scheme = mirrorUri.Scheme })!.Uri,
                _ => throw new UriFormatException("Expecting a valid Host or URI")
            };
        }

        return base.SendAsync(request, cancellationToken);
    }
}