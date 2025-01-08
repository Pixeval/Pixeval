// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Global;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net;

internal class RetryHttpClientHandler(HttpMessageHandler delegatedHandler, int timeout) : HttpMessageHandler
{
    private readonly HttpMessageInvoker _delegatedHandler = new(delegatedHandler);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var result = await Functions.RetryAsync(() => _delegatedHandler.SendAsync(request, cancellationToken), 2, timeout).ConfigureAwait(false);
        return result switch
        {
            Result<HttpResponseMessage>.Success(var response) => response,
            Result<HttpResponseMessage>.Failure failure => ThrowUtils.Throw<HttpResponseMessage>(failure.Cause ?? new HttpRequestException()),
            _ => ThrowUtils.ArgumentOutOfRange<Result<HttpResponseMessage>, HttpResponseMessage>(result, "Unexpected case")
        };
    }
}

internal class MakoRetryHttpClientHandler(MakoClient makoClient, HttpMessageHandler delegatedHandler) : HttpMessageHandler, IMakoClientSupport
{
    private readonly HttpMessageInvoker _delegatedHandler = new(delegatedHandler);

    public MakoClient MakoClient { get; set; } = makoClient;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var result = await Functions.RetryAsync(() => _delegatedHandler.SendAsync(request, cancellationToken), 2, MakoClient.Configuration.ConnectionTimeout).ConfigureAwait(false);
        return result switch
        {
            Result<HttpResponseMessage>.Success(var response) => response,
            Result<HttpResponseMessage>.Failure failure => ThrowUtils.Throw<HttpResponseMessage>(failure.Cause ?? new HttpRequestException()),
            _ => ThrowUtils.ArgumentOutOfRange<Result<HttpResponseMessage>, HttpResponseMessage>(result, "Unexpected case")
        };
    }
}
