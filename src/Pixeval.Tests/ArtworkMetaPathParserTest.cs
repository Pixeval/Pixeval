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
        Assert.HasCount(4, result.Highlights);
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
        Assert.AreEqual("missing", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void TransducerWithConditionalBranchesShouldReturnSemanticDiagnostic()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{id?yes:no}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.NonParameterizedMacroBearingParameter, result.Diagnostics[0].Kind);
        Assert.AreEqual("id", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void PredicateWithoutConditionalBranchesShouldReturnSemanticDiagnostic()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{if_pic_set}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.ConditionalBranchesMissing, result.Diagnostics[0].Kind);
        Assert.AreEqual("if_pic_set", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void PicSetIndexShouldBeContainedByPicSetPredicate()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{pic_set_index}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MacroShouldBeContained, result.Diagnostics[0].Kind);
        Assert.AreEqual("pic_set_index", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual("if_pic_set", result.Diagnostics[0].Arguments[1]);
    }

    [TestMethod]
    public void LastSegmentMacroShouldStayInLastSegment()
    {
        var result = ArtworkMetaPathAnalyzer.Analyze("@{ext}\\name");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MacroShouldBeInLastSegment, result.Diagnostics[0].Kind);
        Assert.AreEqual("ext", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void ReduceShouldUseParsedTree()
    {
        var path = ArtworkMetaPathParser.Instance.Reduce("@{id}", DesignHelper.DownloadParserSampleWork(ImageType.SingleImage));

        Assert.AreEqual("12345678", path);
    }

    [TestMethod]
    public void ReduceShouldThrowSemanticDiagnosticBeforeEvaluation()
    {
        try
        {
            _ = ArtworkMetaPathParser.Instance.Reduce(
                "@{id?yes:no}",
                DesignHelper.DownloadParserSampleWork(ImageType.SingleImage));
            Assert.Fail("Expected macro reduction to throw.");
        }
        catch (MacroParseException exception)
        {
            Assert.AreEqual(MacroParseException.ErrorType.NonParameterizedMacroBearingParameter, exception.Type);
            Assert.AreEqual("id", exception.Parameter[0]);
        }
    }
}
