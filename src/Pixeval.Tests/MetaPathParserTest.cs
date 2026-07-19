using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Models.Database;
using Pixeval.Models.Download;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Tests;

[TestClass]
public sealed class MetaPathParserTest
{
    [TestMethod]
    public void ValidMacroShouldProduceHighlightsAndAst()
    {
        var result = DownloadPathMacroParser.Analyze("@{id}");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Root);
        Assert.HasCount(4, result.Highlights);
        Assert.AreSequenceEqual(
        [
            MacroHighlightKind.Delimiter,
            MacroHighlightKind.Delimiter,
            MacroHighlightKind.Name,
            MacroHighlightKind.Delimiter
        ], result.Highlights.Select(highlight => highlight.Kind).ToArray());
    }

    [TestMethod]
    public void FormattedTransducerShouldProduceFormatterHighlight()
    {
        var result = DownloadPathMacroParser.Analyze("@{ext:u}");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Root);
        Assert.HasCount(6, result.Highlights);
        Assert.AreSequenceEqual(
        [
            MacroHighlightKind.Delimiter,
            MacroHighlightKind.Delimiter,
            MacroHighlightKind.Name,
            MacroHighlightKind.Separator,
            MacroHighlightKind.Formatter,
            MacroHighlightKind.Delimiter
        ], result.Highlights.Select(highlight => highlight.Kind).ToArray());
    }

    [TestMethod]
    public void MissingRightBraceShouldReturnParserDiagnostic()
    {
        var result = DownloadPathMacroParser.Analyze("@{id");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MissingRightBrace, result.Diagnostics[0].Kind);
    }

    [TestMethod]
    public void BrokenMacroShouldNotStopLaterHighlighting()
    {
        var result = DownloadPathMacroParser.Analyze("@{id @{id}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MissingRightBrace, result.Diagnostics[0].Kind);
        Assert.AreEqual(2, result.Highlights.Count(highlight => highlight.Kind is MacroHighlightKind.Name));
    }

    [TestMethod]
    public void UnknownMacroShouldReturnSemanticDiagnostic()
    {
        var result = DownloadPathMacroParser.Analyze("@{missing}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.UnknownMacroName, result.Diagnostics[0].Kind);
        Assert.AreEqual("missing", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void TransducerWithConditionalBranchesShouldReturnSemanticDiagnostic()
    {
        var result = DownloadPathMacroParser.Analyze("@{id?yes:no}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.NonParameterizedMacroBearingParameter, result.Diagnostics[0].Kind);
        Assert.AreEqual("id", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void TransducerShouldValidateFormatter()
    {
        var result = DownloadPathMacroParser.Analyze("@{id:00}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.InvalidFormatter, result.Diagnostics[0].Kind);
        Assert.AreEqual("id", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual("00", result.Diagnostics[0].Arguments[1]);
    }

    [TestMethod]
    public void IntegerTransducerShouldRejectInvalidFormatter()
    {
        var result = DownloadPathMacroParser.Analyze("@{is_pic_set?@{pic_set_index:**}:}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.InvalidFormatter, result.Diagnostics[0].Kind);
        Assert.AreEqual("pic_set_index", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual("**", result.Diagnostics[0].Arguments[1]);
    }

    [TestMethod]
    public void PredicateWithFormatterShouldReturnSemanticDiagnostic()
    {
        var result = DownloadPathMacroParser.Analyze("@{is_pic_set:00?yes:no}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.InvalidFormatter, result.Diagnostics[0].Kind);
        Assert.AreEqual("is_pic_set", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual("00", result.Diagnostics[0].Arguments[1]);
    }

    [TestMethod]
    public void PredicateWithoutConditionalBranchesShouldReturnSemanticDiagnostic()
    {
        var result = DownloadPathMacroParser.Analyze("@{is_pic_set}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.ConditionalBranchesMissing, result.Diagnostics[0].Kind);
        Assert.AreEqual("is_pic_set", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void PicSetIndexShouldBeContainedByPicSetPredicate()
    {
        var result = DownloadPathMacroParser.Analyze("@{pic_set_index}");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MacroContextRestrictionNotSatisfied, result.Diagnostics[0].Kind);
        Assert.AreEqual("pic_set_index", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    [DataRow("is_group")]
    [DataRow("is_bookmark_group")]
    [DataRow("is_post_group")]
    [DataRow("is_series_group")]
    public void GroupIdShouldBeAllowedByAnyGroupPredicate(string predicateName)
    {
        var result = DownloadPathMacroParser.Analyze("@{" + predicateName + "?@{group_id}:}");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void ContextRestrictionDelegateShouldSupportMixedConditions()
    {
        var analyzer = new MetaPathAnalyzer<EmptyParserContext>(
        [
            new TestPredicateMacro("a"),
            new TestPredicateMacro("b"),
            new TestPredicateMacro("c"),
            new TestContextRestrictedMacro()
        ]);

        Assert.IsTrue(analyzer.Analyze("@{a?@{b?@{restricted}:}:}").IsSuccess);
        Assert.IsTrue(analyzer.Analyze("@{a?@{c?@{restricted}:}:}").IsSuccess);
        Assert.IsFalse(analyzer.Analyze("@{a?@{restricted}:}").IsSuccess);
        Assert.IsFalse(analyzer.Analyze("@{b?@{restricted}:}").IsSuccess);
    }

    [TestMethod]
    public void LastSegmentMacroShouldStayInLastSegment()
    {
        var result = DownloadPathMacroParser.Analyze("@{ext}\\name");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(MacroDiagnosticKind.MacroShouldBeInLastSegment, result.Diagnostics[0].Kind);
        Assert.AreEqual("ext", result.Diagnostics[0].Arguments[0]);
    }

    [TestMethod]
    public void ReduceShouldUseParsedTree()
    {
        var path = DownloadPathMacroParser.Reduce("@{id}", new ParserContext(DesignHelper.DownloadParserSampleWork(ImageType.SingleImage)));

        Assert.AreEqual("12345678", path);
    }

    [TestMethod]
    public void ReduceShouldApplyDateTimeFormatter()
    {
        var path = DownloadPathMacroParser.Reduce(
            "@{publish_time:yyyy-MM-dd}",
            new ParserContext(DesignHelper.DownloadParserSampleWork(ImageType.SingleImage)));

        Assert.AreEqual("2020-10-12", path);
    }

    [TestMethod]
    public void PlaceholderMacroShouldStillCountAsEvaluated()
    {
        var path = DownloadPathMacroParser.Reduce("@{ext}", new ParserContext(DesignHelper.DownloadParserSampleWork(ImageType.SingleImage)));

        Assert.AreEqual("<ext>", path);
    }

    [TestMethod]
    public void DefaultDownloadPathShouldProvideExtensionSeparator()
    {
        var settings = new DownloadSettingsGroup();

        Assert.IsTrue(settings.DownloadPathMacro.EndsWith(".@{ext}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FormattedFileExtensionShouldUseExplicitSeparator()
    {
        var path = DownloadPathMacroParser.Reduce("@{ext:u}", new ParserContext(DesignHelper.DownloadParserSampleWork(ImageType.SingleImage)));

        Assert.AreEqual("<ext:u>", path);
        Assert.AreEqual("work.PNG", IoHelper.ChangeExtension("work." + path, ".png"));
        Assert.AreEqual("work.PNG", IoHelper.ChangeExtension("work." + path, "png"));
        Assert.AreEqual("work", IoHelper.RemoveTokenExtension("work." + path));
    }

    [TestMethod]
    public void WorkSubscriptionGroupMacrosShouldMatchSubscriptionType()
    {
        var sample = DesignHelper.DownloadParserSampleWork(ImageType.SingleImage);

        Assert.AreEqual(
            "not-group-not-bookmark-not-post-not-series",
            DownloadPathMacroParser.Reduce(
                "@{is_group?group:not-group}-@{is_bookmark_group?bookmark:not-bookmark}-@{is_post_group?post:not-post}-@{is_series_group?series:not-series}",
                new ParserContext(sample)));
        Assert.AreEqual(
            "group-bookmark-not-post-not-series",
            DownloadPathMacroParser.Reduce(
                "@{is_group?group:not-group}-@{is_bookmark_group?bookmark:not-bookmark}-@{is_post_group?post:not-post}-@{is_series_group?series:not-series}",
                new ParserContext(sample, new() { SubscriptionType = WorkSubscriptionType.Bookmarks })));
        Assert.AreEqual(
            "group-not-bookmark-post-not-series",
            DownloadPathMacroParser.Reduce(
                "@{is_group?group:not-group}-@{is_bookmark_group?bookmark:not-bookmark}-@{is_post_group?post:not-post}-@{is_series_group?series:not-series}",
                new ParserContext(sample, new() { SubscriptionType = WorkSubscriptionType.Posts })));
        Assert.AreEqual(
            "group-not-bookmark-not-post-series",
            DownloadPathMacroParser.Reduce(
                "@{is_group?group:not-group}-@{is_bookmark_group?bookmark:not-bookmark}-@{is_post_group?post:not-post}-@{is_series_group?series:not-series}",
                new ParserContext(sample, new() { SubscriptionType = WorkSubscriptionType.Series })));
    }

    [TestMethod]
    public void GroupIdShouldUseIntegerFormatter()
    {
        var sample = DesignHelper.DownloadParserSampleWork(ImageType.SingleImage);
        var subscription = new WorkSubscriptionEntry
        {
            Id = 42,
            SubscriptionType = WorkSubscriptionType.Posts
        };

        var path = DownloadPathMacroParser.Reduce(
            "@{is_post_group?@{group_id:000}:}",
            new ParserContext(sample, subscription));

        Assert.AreEqual("042", path);
    }

    [TestMethod]
    public void PicSetIndexShouldPreserveFormatterUntilTokenReplacement()
    {
        var sample = DesignHelper.DownloadParserSampleWork(ImageType.ImageSet);
        var path = DownloadPathMacroParser.Reduce(
            "@{is_pic_set?p@{pic_set_index:00}:}.@{ext}",
            new ParserContext(sample));

        Assert.AreEqual("p<pic_set_index:00>.<ext>", path);
        Assert.AreEqual("p00.jpg", IoHelper.ReplaceTokenSetIndex(IoHelper.ChangeExtension(path, ".jpg"), sample.SetIndex));
    }

    [TestMethod]
    public void ReduceShouldThrowSemanticDiagnosticBeforeEvaluation()
    {
        try
        {
            _ = DownloadPathMacroParser.Reduce(
                "@{id?yes:no}",
                new ParserContext(DesignHelper.DownloadParserSampleWork(ImageType.SingleImage)));
            Assert.Fail("Expected macro reduction to throw.");
        }
        catch (MacroParseException exception)
        {
            Assert.AreEqual(MacroParseException.ErrorType.NonParameterizedMacroBearingParameter, exception.Type);
            Assert.AreEqual("id", exception.Parameter[0]);
        }
    }

    [TestMethod]
    public void ReduceShouldThrowWhenParserContextCannotProvideMacroContext()
    {
        try
        {
            var parser = new MetaPathParser<EmptyParserContext>(DownloadPathMacroParser.MacroProvider);
            _ = parser.Reduce("@{id}", new EmptyParserContext());
            Assert.Fail("Expected incomplete macro reduction to throw.");
        }
        catch (MacroParseException exception)
        {
            Assert.AreEqual(MacroParseException.ErrorType.ReductionNotCompleted, exception.Type);
            Assert.AreEqual("id", exception.Parameter[0]);
        }
    }

    private sealed record EmptyParserContext;

    private sealed class TestPredicateMacro(string name) : IPredicate<EmptyParserContext>
    {
        public string Name { get; } = name;

        public string Description => "";

        public bool Match(EmptyParserContext context) => true;
    }

    private sealed class TestContextRestrictedMacro : ITransducer<EmptyParserContext>, IContextRestrictedMacro
    {
        public string Name => "restricted";

        public string Description => "";

        public MacroContextPredicate ContextPredicate => static context =>
            context.TryGetValue("a", out var a) && a
            && (context.TryGetValue("b", out var b) && b
                || context.TryGetValue("c", out var c) && c);

        public bool IsFormatterValid(string? formatter) => true;

        public string Substitute(EmptyParserContext context, string? formatter, out bool includeToken)
        {
            includeToken = false;
            return "restricted";
        }
    }
}
