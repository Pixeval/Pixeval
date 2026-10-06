using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
public sealed class VirtualizingStackPanelTest
{
    [TestMethod]
    public void RemovalThenInsertionBeforeMeasureDoesNotAccessAnEmptyCache()
    {
        var panel = new TestPanel();
        var items = new List<object?> { "first", "removed", "last" };
        panel.NotifyItemsChanged(items, new(NotifyCollectionChangedAction.Reset));
        items.RemoveAt(1);
        panel.NotifyItemsChanged(items, new(NotifyCollectionChangedAction.Remove, "removed", 1));
        items.Insert(1, "inserted");
        panel.NotifyItemsChanged(items, new(NotifyCollectionChangedAction.Add, "inserted", 1));
        items.Insert(2, "second insertion");
        panel.NotifyItemsChanged(items, new(NotifyCollectionChangedAction.Add, "second insertion", 2));
    }

    [TestMethod]
    [DataRow(2, 200, 100, 400, 300)]
    [DataRow(0.5, 800, 400, 1600, 1200)]
    public void EffectiveViewportIsConvertedToUnscaledCoordinates(
        double scale,
        double expectedX,
        double expectedY,
        double expectedWidth,
        double expectedHeight)
    {
        var viewport = VirtualizingStackPanel.ToUnscaledViewport(new Rect(400, 200, 800, 600), scale);

        Assert.AreEqual(new Rect(expectedX, expectedY, expectedWidth, expectedHeight), viewport);
    }

    [TestMethod]
    public void LiveScrollViewportTakesPrecedenceOverStaleEffectiveViewport()
    {
        var viewport = VirtualizingStackPanel.ResolveUnscaledViewport(
            new Rect(50, 25, 400, 300),
            new Vector(600, 300),
            new Size(800, 600),
            2);

        Assert.AreEqual(new Rect(300, 150, 400, 300), viewport);
    }

    [TestMethod]
    public void EffectiveViewportIsUsedUntilLiveScrollViewportIsAvailable()
    {
        var viewport = VirtualizingStackPanel.ResolveUnscaledViewport(
            new Rect(400, 200, 800, 600),
            default,
            default,
            2);

        Assert.AreEqual(new Rect(200, 100, 400, 300), viewport);
    }

    private sealed class TestPanel : VirtualizingStackPanel
    {
        public void NotifyItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e) => OnItemsChanged(items, e);
    }
}
