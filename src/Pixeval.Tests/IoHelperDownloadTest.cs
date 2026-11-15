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
    public async Task StalledBodyTimesOutAfterWritingReceivedBytesToDestination()
    {
        using var source = new ControlledDownloadStream();
        source.Send([1, 2, 3, 4]);
        using var client = new HttpClient(new DownloadStreamHandler(source));
        using var destination = new MemoryStream();
        var error = await client.DownloadStreamAsync(destination, new Uri("https://example.test/stalled"),
            readTimeout: TimeSpan.FromMilliseconds(100));

        Assert.IsInstanceOfType<TimeoutException>(error);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, destination.ToArray());
    }

    [TestMethod]
    public async Task CancellingStalledBodyPreservesCancellationInsteadOfReportingTimeout()
    {
        using var source = new ControlledDownloadStream();
        using var client = new HttpClient(new DownloadStreamHandler(source));
        using var destination = new MemoryStream();
        using var cancellation = new CancellationTokenSource();
        var downloading = client.DownloadStreamAsync(destination, new Uri("https://example.test/cancelled"),
            readTimeout: TimeSpan.FromSeconds(5), token: cancellation.Token);
        await source.WaitForReadAsync();
        await cancellation.CancelAsync();
        var error = await downloading.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsInstanceOfType<OperationCanceledException>(error);
        Assert.IsNotInstanceOfType<TimeoutException>(error);
    }

    [TestMethod]
    public async Task ActiveSlowBodyCanOutliveBothHttpAndReadTimeouts()
    {
        using var source = new ControlledDownloadStream();
        using var client = new HttpClient(new DownloadStreamHandler(source));
        client.Timeout = TimeSpan.FromMilliseconds(100);
        using var destination = new MemoryStream();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var downloading = client.DownloadStreamAsync(destination, new Uri("https://example.test/slow"),
            readTimeout: TimeSpan.FromSeconds(1), token: cancellation.Token);
        for (var i = 0; i < 6; i++)
        {
            await source.WaitForReadAsync();
            await Task.Delay(250);
            source.Send([(byte) i]);
        }
        source.Complete();
        var error = await downloading.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsNull(error);
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, destination.ToArray());
    }

    [TestMethod]
    public async Task NetworkDataReachesDestinationBeforeResponseCompletes()
    {
        using var source = new ControlledDownloadStream();
        source.Send([1, 2, 3]);
        using var client = new HttpClient(new DownloadStreamHandler(source));
        using var destination = new MemoryStream();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var downloading = client.DownloadStreamAsync(destination, new Uri("https://example.test/streaming"),
            token: cancellation.Token);
        await source.WaitForReadAsync();
        await source.WaitForReadAsync();

        Assert.IsFalse(downloading.IsCompleted);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destination.ToArray());
        source.Send([4, 5]);
        source.Complete();
        Assert.IsNull(await downloading);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, destination.ToArray());
    }

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
