using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Filters;

namespace Pixeval.Tests;

[TestClass]
public sealed class FilterLanguageTest
{
    private static readonly FilterLanguage _Language = new([
        new TitleSyntax(),
        new AuthorSyntax(),
        new TagSyntax(),
        new BookmarkSyntax(),
        new IndexSyntax(),
        new GifSyntax()
    ],
    [
        new("builtin.and", "and", "and", "逻辑与分组"),
        new("builtin.or", "or", "or", "逻辑或分组"),
        new("builtin.not", "!", "!", "逻辑非")
    ]);

    [TestMethod]
    public void NumericTextAfterTagShouldRemainString()
    {
        var result = _Language.Analyze("#123");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Query);
        Assert.HasCount(1, result.Query.Root.Children);
        var predicate = (FilterPredicateNode)result.Query.Root.Children.Single();
        var text = (FilterTextValue)predicate.Value!;
        Assert.AreEqual("Tag", predicate.Syntax.Key);
        Assert.AreEqual("123", text.ToString());
    }

    [TestMethod]
    public void MissingTagValueShouldReturnTagCompletion()
    {
        var result = _Language.Analyze("#", 1);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.Contains(result.Completions.Select(t => t.DisplayText).ToArray(), "#tag");
    }

    [TestMethod]
    public void TagValueCompletionProviderShouldOverrideSyntaxExample()
    {
        var result = _Language.Analyze(
            "#to",
            3,
            static context => context.Match.Syntax.Key == "Tag"
                ? new FilterCompletionDefinition[]
                {
                    new("tag:touhou", "touhou", "touhou", "东方")
                }
                : []);

        Assert.IsTrue(result.Completions.Any(t => t.DisplayText == "touhou"));
        Assert.IsFalse(result.Completions.Any(t => t.DisplayText == "#tag"));
        var completion = result.Completions.Single(t => t.DisplayText == "touhou");
        Assert.AreEqual("touhou", completion.InsertText);
        Assert.AreEqual(FilterTextSpan.FromBounds(1, 3), completion.ReplacementSpan);
    }

    [TestMethod]
    public void AuthorValueCompletionProviderShouldOverrideSyntaxExample()
    {
        var result = _Language.Analyze(
            "artist:sa",
            9,
            static context => context.Match.Syntax.Key == "Author"
                ? new FilterCompletionDefinition[]
                {
                    new("author:saberiii", "saberiii", "saberiii")
                }
                : []);

        Assert.IsTrue(result.Completions.Any(t => t.DisplayText == "saberiii"));
        Assert.IsFalse(result.Completions.Any(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase)));
        var completion = result.Completions.Single(t => t.DisplayText == "saberiii");
        Assert.AreEqual(FilterTextSpan.FromBounds(7, 9), completion.ReplacementSpan);
    }

    [TestMethod]
    public void MissingConstraintValueShouldReturnFlagCompletion()
    {
        var result = _Language.Analyze("+", 1);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.Contains(result.Completions.Select(t => t.DisplayText).ToArray(), "+gif");
    }

    [TestMethod]
    public void AliasCompletionShouldOnlyShowOneItemPerSyntax()
    {
        var result = _Language.Analyze(string.Empty, 0);

        Assert.IsTrue(result.IsSuccess);
        Assert.ContainsSingle(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase), result.Completions);
    }

    [TestMethod]
    public void EmptyInputShouldReturnIntrinsicAndRegisteredCompletions()
    {
        var result = _Language.Analyze(string.Empty, 0);

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.IsSubsetOf(new[] { "and", "or", "!" }, result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.Any(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase)));
        Assert.AreEqual("逻辑与分组", result.Completions.Single(t => t.DisplayText == "and").Description);
    }

    [TestMethod]
    public void RightParenthesisShouldStillShowAllCompletions()
    {
        var result = _Language.Analyze("keyword)", 8);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.IsSubsetOf(new[] { "and", "or", "!" }, result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.Any(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void NegationInputShouldShowUnprefixedDisplayText()
    {
        var result = _Language.Analyze("!", 1);

        Assert.IsFalse(result.IsSuccess);
        var authorCompletion = result.Completions.Single(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(authorCompletion.DisplayText.StartsWith("!", StringComparison.Ordinal));
        Assert.IsTrue(authorCompletion.InsertText.StartsWith("!", StringComparison.Ordinal));
        CollectionAssert.IsSubsetOf(new[] { "and", "or", "!" }, result.Completions.Select(t => t.DisplayText).ToArray());
    }

    [TestMethod]
    public void LeftParenthesisShouldIgnoreTypedFragmentAndShowAllCompletions()
    {
        var result = _Language.Analyze("(xxx", 4);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.IsSubsetOf(new[] { "and", "or", "!" }, result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.Any(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void IndexRangeShouldUseOneBasedConversion()
    {
        var result = _Language.Analyze("i:1-3");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Query);
        Assert.AreEqual(0, result.Query.ViewRange.Start.Value);
        Assert.AreEqual(3, result.Query.ViewRange.End.Value);
    }

    [TestMethod]
    public void BookmarkRangeShouldRemainInclusive()
    {
        var result = _Language.Analyze("l:100-200");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Query);
        var predicate = (FilterPredicateNode)result.Query.Root.Children.Single();
        var range = (FilterLongRange)predicate.Value!;
        Assert.AreEqual(100L, range.Start);
        Assert.AreEqual(200L, range.End);
        Assert.IsTrue(range.Contains(200));
        Assert.IsFalse(range.Contains(99));
    }

    [TestMethod]
    public void IntervalRangeSyntaxShouldBeRejected()
    {
        var result = _Language.Analyze("l:[100,200]");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FilterDiagnosticKind.InvalidLongRangeFormat, result.Diagnostics[0].Kind);
    }

    private sealed class TitleSyntax : FilterTextSyntax
    {
        public override string Key => "Title";

        public override string? ExampleValue => "keyword";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Default("keyword")];
    }

    private sealed class TagSyntax : FilterTextSyntax
    {
        public override string Key => "Tag";

        public override string? ExampleValue => "tag";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.PrefixOnly("#", "tag")];
    }

    private sealed class AuthorSyntax : FilterTextSyntax
    {
        public override string Key => "Author";

        public override string? ExampleValue => "artist";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.Keyword("a", exampleValue: "artist"),
            FilterSyntaxPattern.Keyword("artist", exampleValue: "artist")
        ];
    }

    private sealed class BookmarkSyntax : FilterLongRangeSyntax
    {
        protected override FilterLongRangeBindingMode BindingMode => FilterLongRangeBindingMode.Inclusive;

        public override string Key => "Bookmark";

        public override string? ExampleValue => "100-200";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("l", exampleValue: "100-200")];
    }

    private sealed class IndexSyntax : FilterLongRangeSyntax
    {
        protected override FilterLongRangeBindingMode BindingMode => FilterLongRangeBindingMode.OneBasedIndex;

        public override string Key => "Index";

        public override FilterTermRole Role => FilterTermRole.ViewRange;

        public override string? ExampleValue => "1-3";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("i", exampleValue: "1-3")];
    }

    private sealed class GifSyntax : FilterFlagSyntax
    {
        public override string Key => "Gif";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["gif"], Metadata: false),
            new("-", ["gif"], Metadata: true)
        ];
    }
}
