#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/PixivApiHttpMessageHandler.cs
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
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net;

internal class PixivApiHttpMessageHandler(MakoClient makoClient) : MakoClientSupportedHttpMessageHandler(makoClient)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = request.Headers;
        var host = request.RequestUri!.Host; // the 'RequestUri' is guaranteed to be notnull here, because the 'HttpClient' will set it to 'BaseAddress' if it's null

        var domainFronting = (MakoHttpOptions.DomainFrontingRequiredHost.IsMatch(host) || host is MakoHttpOptions.OAuthHost) && MakoClient.Configuration.DomainFronting;

        if (domainFronting)
            MakoHttpOptions.UseHttpScheme(request);

        headers.UserAgent.AddRange(MakoClient.Configuration.UserAgent);
        headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(MakoClient.Configuration.CultureInfo.Name));

        switch (host)
        {
            case MakoHttpOptions.AppApiHost:
                var provider = MakoClient.Provider.GetRequiredService<PixivTokenProvider>();
                var tokenResponse = await provider.GetTokenAsync();
                if (tokenResponse is null)
                    ThrowUtils.Throw(new MakoTokenRefreshFailedException());
                headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                break;
            case MakoHttpOptions.WebApiHost:
                _ = headers.TryAddWithoutValidation("Cookie", MakoClient.Configuration.Cookie);
                break;
        }

        return await GetHttpMessageInvoker(domainFronting)
            .SendAsync(request, cancellationToken);
    }
}
