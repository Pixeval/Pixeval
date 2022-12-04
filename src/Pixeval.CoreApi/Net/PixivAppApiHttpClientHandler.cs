#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/PixivAppApiHttpClientHandler.cs
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

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotNext.Threading;
using Microsoft.Extensions.Options;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Services;

namespace Pixeval.CoreApi.Net;

public class PixivAppApiHttpClientHandler : HttpClientHandler
{
    private readonly IOptions<PixivClientOptions> _clientOptions;
    private readonly IOptions<PixivHttpOptions> _httpOptions;
    private readonly ISessionRefresher _sessionRefresher;
    private AsyncLazy<string> _accessTokenLazy;

    public PixivAppApiHttpClientHandler(IOptions<PixivClientOptions> clientOptions, IOptions<PixivHttpOptions> httpOptions, ISessionRefresher sessionRefresher)
    {
        _clientOptions = clientOptions;
        _httpOptions = httpOptions;
        _sessionRefresher = sessionRefresher;
        _accessTokenLazy = new(async () => await _sessionRefresher.GetAccessTokenAsync());
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = request.Headers;
        var host = request.RequestUri!.Host; // the 'RequestUri' is guaranteed to be nonnull here, because the 'HttpClient' will set it to 'BaseAddress' if its null

        headers.TryAddWithoutValidation("Accept-Language", _clientOptions.Value.CultureInfo.Name);

        if (host == _httpOptions.Value.AppApiHost)
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", await _accessTokenLazy.Task);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}