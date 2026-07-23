using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
public sealed class SwipeControlTest
{
    [TestMethod]
    public void StableFocusTarget_IsFocusableButNotInTabOrder()
    {
        var control = new SwipeControl();

        Assert.IsTrue(control.Focusable);
        Assert.IsFalse(control.IsTabStop);
    }
}
