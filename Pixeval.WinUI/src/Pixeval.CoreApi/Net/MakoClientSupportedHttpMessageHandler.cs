using System.Net.Http;

namespace Pixeval.CoreApi.Net
{
    public abstract class MakoClientSupportedHttpMessageHandler : HttpMessageHandler, IMakoClientSupport
    {
        public abstract MakoClient MakoClient { get; set; }
    }
}