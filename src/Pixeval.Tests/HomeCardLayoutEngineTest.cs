using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Models.Home;
using Pixeval.Models.Options;

namespace Pixeval.Tests;

[TestClass]
public sealed class HomeCardLayoutEngineTest
{
    [TestMethod]
    public void CanPlace_DistinctCardWithSameValues_DoesNotIgnoreCollision()
    {
        var existing = CreateCard(0, 0);
        var moving = CreateCard(0, 0);

        var result = HomeCardLayoutEngine.CanPlace([existing], moving, HomeCardBounds.From(moving), 2, 2);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanPlace_SameCardReference_IgnoresOriginalBounds()
    {
        var moving = CreateCard(0, 0);

        var result = HomeCardLayoutEngine.CanPlace([moving], moving, new(1, 1, 1, 1), 2, 2);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryFindFreePosition_ScansRowsThenColumns()
    {
        var result = HomeCardLayoutEngine.TryFindFreePosition([CreateCard(0, 0)], 1, 1, 2, 2, out var column, out var row);

        Assert.IsTrue(result);
        Assert.AreEqual(1, column);
        Assert.AreEqual(0, row);
    }

    [TestMethod]
    [DataRow(-1, -1, 4, 4, 0, 0, 3, 2)]
    [DataRow(2, 1, 0, 0, 2, 1, 1, 1)]
    public void Clamp_KeepsBoundsInsideGrid(
        int column,
        int row,
        int columnSpan,
        int rowSpan,
        int expectedColumn,
        int expectedRow,
        int expectedColumnSpan,
        int expectedRowSpan)
    {
        var result = HomeCardLayoutEngine.Clamp(new(column, row, columnSpan, rowSpan), 2, 3);

        Assert.AreEqual(new HomeCardBounds(expectedColumn, expectedRow, expectedColumnSpan, expectedRowSpan), result);
    }

    private static HomePageCardLayout CreateCard(int column, int row) =>
        new(HomePageCardSourceKind.WorkRecommended, column, row);
}
