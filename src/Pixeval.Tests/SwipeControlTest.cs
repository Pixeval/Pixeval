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

    [TestMethod]
    [DataRow(0, 1, false)]
    [DataRow(1, 0, true)]
    [DataRow(1, 1, false)]
    public void TransitionDirection_FollowsNavigationIndex(int oldIndex, int newIndex, bool expected)
    {
        var control = new SwipeControl();

        control.SetTransitionDirection(oldIndex, newIndex);

        Assert.AreEqual(expected, control.IsTransitionReversed);
    }
}
