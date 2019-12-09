using System.Net.Http;

namespace Pixeval.Data.Model.Web.Delegation
{
    public class PixivApiHttpClientHandler : DnsResolvedHttpClientHandler
    {
        private PixivApiHttpClientHandler() : base(PixivAuthenticationHttpRequestHandler.Instance)
        {

        }

        protected override DnsResolver DnsResolver { get; set; } = PixivApiDnsResolver.Instance;

        public static HttpMessageHandler Instance = new PixivApiHttpClientHandler();
    }
}