using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;
using WebApiClientCore.Exceptions;

namespace Pixeval.Tests;

[TestClass]
public sealed class FileLoggerTest
{
    [TestMethod]
    public async Task UnavailableLogDirectoryShouldNotThrow()
    {
        var path = Path.GetTempFileName();
        try
        {
            var logger = new FileLogger(path);
            var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            logger.Logging += (_, _) =>
            {
                completionSource.SetResult();
                return false;
            };

            logger.LogError("Test log", new IOException("Test exception"));
            await completionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task LogErrorShouldIncludeRedactedNetworkDetails()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            "https://oauth.secure.pixiv.net/auth/token?code=sensitive-code");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "sensitive-authorization");
        _ = request.Headers.TryAddWithoutValidation("Cookie", "session=sensitive-cookie");

        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.RequestMessage = request;
        response.Content = new StringContent(
            """
            {"error":"upstream failure","refresh_token":"sensitive-refresh-token"}
            """,
            Encoding.UTF8,
            "text/plain");
        response.Headers.Location = new Uri("https://www.pixiv.net/callback?code=sensitive-location-code");
        _ = response.Headers.TryAddWithoutValidation("Set-Cookie", "session=sensitive-response-cookie");

        var exception = new HttpRequestException(
            "Request failed",
            new ApiResponseStatusException(response),
            HttpStatusCode.OK);
        var logger = new FileLogger(Path.Combine(Path.GetTempPath(), nameof(Pixeval), nameof(FileLoggerTest)));
        var completionSource = new TaskCompletionSource<LogModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        logger.Logging += (_, model) =>
        {
            completionSource.SetResult(model);
            return true;
        };

        logger.LogError("Network request failed", exception);
        var log = await completionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var text = log.ToString();

        StringAssert.Contains(text, "Network:");
        StringAssert.Contains(text, "StatusCode: 200 OK");
        StringAssert.Contains(text, "Uri: https://oauth.secure.pixiv.net/auth/token");
        StringAssert.Contains(text, "refresh_token\":\"<redacted>");
        StringAssert.Contains(text, "Authorization: <redacted>");
        StringAssert.Contains(text, "Cookie: <redacted>");
        StringAssert.Contains(text, "Location: https://www.pixiv.net/callback");
        StringAssert.Contains(text, "Set-Cookie: <redacted>");
        Assert.IsFalse(text.Contains("sensitive-code", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("sensitive-authorization", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("sensitive-cookie", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("sensitive-location-code", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("sensitive-response-cookie", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("sensitive-refresh-token", StringComparison.Ordinal));
    }
}
