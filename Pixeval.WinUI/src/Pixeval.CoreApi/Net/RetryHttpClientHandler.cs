using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Net
{
    internal class RetryHttpClientHandler : HttpMessageHandler, IMakoClientSupport
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global ! Dependency Injected
        public MakoClient MakoClient { get; set; } = null!;

        private readonly HttpMessageInvoker _delegatedHandler;
        
        public RetryHttpClientHandler(HttpMessageHandler delegatedHandler)
        {
            _delegatedHandler = new HttpMessageInvoker(delegatedHandler);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Functions.RetryAsync(() => _delegatedHandler.SendAsync(request, cancellationToken), 2, MakoClient!.Configuration.ConnectionTimeout) switch
            {
                Result<HttpResponseMessage>.Success (var response) => response,
                Result<HttpResponseMessage>.Failure failure        => throw failure.Cause ?? new HttpRequestException(),
                _                                                  => throw new InvalidOperationException("Unexpected case")
            };
        }
    }
}