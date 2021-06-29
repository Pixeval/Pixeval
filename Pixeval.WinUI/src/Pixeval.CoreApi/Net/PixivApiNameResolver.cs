using System.Net;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Net
{
    internal class PixivApiNameResolver : INameResolver
    {
        public Task<IPAddress[]> Lookup(string hostname)
        {
            // not a good idea
            if (hostname.Contains("pixiv"))
            {
                return Task.FromResult(new[]
                {
                    IPAddress.Parse("210.140.131.219"), IPAddress.Parse("210.140.131.223"), IPAddress.Parse("210.140.131.226")
                });
            }

            return Dns.GetHostAddressesAsync(hostname);
        }
    }
}