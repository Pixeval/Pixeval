using System.Collections.Generic;
using Avalonia.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Views.Viewers;
using SmoothScroll.Avalonia.Interaction;

namespace Pixeval.Tests;

[TestClass]
public sealed class ImageViewerScrollGestureProfilesTest
{
    [TestMethod]
    public void Paging_OnlyOverridesUnmodifiedWheelWithZoom()
    {
        var paging = ImageViewerScrollGestureProfiles.Paging;
        Assert.AreEqual(ScrollGestureAction.Zoom, Resolve(paging, ScrollInputGesture.MouseWheel));
        Assert.AreEqual(ScrollGestureAction.HorizontalScroll, Resolve(paging, ScrollInputGesture.MouseWheel, KeyModifiers.Shift));
        Assert.AreEqual(ScrollGestureAction.Zoom, Resolve(paging, ScrollInputGesture.MouseWheel, KeyModifiers.Control));
        Assert.AreEqual(ScrollGestureAction.Pan, Resolve(paging, ScrollInputGesture.MouseLeftDrag));
        Assert.AreEqual(ScrollGestureAction.Pan, Resolve(paging, ScrollInputGesture.TouchDrag));
        Assert.AreEqual(ScrollGestureAction.Zoom, Resolve(paging, ScrollInputGesture.TouchPinch));
    }

    [TestMethod]
    public void Continuous_WheelFollowsStackOrientation()
    {
        var horizontal = ImageViewerScrollGestureProfiles.Horizontal;
        var vertical = ImageViewerScrollGestureProfiles.Vertical;
        Assert.AreEqual(ScrollGestureAction.HorizontalScroll, Resolve(horizontal, ScrollInputGesture.MouseWheel));
        Assert.AreEqual(ScrollGestureAction.AutoScroll, Resolve(vertical, ScrollInputGesture.MouseWheel));
    }

    private static ScrollGestureAction Resolve(
        IReadOnlyDictionary<ScrollGesture, ScrollGestureAction> bindings,
        ScrollInputGesture gesture,
        KeyModifiers modifiers = KeyModifiers.None) =>
        bindings[new ScrollGesture(gesture, modifiers)];
}
