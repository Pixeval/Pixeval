using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.I18N;

namespace Pixeval.Tests;

[TestClass]
public sealed class I18NResilienceTest
{
    [TestMethod]
    public void UnavailableResourceDirectoryShouldBeIgnored()
    {
        var path = Path.GetTempFileName();
        try
        {
            var plugin = new JsonMarkdownLangPlugin();

            plugin.Load(CultureInfo.GetCultureInfo("en-US"), new DirectoryInfo(path), true);

            Assert.HasCount(0, plugin.DefaultCultureResources);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
