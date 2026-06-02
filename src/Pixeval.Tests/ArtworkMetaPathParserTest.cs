using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class ArtworkMetaPathParserTest
{
    [TestMethod]
    public void ValidMacroShouldProduceHighlightsAndAst()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{id}");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Root);
        Assert.AreEqual(4, result.Highlights.Count);
        CollectionAssert.AreEqual(
            new[] { MacroHighlightKind.Delimiter, MacroHighlightKind.Delimiter, MacroHighlightKind.Name, MacroHighlightKind.Delimiter },
            result.Highlights.Select(highlight => highlight.Kind).ToArray());
    }

    [TestMethod]
    public void MissingRightBraceShouldReturnParserDiagnostic()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{id");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MissingRightBrace, result.Diagnostics[0].Kind);
    }

    [TestMethod]
    public void BrokenMacroShouldNotStopLaterHighlighting()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{id @{id}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MissingRightBrace, result.Diagnostics[0].Kind);
        Assert.AreEqual(2, result.Highlights.Count(highlight => highlight.Kind is MacroHighlightKind.Name));
    }

    [TestMethod]
    public void UnknownMacroShouldReturnSemanticDiagnostic()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{missing}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.UnknownMacroName, result.Diagnostics[0].Kind);
        Assert.AreEqual("missing", result.Diagnostics[0].PrimaryParameter);
    }

    [TestMethod]
    public void ReduceShouldUseParsedTree()
    {
        var path = ArtworkMetaPathParser.Instance.Reduce("@{id}", DesignHelper.DownloadParserSampleWork(ImageType.SingleImage));

        Assert.AreEqual("12345678", path);
    }
}
