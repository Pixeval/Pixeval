using Avalonia;
using Avalonia.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using GridItemsAlignment = Pixeval.Controls.WrapPanelItemsAlignment;

namespace Pixeval.Tests;

[TestClass]
public sealed class AdaptiveGridTest
{
    private const double Tolerance = 0.05;

    [TestMethod]
    public void Justify_WithFiniteAxes_UsesVirtualLineSlots()
    {
        var grid = CreateGrid(GridItemsAlignment.Justify);
        AddChildren(grid, 4);

        Arrange(grid, new(340, 170));

        Assert.AreEqual(0, grid.Children[3].Bounds.X, Tolerance);
        Assert.AreEqual(59.995, grid.Children[3].Bounds.Y, Tolerance);
        Assert.AreEqual(3, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    [TestMethod]
    [DataRow(GridItemsAlignment.Center, 15, 10)]
    [DataRow(GridItemsAlignment.End, 30, 20)]
    public void PositionalAlignment_WithFiniteAxes_UsesVirtualGridSlots(
        GridItemsAlignment alignment,
        double expectedMinorStart,
        double expectedMajorStart)
    {
        var grid = CreateGrid(alignment);
        AddChildren(grid, 4);

        Arrange(grid, new(350, 250));

        Assert.AreEqual(expectedMinorStart, grid.Children[0].Bounds.X, Tolerance);
        Assert.AreEqual(expectedMajorStart, grid.Children[0].Bounds.Y, Tolerance);
        Assert.AreEqual(expectedMinorStart, grid.Children[3].Bounds.X, Tolerance);
        Assert.AreEqual(expectedMajorStart + 60, grid.Children[3].Bounds.Y, Tolerance);
        Assert.AreEqual(4, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    [TestMethod]
    public void Stretch_WithFiniteMinorAndSingleLine_UsesVirtualItemSlots()
    {
        var grid = CreateGrid(GridItemsAlignment.Stretch);
        AddChildren(grid, 2);

        Arrange(grid, new(350, 50));

        Assert.AreEqual(0, grid.Children[0].Bounds.X, Tolerance);
        Assert.AreEqual(120, grid.Children[1].Bounds.X, Tolerance);
        Assert.AreEqual(109.997, grid.Children[0].Bounds.Width, Tolerance);
        Assert.AreEqual(1, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    [TestMethod]
    public void Stretch_WithFiniteAxes_AlsoAppliesToMajorAxis()
    {
        var grid = CreateGrid(GridItemsAlignment.Stretch);
        AddChildren(grid, 4);

        Arrange(grid, new(350, 250));

        Assert.AreEqual(0, grid.Children[0].Bounds.Y, Tolerance);
        Assert.AreEqual(64.9975, grid.Children[3].Bounds.Y, Tolerance);
        Assert.AreEqual(54.9975, grid.Children[0].Bounds.Height, Tolerance);
        Assert.AreEqual(4, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    [TestMethod]
    public void End_WithFiniteMajorAndSingleDisplayedLine_UsesVirtualLineSlots()
    {
        var grid = CreateGrid(GridItemsAlignment.End);
        AddChildren(grid, 2);

        Arrange(grid, new(350, 250));

        Assert.AreEqual(20, grid.Children[0].Bounds.Y, Tolerance);
        Assert.AreEqual(20, grid.Children[1].Bounds.Y, Tolerance);
        Assert.AreEqual(4, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    [TestMethod]
    [DataRow(GridItemsAlignment.Center, 15, 100)]
    [DataRow(GridItemsAlignment.End, 30, 100)]
    [DataRow(GridItemsAlignment.Justify, 0, 100)]
    [DataRow(GridItemsAlignment.Stretch, 0, 109.997)]
    public void Alignment_WithInfiniteMeasuredMajor_AppliesOnlyToMinorAxis(
        GridItemsAlignment alignment,
        double expectedMinorStart,
        double expectedMinorSize)
    {
        var grid = CreateGrid(alignment);
        AddChildren(grid, 4);

        grid.Measure(new(350, double.PositiveInfinity));
        grid.Arrange(new(new Size(350, 250)));

        Assert.AreEqual(expectedMinorStart, grid.Children[0].Bounds.X, Tolerance);
        Assert.AreEqual(expectedMinorStart, grid.Children[3].Bounds.X, Tolerance);
        Assert.AreEqual(expectedMinorSize, grid.Children[0].Bounds.Width, Tolerance);
        Assert.AreEqual(60, grid.Children[3].Bounds.Y, Tolerance);
        Assert.AreEqual(50, grid.Children[0].Bounds.Height, Tolerance);
        Assert.AreEqual(2, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    [TestMethod]
    public void Stretch_WithInfiniteMeasuredMajorAndSingleLine_AppliesOnlyToMinorAxis()
    {
        var grid = CreateGrid(GridItemsAlignment.Stretch);
        AddChildren(grid, 2);

        grid.Measure(new(350, double.PositiveInfinity));
        grid.Arrange(new(new Size(350, 250)));

        Assert.AreEqual(120, grid.Children[1].Bounds.X, Tolerance);
        Assert.AreEqual(0, grid.Children[1].Bounds.Y, Tolerance);
        Assert.AreEqual(109.997, grid.Children[0].Bounds.Width, Tolerance);
        Assert.AreEqual(50, grid.Children[0].Bounds.Height, Tolerance);
        Assert.AreEqual(1, grid.Lines);
        Assert.AreEqual(3, grid.ItemsPerLine);
    }

    private static AdaptiveGrid CreateGrid(GridItemsAlignment alignment) => new()
    {
        ItemSize = 100,
        ItemSpacing = 10,
        ItemsAlignment = alignment,
        LineSize = 50,
        LineSpacing = 10
    };

    private static void AddChildren(AdaptiveGrid grid, int count)
    {
        for (var i = 0; i < count; ++i)
            grid.Children.Add(new Border());
    }

    private static void Arrange(AdaptiveGrid grid, Size size)
    {
        grid.Measure(size);
        grid.Arrange(new(size));
    }
}
