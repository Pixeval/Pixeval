using System.Net;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Net
{
    internal class LocalMachineNameResolver : INameResolver
    {
        public Task<IPAddress[]> Lookup(string hostname)
        {
            return Dns.GetHostAddressesAsync(hostname);
        }
    }
}