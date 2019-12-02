using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Pzxlane.Objects;

namespace Pzxlane.Data.Model.Web.Delegation
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

            return await Retry.For<IPAddress, Task<HttpResponseMessage>, TaskCanceledException>(await DnsResolver.Lookup(host), ip =>
            {
                var ipAddressString = ip.ToString();

                var isSslSession = request.RequestUri.ToString().StartsWith("https://");

                request.RequestUri = new Uri($"{(isSslSession ? "https://" : "http://")}{ipAddressString}{request.RequestUri.PathAndQuery}");
                request.Headers.Host = host;

                requestHandler?.Handle(request);

                return base.SendAsync(request, cancellationToken);
            }, 3);
        }

        protected abstract DnsResolver DnsResolver { get; set; }
    }
}