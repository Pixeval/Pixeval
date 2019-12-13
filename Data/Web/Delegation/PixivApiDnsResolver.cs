using System.Net;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivApiDnsResolver : DnsResolver
    {
        public static DnsResolver Instance = new PixivApiDnsResolver();

        protected override void UseDefaultDns()
        {
            IpList.Add(IPAddress.Parse("210.140.131.219"));
            IpList.Add(IPAddress.Parse("210.140.131.223"));
            IpList.Add(IPAddress.Parse("210.140.131.226"));
        }
    }
}