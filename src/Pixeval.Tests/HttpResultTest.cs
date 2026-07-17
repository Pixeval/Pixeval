using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class HttpResultTest
{
    [TestMethod]
    public async Task TransportExceptionShouldReturnFailure()
    {
        using var client = new HttpClient(new ThrowingHandler());

        var result = await client.GetStringResultAsync("https://example.com");

        var failure = Assert.IsInstanceOfType<Result<string>.Failure>(result);
        Assert.IsInstanceOfType<HttpRequestException>(failure.Cause);
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Test exception"));
    }
}
