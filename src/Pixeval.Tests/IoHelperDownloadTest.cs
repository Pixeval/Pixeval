using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Tests;

[TestClass]
public sealed class IoHelperDownloadTest
{
    [TestMethod]
    public async Task PreviewCanReadGrowingBufferWithoutCorruptingDownload()
    {
        var expected = new byte[16384];
        new Random(42).NextBytes(expected);
        using var client = new HttpClient(new ImageResponseHandler(expected));
        var updates = 0;
        var result = await client.DownloadMemoryStreamAsync(new Uri("https://example.test/image"),
            bufferSize: 1024, onDataAvailable: (stream, _) =>
            {
                updates++;
                stream.Position = 0;
                Assert.AreEqual(expected[0], stream.ReadByte());
                return Task.CompletedTask;
            });
        await using var downloaded = result.UnwrapOrThrow();
        using var copy = new MemoryStream();
        await downloaded.CopyToAsync(copy);
        Assert.IsGreaterThan(1, updates);
        CollectionAssert.AreEqual(expected, copy.ToArray());
    }

    [TestMethod]
    public async Task CancellationDuringPreviewReleasesDownloadBuffer()
    {
        using var client = new HttpClient(new ImageResponseHandler(new byte[16384]));
        using var cancellation = new CancellationTokenSource();
        Stream? borrowed = null;
        var result = await client.DownloadMemoryStreamAsync(new Uri("https://example.test/image"),
            onDataAvailable: (stream, token) =>
            {
                borrowed = stream;
                cancellation.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }, token: cancellation.Token);
        Assert.IsInstanceOfType<Result<Stream>.Failure>(result);
        Assert.IsNotNull(borrowed);
        Assert.IsFalse(borrowed.CanRead);
    }

    private sealed class ImageResponseHandler(byte[] bytes) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
    }

    [TestMethod]
    public async Task InvalidUriShouldReturnFailure()
    {
        using var client = new HttpClient();

        var result = await client.DownloadMemoryStreamAsync("http://[");

        Assert.IsInstanceOfType<Result<Stream>.Failure>(result);
    }

    [TestMethod]
    public async Task LocalFileUriShouldReturnReadableStream()
    {
        var path = Path.GetTempFileName();
        var expected = new byte[] { 1, 2, 3 };
        await File.WriteAllBytesAsync(path, expected);

        try
        {
            using var client = new HttpClient();
            var uri = new UriBuilder(Uri.UriSchemeFile, "", -1, path).Uri;
            var result = await client.DownloadMemoryStreamAsync(uri);
            await using var stream = result.UnwrapOrThrow();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            Assert.IsTrue(stream is FileStream);
            Assert.AreSequenceEqual(expected, memory.ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task LocalFileUriShouldCopyToDestinationStream()
    {
        var path = Path.GetTempFileName();
        var expected = new byte[] { 4, 5, 6 };
        await File.WriteAllBytesAsync(path, expected);

        try
        {
            using var client = new HttpClient();
            var uri = new UriBuilder(Uri.UriSchemeFile, "", -1, path).Uri;
            using var destination = new MemoryStream();
            var error = await client.DownloadStreamAsync(destination, uri);

            Assert.IsNull(error);
            Assert.AreSequenceEqual(expected, destination.ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }
}
