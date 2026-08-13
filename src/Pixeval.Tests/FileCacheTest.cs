using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Tests;

[TestClass]
public sealed class FileCacheTest
{
    [TestMethod]
    public async Task UnavailableCacheDirectoryShouldDegradeToFailure()
    {
        var cachePath = Path.GetTempFileName();
        try
        {
            var cache = new FileCache(cachePath);
            using var source = new MemoryStream([1, 2, 3]);

            Assert.IsNull(cache.TryOpen("key"));
            Assert.AreEqual(FileCacheWriteResult.Failed, cache.TryCache("key", source, null));

            await cache.EnforceSizeLimitAsync(1);
            await cache.PurgeAsync();
        }
        finally
        {
            File.Delete(cachePath);
        }
    }
}
