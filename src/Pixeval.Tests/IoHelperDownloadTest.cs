using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Tests;

[TestClass]
public sealed class IoHelperDownloadTest
{
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
