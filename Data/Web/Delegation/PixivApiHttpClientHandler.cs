using System.Net.Http;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivApiHttpClientHandler : DnsResolvedHttpClientHandler
    {
        public static HttpMessageHandler Instance = new PixivApiHttpClientHandler();

        private PixivApiHttpClientHandler() : base(PixivAuthenticationHttpRequestHandler.Instance)
        {
        }

        protected override DnsResolver DnsResolver { get; set; } = PixivApiDnsResolver.Instance;
    }
}