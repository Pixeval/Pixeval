using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
public sealed class FloatingDockHostTest
{
    [TestMethod]
    [DataRow(Dock.Left)]
    [DataRow(Dock.Bottom)]
    public void DockedLayoutReservesSpaceOnTheSelectedSide(Dock dockPosition)
    {
        var host = new FloatingDockHost
        {
            DockPosition = dockPosition,
            DockProgress = 1,
            DockedPaneSize = 100,
            IsDocked = true,
        };
        host.Children.Add(new Border());
        host.Children.Add(new Border());

        var finalSize = new Size(800, 600);
        host.Measure(finalSize);
        host.Arrange(new Rect(finalSize));

        var content = host.Children[0];
        var pane = host.Children[1];

        if (dockPosition is Dock.Left)
        {
            Assert.AreEqual(new Rect(100, 0, 700, 600), content.Bounds);
            Assert.AreEqual(new Rect(0, 0, 100, 600), pane.Bounds);
        }
        else
        {
            Assert.AreEqual(new Rect(0, 0, 800, 500), content.Bounds);
            Assert.AreEqual(new Rect(0, 500, 800, 100), pane.Bounds);
        }
    }

    [TestMethod]
    [DataRow(Dock.Left)]
    [DataRow(Dock.Right)]
    [DataRow(Dock.Top)]
    [DataRow(Dock.Bottom)]
    public void FloatingLayoutUsesPaneAlignmentRegardlessOfDockPosition(Dock dockPosition)
    {
        var host = new FloatingDockHost
        {
            DockPosition = dockPosition,
            DockProgress = 0,
            FloatingPaneHorizontalAlignment = HorizontalAlignment.Right,
            FloatingPaneMargin = 20,
            FloatingPaneVerticalAlignment = VerticalAlignment.Top,
            FloatingPaneWidth = 200,
        };
        host.Children.Add(new Border());
        host.Children.Add(new Border
        {
            Height = 100,
            Width = 200,
        });

        var finalSize = new Size(800, 600);
        host.Measure(finalSize);
        host.Arrange(new Rect(finalSize));

        Assert.AreEqual(new Rect(580, 20, 200, 100), host.Children[1].Bounds);
    }
}
