using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class TabClosePlannerTest
{
    private static readonly string[] Tabs = ["A", "B", "C", "D"];

    [TestMethod]
    public void OthersShouldReturnEveryTabExceptContextTab() =>
        Assert.AreSequenceEqual(
            ["A", "C", "D"], TabClosePlanner.GetTargets(Tabs, "B", TabCloseScope.Others).ToArray());

    [TestMethod]
    public void LeftShouldOnlyReturnTabsBeforeContextTab() =>
        Assert.AreSequenceEqual(
            ["A"], TabClosePlanner.GetTargets(Tabs, "B", TabCloseScope.Left).ToArray());

    [TestMethod]
    public void RightShouldOnlyReturnTabsAfterContextTab() =>
        Assert.AreSequenceEqual(
            ["C", "D"], TabClosePlanner.GetTargets(Tabs, "B", TabCloseScope.Right).ToArray());

    [TestMethod]
    public void EmptySideShouldReturnNoTargets()
    {
        Assert.IsEmpty(TabClosePlanner.GetTargets(Tabs, "A", TabCloseScope.Left));
        Assert.IsEmpty(TabClosePlanner.GetTargets(Tabs, "D", TabCloseScope.Right));
    }

    [TestMethod]
    public void MissingContextTabShouldReturnNoTargets() =>
        Assert.IsEmpty(TabClosePlanner.GetTargets(Tabs, "Missing", TabCloseScope.Others));
}
