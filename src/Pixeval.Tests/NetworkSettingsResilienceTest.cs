using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.Utilities.GitHub;

namespace Pixeval.Tests;

[TestClass]
public sealed class NetworkSettingsResilienceTest
{
    [TestMethod]
    public void InvalidConfiguredAddressesShouldBeIgnored()
    {
        var settings = new NetworkSettingsGroup { GitHubNameResolver = ["not-an-ip-address", IPAddress.Loopback.ToString()] };

        var configured = GitHubHttpOptions.TryGetConfiguredAddresses(
            settings,
            GitHubHttpOptions.Host,
            out var addresses);

        Assert.IsTrue(configured);
        Assert.HasCount(1, addresses);
        Assert.AreEqual(IPAddress.Loopback, addresses[0]);
    }
}
