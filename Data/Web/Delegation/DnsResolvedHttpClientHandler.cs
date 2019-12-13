using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public abstract class DnsResolvedHttpClientHandler : HttpClientHandler
    {
        private readonly IHttpRequestHandler requestHandler;

        static DnsResolvedHttpClientHandler()
        {
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
        }

        protected DnsResolvedHttpClientHandler(IHttpRequestHandler requestHandler = null)
        {
            this.requestHandler = requestHandler;
            // SSL bypass
            ServerCertificateCustomValidationCallback = DangerousAcceptAnyServerCertificateValidator;
        }

        protected abstract DnsResolver DnsResolver { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var host = request.RequestUri.DnsSafeHost;

            var isSslSession = request.RequestUri.ToString().StartsWith("https://");

            request.RequestUri = new Uri($"{(isSslSession ? "https://" : "http://")}{(await DnsResolver.Lookup(host))[0]}{request.RequestUri.PathAndQuery}");
            request.Headers.Host = host;

            requestHandler?.Handle(request);

            var result = await base.SendAsync(request, cancellationToken);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                using var _ = await new AsyncLock().LockAsync();
                await Authentication.Authenticate(Identity.Global.MailAddress, Identity.Global.Password);

                var token = request.Headers.Authorization;
                if (token != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, Identity.Global.AccessToken);

                return await base.SendAsync(request, cancellationToken);
            }

            return result;
        }
    }
}