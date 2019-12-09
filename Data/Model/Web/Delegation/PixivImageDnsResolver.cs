using System.Linq;
using System.Net;

namespace Pixeval.Data.Model.Web.Delegation
{
    public class PixivImageDnsResolver : DnsResolver
    {
        protected override void UseDefaultDns()
        {
            foreach (var i in Enumerable.Range(136, 10))
            {
                IpList.Add(IPAddress.Parse($"210.140.92.{i}"));
            }
        }

        public static DnsResolver Instance = new PixivImageDnsResolver();
    }
}