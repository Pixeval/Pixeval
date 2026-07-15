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
        Assert.AreEqual(ScrollGestureAction.Zoom, Resolve(ImageViewerScrollGestureProfiles.Paging, ScrollInputGesture.MouseWheel));
        Assert.AreEqual(ScrollGestureAction.HorizontalScroll, Resolve(ImageViewerScrollGestureProfiles.Paging, ScrollInputGesture.MouseWheel, KeyModifiers.Shift));
        Assert.AreEqual(ScrollGestureAction.Zoom, Resolve(ImageViewerScrollGestureProfiles.Paging, ScrollInputGesture.MouseWheel, KeyModifiers.Control));
        Assert.AreEqual(ScrollGestureAction.Pan, Resolve(ImageViewerScrollGestureProfiles.Paging, ScrollInputGesture.MouseLeftDrag));
        Assert.AreEqual(ScrollGestureAction.Pan, Resolve(ImageViewerScrollGestureProfiles.Paging, ScrollInputGesture.TouchDrag));
        Assert.AreEqual(ScrollGestureAction.Zoom, Resolve(ImageViewerScrollGestureProfiles.Paging, ScrollInputGesture.TouchPinch));
    }

    [TestMethod]
    public void Continuous_WheelFollowsStackOrientation()
    {
        Assert.AreEqual(ScrollGestureAction.HorizontalScroll, Resolve(ImageViewerScrollGestureProfiles.Horizontal, ScrollInputGesture.MouseWheel));
        Assert.AreEqual(ScrollGestureAction.AutoScroll, Resolve(ImageViewerScrollGestureProfiles.Vertical, ScrollInputGesture.MouseWheel));
    }

    private static ScrollGestureAction Resolve(
        IReadOnlyDictionary<ScrollGesture, ScrollGestureAction> bindings,
        ScrollInputGesture gesture,
        KeyModifiers modifiers = KeyModifiers.None) =>
        bindings[new ScrollGesture(gesture, modifiers)];
}
