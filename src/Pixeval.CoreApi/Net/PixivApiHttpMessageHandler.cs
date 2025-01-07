// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

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
