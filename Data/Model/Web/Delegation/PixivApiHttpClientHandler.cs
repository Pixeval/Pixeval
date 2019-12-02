using System.Net.Http;

namespace Pzxlane.Data.Model.Web.Delegation
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