using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Util
{
    public class DelegatedHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpMessageInvoker _delegate;

        public DelegatedHttpMessageHandler(HttpMessageInvoker @delegate)
        {
            _delegate = @delegate;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _delegate.SendAsync(request, cancellationToken);
        }
    }
}