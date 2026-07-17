using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Tests;

[TestClass]
public sealed class FileCacheTest
{
    [TestMethod]
    public void UnavailableCacheDirectoryShouldDegradeToFailure()
    {
        var cachePath = Path.GetTempFileName();
        try
        {
            var cache = new FileCache(cachePath);
            using var source = new MemoryStream([1, 2, 3]);

            Assert.IsNull(cache.TryOpen("key"));
            Assert.AreEqual(FileCacheWriteResult.Failed, cache.TryCache("key", source, null));

            cache.EnforceSizeLimit(1);
            cache.Purge();
        }
        finally
        {
            File.Delete(cachePath);
        }
    }
}
