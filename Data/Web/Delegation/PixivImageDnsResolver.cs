using System.Linq;
using System.Net;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivImageDnsResolver : DnsResolver
    {
        public static DnsResolver Instance = new PixivImageDnsResolver();

        protected override void UseDefaultDns()
        {
            foreach (var i in Enumerable.Range(136, 10)) IpList.Add(IPAddress.Parse($"210.140.92.{i}"));
        }
    }
}