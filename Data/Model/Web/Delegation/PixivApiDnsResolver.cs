using System.Net;

namespace Pzxlane.Data.Model.Web.Delegation
{
    public class PixivApiDnsResolver : DnsResolver
    {
        protected override void UseDefaultDns()
        {
            IpList.Add(IPAddress.Parse("210.140.131.219"));
            IpList.Add(IPAddress.Parse("210.140.131.223"));
            IpList.Add(IPAddress.Parse("210.140.131.226"));
        }

        public static DnsResolver Instance = new PixivApiDnsResolver();
    }
}