using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Views.Viewers;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Tests;

[TestClass]
public sealed class ImageViewerGeometryTest
{
    [TestMethod]
    public void ReplacementDuringInitialFitRetargetsPendingView()
    {
        var pending = new SingleImageViewer.PendingView(new(100, 50), new(0.5, 0.5), 8, Animated: true);
        var replacement = pending.Resize(new(1000, 500));
        Assert.AreEqual(new Size(1000, 500), replacement.Size);
        Assert.AreEqual(new Point(0.5, 0.5), replacement.Anchor);
        Assert.AreEqual(0.8, replacement.ZoomFactor, 0.000001);
        Assert.IsFalse(replacement.Animated);
    }

    [TestMethod]
    public void MultipleReplacementsKeepTheOriginalFitTarget()
    {
        var pending = new SingleImageViewer.PendingView(new(100, 50), new(0.5, 0.5), 8, Animated: true);
        var replacement = pending.Resize(new(1000, 500)).Resize(new(4000, 2000));
        Assert.AreEqual(new Size(800, 400), replacement.Size * replacement.ZoomFactor);
        Assert.IsFalse(replacement.Animated);
    }

    [TestMethod]
    public void ResolutionChangesRetainVisualSizeAndPanPosition()
    {
        var viewport = new Size(800, 600);
        var oldSize = new Size(1000, 500);
        var offset = new Vector(250, 100);
        var anchor = SingleImageViewer.GetViewportAnchor(oldSize, viewport, offset, 1.5);

        foreach (var imageSize in new Size[] { new(100, 50), new(1000, 500), new(4000, 2000) })
        {
            var zoom = SingleImageViewer.GetReplacementZoomFactor(1.5, oldSize, imageSize);
            Assert.AreEqual(new Size(1500, 750), imageSize * zoom);
            Assert.AreEqual(offset, SingleImageViewer.GetAnchorOffset(anchor, imageSize, viewport, zoom));
        }
    }

    [TestMethod]
    public void ClampedReplacementKeepsViewportAnchorWithoutChangingLimits()
    {
        var viewport = new Size(800, 600);
        var oldSize = new Size(1000, 500);
        var newSize = new Size(40000, 20000);
        var anchor = SingleImageViewer.GetViewportAnchor(oldSize, viewport, new(250, 100), 1.5);
        var scrollView = new ScrollView();
        var minimum = scrollView.MinZoomFactor;
        var maximum = scrollView.MaxZoomFactor;
        _ = scrollView.ZoomTo(SingleImageViewer.GetReplacementZoomFactor(1.5, oldSize, newSize), false);
        Assert.AreEqual(minimum, scrollView.ZoomFactor);
        Assert.AreEqual(minimum, scrollView.MinZoomFactor);
        Assert.AreEqual(maximum, scrollView.MaxZoomFactor);
        var offset = SingleImageViewer.GetAnchorOffset(anchor, newSize, viewport, scrollView.ZoomFactor);
        var restored = SingleImageViewer.GetViewportAnchor(newSize, viewport, offset, scrollView.ZoomFactor);
        Assert.AreEqual(anchor.X, restored.X, 0.000001);
        Assert.AreEqual(anchor.Y, restored.Y, 0.000001);
    }

    [TestMethod]
    public void AspectRatioChangesPreserveNormalizedViewportCenter()
    {
        var viewport = new Size(800, 600);
        var oldSize = new Size(1000, 500);
        var newSize = new Size(1000, 1000);
        var anchor = SingleImageViewer.GetViewportAnchor(oldSize, viewport, new(500, 300), 3);
        var zoom = SingleImageViewer.GetReplacementZoomFactor(3, oldSize, newSize);
        Assert.AreEqual(1.5, zoom);
        var offset = SingleImageViewer.GetAnchorOffset(anchor, newSize, viewport, zoom);
        Assert.AreEqual(new Vector(50, 300), offset);
    }

    [TestMethod]
    public void CenteredImageStaysCenteredWhenItStartsSmallerThanViewport()
    {
        var viewport = new Size(800, 600);
        var oldSize = new Size(100, 50);
        var newSize = new Size(1000, 500);
        var anchor = SingleImageViewer.GetViewportAnchor(oldSize, viewport, default, 1);
        Assert.AreEqual(new Point(0.5, 0.5), anchor);
        Assert.AreEqual(default(Vector), SingleImageViewer.GetAnchorOffset(anchor, newSize, viewport, 0.1));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ContinuousPageLayoutDoesNotGrowWithItsImageResolution(bool vertical)
    {
        var image = new ImageStub();
        var constraint = vertical ? new Size(800, double.PositiveInfinity) : new Size(double.PositiveInfinity, 600);
        var expectedSize = vertical ? new Size(800, 1600) : new Size(300, 600);

        foreach (var imageSize in new Size[] { new(100, 200), new(1000, 2000), new(4000, 8000) })
        {
            image.SourceSize = imageSize;
            image.InvalidateMeasure();
            image.Measure(constraint);
            Assert.AreEqual(expectedSize, image.DesiredSize);
        }
    }

    private sealed class ImageStub : Control
    {
        public Size SourceSize { get; set; }

        protected override Size MeasureOverride(Size availableSize) => Stretch.Uniform.CalculateSize(availableSize, SourceSize);
    }
}
