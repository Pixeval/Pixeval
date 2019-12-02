using System.Threading.Tasks;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Refit;

namespace Pzxlane.Data.Model.Web.Protocol
{
    // thanks to @Notsfsssf[https://github.com/Notsfsssf] found this way to bypass SNI and DNS pollution
    public interface IResolveDnsProtocol
    {
        [Get("/dns-query")]
        Task<DnsResolveResponse> ResolveDns(DnsResolveRequest dnsResolverRequest);
    }
}