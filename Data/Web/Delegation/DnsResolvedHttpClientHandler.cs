using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var host = request.RequestUri.DnsSafeHost;

            var isSslSession = request.RequestUri.ToString().StartsWith("https://");

            request.RequestUri = new Uri($"{(isSslSession ? "https://" : "http://")}{(await DnsResolver.Lookup(host))[0]}{request.RequestUri.PathAndQuery}");
            request.Headers.Host = host;

            requestHandler?.Handle(request);

            return await base.SendAsync(request, cancellationToken);
        }

        protected abstract DnsResolver DnsResolver { get; set; }
    }
}