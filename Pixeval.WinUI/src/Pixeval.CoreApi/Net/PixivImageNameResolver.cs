using System.Net;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Net
{
    internal class PixivImageNameResolver : INameResolver
    {
        public Task<IPAddress[]> Lookup(string hostname)
        {
            if (hostname == "i.pximg.net")
            {
                return Task.FromResult(new[]
                {
                    IPAddress.Parse("210.140.92.138"), IPAddress.Parse("210.140.92.139"), IPAddress.Parse("210.140.92.140")
                });
            }

            return Dns.GetHostAddressesAsync(hostname);
        }
    }
}