using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Filters;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Syntax;
using Pixeval.Filters.Text;
using Pixeval.Filters.Values;

namespace Pixeval.Tests;

[TestClass]
public sealed class FilterLanguageTest
{
    private static readonly FilterLanguage _Language = new([
        new TitleSyntax(),
        new AuthorSyntax(),
        new TagSyntax(),
        new BookmarkSyntax(),
        new ScoreSyntax(),
        new WeightSyntax(),
        new StartDateSyntax(),
        new AiSyntax(),
        new R18Syntax(),
        new R18GSyntax(),
        new GifSyntax()
    ],
    [
        new("builtin.and", "and", "and", "逻辑与分组"),
        new("builtin.or", "or", "or", "逻辑或分组"),
        new("builtin.not", "!", "!", "逻辑非")
    ],
    [
        new("constraint.include", "+ai", "+ai", "正约束", CoveredSyntaxPrefixes: ["+"]),
        new("constraint.exclude", "-ai", "-ai", "反约束", CoveredSyntaxPrefixes: ["-"])
    ],
    new Dictionary<FilterValueKind, IReadOnlyCollection<FilterCompletionDefinition>>
    {
        [FilterValueKind.Text] =
        [
            new("hint.text.plain", "abc", "", "普通的字符串"),
            new("hint.text.quoted", "\"ab# c\"", "", "带空格或转义字符的字符串")
        ],
        [FilterValueKind.Long] =
        [
            new("hint.long.plain", "12345", "", "普通的整数")
        ],
        [FilterValueKind.Double] =
        [
            new("hint.double.integer", "2", "", "整数形式的小数值"),
            new("hint.double.decimal", "1.5", "", "小数形式的小数值"),
            new("hint.double.fraction", "1/2", "", "分数形式的小数值")
        ],
        [FilterValueKind.LongRange] =
        [
            new("hint.long-range.lower", "2-", "", "大于等于 2"),
            new("hint.long-range.upper", "-3", "", "小于等于 3"),
            new("hint.long-range.closed", "2-3", "", "大于等于 2 且小于等于 3")
        ],
        [FilterValueKind.DoubleRange] =
        [
            new("hint.double-range.lower", "2-", "", "大于等于 2"),
            new("hint.double-range.upper-fraction", "-1/2", "", "小于等于 1/2")
        ],
        [FilterValueKind.Date] =
        [
            new("hint.date.month-day-dash", "MM-dd", "", "今年某月某日"),
            new("hint.date.full-dash", "yyyy-MM-dd", "", "某年某月某日")
        ]
    });

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
        Assert.AreEqual(FilterDiagnosticKind.MissingTextValue, result.Diagnostics[0].Kind);
        Assert.AreEqual("#", result.Diagnostics[0].Arguments[0]);
        CollectionAssert.AreEquivalent((string[]) ["abc", "\"ab# c\""], result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.All(t => t.IsHintOnly));
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

        Assert.Contains(t => t.DisplayText == "touhou", result.Completions);
        Assert.DoesNotContain(t => t.DisplayText == "#tag", result.Completions);
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

        Assert.Contains(t => t.DisplayText == "saberiii", result.Completions);
        Assert.DoesNotContain(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase), result.Completions);
        var completion = result.Completions.Single(t => t.DisplayText == "saberiii");
        Assert.AreEqual(FilterTextSpan.FromBounds(7, 9), completion.ReplacementSpan);
    }

    [TestMethod]
    public void MissingConstraintValueShouldReturnFlagCompletion()
    {
        var result = _Language.Analyze("+", 1);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.AreEquivalent((string[]) ["+ai", "+r18", "+r18g", "+gif"], result.Completions.Select(t => t.DisplayText).ToArray());
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
        CollectionAssert.IsSubsetOf((string[]) ["and", "or", "!"], result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.Contains(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase), result.Completions);
        var andCompletion = result.Completions.Single(t => t.DisplayText == "and");
        var orCompletion = result.Completions.Single(t => t.DisplayText == "or");
        Assert.AreEqual("(and", andCompletion.InsertText);
        Assert.AreEqual("(or", orCompletion.InsertText);
        Assert.AreEqual("逻辑与分组", andCompletion.Description);
    }

    [TestMethod]
    public void SyntaxExampleCompletionsShouldOnlyInsertHeader()
    {
        var result = _Language.Analyze(string.Empty, 0);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("", result.Completions.Single(t => t.DisplayText == "keyword").InsertText);
        Assert.AreEqual("a:", result.Completions.Single(t => t.DisplayText == "a:artist").InsertText);
        Assert.AreEqual("#", result.Completions.Single(t => t.DisplayText == "#tag").InsertText);
        Assert.AreEqual("l:", result.Completions.Single(t => t.DisplayText == "l:100-200").InsertText);
        Assert.AreEqual("s:", result.Completions.Single(t => t.DisplayText == "s:2024-1-1").InsertText);
        Assert.AreEqual("+ai", result.Completions.Single(t => t.DisplayText == "+ai").InsertText);
        Assert.AreEqual("-ai", result.Completions.Single(t => t.DisplayText == "-ai").InsertText);
    }

    [TestMethod]
    public void ValueSyntaxHintsShouldNotInsertText()
    {
        AssertValueHintsOnly("#", ["abc", "\"ab# c\""]);
        AssertValueHintsOnly("a:", ["abc", "\"ab# c\""]);
        AssertValueHintsOnly("score:", ["12345"]);
        AssertValueHintsOnly("weight:", ["2", "1.5", "1/2"]);
        AssertValueHintsOnly("l:", ["2-", "-3", "2-3"]);
        AssertValueHintsOnly("s:", ["MM-dd", "yyyy-MM-dd"]);
    }

    [TestMethod]
    public void ValueSyntaxHintsShouldIgnoreTypedPrefix()
    {
        var result = _Language.Analyze("s:202", 5);

        CollectionAssert.AreEquivalent((string[]) ["MM-dd", "yyyy-MM-dd"], result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.All(t => t.InsertText == "s:202"));
        Assert.IsTrue(result.Completions.All(t => t.IsHintOnly));
    }

    [TestMethod]
    public void ValueSyntaxHintsShouldSupportDoubleRange()
    {
        var language = new FilterLanguage(
            [new RatioSyntax()],
            valueHintCompletions: new Dictionary<FilterValueKind, IReadOnlyCollection<FilterCompletionDefinition>>
            {
                [FilterValueKind.DoubleRange] =
                [
                    new("hint.double-range.lower", "2-", ""),
                    new("hint.double-range.upper-fraction", "-1/2", "")
                ]
            });
        var result = language.Analyze("r:", 2);

        CollectionAssert.AreEquivalent((string[]) ["2-", "-1/2"], result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.All(t => t.InsertText == "r:"));
        Assert.IsTrue(result.Completions.All(t => t.IsHintOnly));
    }

    [TestMethod]
    public void LongValueSyntaxShouldBindLong()
    {
        var result = _Language.Analyze("score:12345");

        Assert.IsTrue(result.IsSuccess);
        var predicate = (FilterPredicateNode) result.Query!.Root.Children.Single();
        Assert.AreEqual("Score", predicate.Syntax.Key);
        Assert.AreEqual(12345L, predicate.Value);
    }

    [TestMethod]
    public void DoubleValueSyntaxShouldBindDouble()
    {
        var result = _Language.Analyze("weight:1/2");

        Assert.IsTrue(result.IsSuccess);
        var predicate = (FilterPredicateNode) result.Query!.Root.Children.Single();
        Assert.AreEqual("Weight", predicate.Syntax.Key);
        Assert.AreEqual(0.5d, (double) predicate.Value!, 0.000001d);
    }

    [TestMethod]
    public void EmptyInputShouldGroupFlagConstraintCompletions()
    {
        var result = _Language.Analyze(string.Empty, 0);
        var displayTexts = result.Completions.Select(t => t.DisplayText).ToArray();

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.Contains(displayTexts, "+ai");
        CollectionAssert.Contains(displayTexts, "-ai");
        CollectionAssert.DoesNotContain(displayTexts, "+r18");
        CollectionAssert.DoesNotContain(displayTexts, "+r18g");
        CollectionAssert.DoesNotContain(displayTexts, "+gif");
        CollectionAssert.DoesNotContain(displayTexts, "-r18");
        CollectionAssert.DoesNotContain(displayTexts, "-r18g");
        CollectionAssert.DoesNotContain(displayTexts, "-gif");
        Assert.AreEqual("正约束", result.Completions.Single(t => t.DisplayText == "+ai").Description);
        Assert.AreEqual("反约束", result.Completions.Single(t => t.DisplayText == "-ai").Description);
    }

    [TestMethod]
    public void PrefixConstraintInputShouldShowSpecificFlagCompletions()
    {
        var includeResult = _Language.Analyze("+", 1);
        var excludeResult = _Language.Analyze("-", 1);

        Assert.IsFalse(includeResult.IsSuccess);
        CollectionAssert.AreEquivalent((string[]) ["+ai", "+r18", "+r18g", "+gif"], includeResult.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsFalse(excludeResult.IsSuccess);
        CollectionAssert.AreEquivalent((string[]) ["-ai", "-r18", "-r18g", "-gif"], excludeResult.Completions.Select(t => t.DisplayText).ToArray());
    }

    [TestMethod]
    public void RightParenthesisShouldStillShowAllCompletions()
    {
        var result = _Language.Analyze("keyword)", 8);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FilterDiagnosticKind.UnexpectedToken, result.Diagnostics[0].Kind);
        Assert.AreEqual(")", result.Diagnostics[0].Arguments[0]);
        CollectionAssert.IsSubsetOf((string[]) ["and", "or", "!"], result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.Contains(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase), result.Completions);
    }

    [TestMethod]
    public void NegationInputShouldShowUnprefixedDisplayText()
    {
        var result = _Language.Analyze("!", 1);

        Assert.IsFalse(result.IsSuccess);
        var authorCompletion = result.Completions.Single(t => t.DisplayText.Contains("artist", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(authorCompletion.DisplayText.StartsWith('!'));
        Assert.IsTrue(authorCompletion.InsertText.StartsWith('!'));
        CollectionAssert.IsSubsetOf((string[]) ["and", "or", "!"], result.Completions.Select(t => t.DisplayText).ToArray());
    }

    [TestMethod]
    public void LeftParenthesisShouldOnlyShowGroupOperatorCompletions()
    {
        var result = _Language.Analyze("(xxx", 4);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.AreEquivalent((string[]) ["and", "or"], result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.AreEqual("and", result.Completions.Single(t => t.DisplayText == "and").InsertText);
        Assert.AreEqual("or", result.Completions.Single(t => t.DisplayText == "or").InsertText);
    }

    [TestMethod]
    public void LeftParenthesisShouldNotEnterValueCompletion()
    {
        var result = _Language.Analyze(
            "(#",
            2,
            static context => context.Match.Syntax.Key == "Tag"
                ? new FilterCompletionDefinition[]
                {
                    new("tag:touhou", "touhou", "touhou")
                }
                : []);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.AreEquivalent((string[]) ["and", "or"], result.Completions.Select(t => t.DisplayText).ToArray());
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
    public void MissingRangeDashShouldIncludeSyntaxAndLiteral()
    {
        var result = _Language.Analyze("l:100");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FilterDiagnosticKind.InvalidLongRangeFormat, result.Diagnostics[0].Kind);
        Assert.AreEqual("l:", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual("100", result.Diagnostics[0].Arguments[1]);
    }

    [TestMethod]
    public void RangeOrderDiagnosticShouldIncludeSyntaxAndBounds()
    {
        var result = _Language.Analyze("l:200-100");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FilterDiagnosticKind.RangeMinimumGreaterThanMaximum, result.Diagnostics[0].Kind);
        Assert.AreEqual("l:", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual(200L, result.Diagnostics[0].Arguments[1]);
        Assert.AreEqual(100L, result.Diagnostics[0].Arguments[2]);
    }

    [TestMethod]
    public void InvalidDateShouldIncludeSyntaxAndDateLiteral()
    {
        var result = _Language.Analyze("s:2024-2-31");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FilterDiagnosticKind.InvalidDate, result.Diagnostics[0].Kind);
        Assert.AreEqual("s:", result.Diagnostics[0].Arguments[0]);
        Assert.AreEqual("2024-2-31", result.Diagnostics[0].Arguments[1]?.ToString());
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
        public override string Key => "Bookmark";

        public override string? ExampleValue => "100-200";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("l", exampleValue: "100-200")];
    }

    private sealed class ScoreSyntax : FilterLongSyntax
    {
        public override string Key => "Score";

        public override string? ExampleValue => "12345";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("score", exampleValue: "12345")];
    }

    private sealed class WeightSyntax : FilterDoubleSyntax
    {
        public override string Key => "Weight";

        public override string? ExampleValue => "1/2";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("weight", exampleValue: "1/2")];
    }

    private sealed class RatioSyntax : FilterDoubleRangeSyntax
    {
        public override string Key => "Ratio";

        public override string? ExampleValue => "1-2";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("r", exampleValue: "1-2")];
    }

    private sealed class StartDateSyntax : FilterDateSyntax
    {
        public override string Key => "StartDate";

        public override string? ExampleValue => "2024-1-1";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } = [FilterSyntaxPattern.Keyword("s", exampleValue: "2024-1-1")];
    }

    private sealed class AiSyntax : FilterFlagSyntax
    {
        public override string Key => "Ai";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["ai"], Metadata: false, Description: "仅显示 AI"),
            new("-", ["ai"], Metadata: true, Description: "排除 AI")
        ];
    }

    private sealed class R18Syntax : FilterFlagSyntax
    {
        public override string Key => "R18";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["r18"], Metadata: false, Description: "仅显示 R18"),
            new("-", ["r18"], Metadata: true, Description: "排除 R18")
        ];
    }

    private sealed class R18GSyntax : FilterFlagSyntax
    {
        public override string Key => "R18G";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["r18g"], Metadata: false, Description: "仅显示 R18G"),
            new("-", ["r18g"], Metadata: true, Description: "排除 R18G")
        ];
    }

    private sealed class GifSyntax : FilterFlagSyntax
    {
        public override string Key => "Gif";

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["gif"], Metadata: false, Description: "仅显示动图"),
            new("-", ["gif"], Metadata: true, Description: "排除动图")
        ];
    }

    private static void AssertValueHintsOnly(string text, IReadOnlyCollection<string> displayTexts)
    {
        var result = _Language.Analyze(text, text.Length);

        CollectionAssert.AreEquivalent(displayTexts.ToArray(), result.Completions.Select(t => t.DisplayText).ToArray());
        Assert.IsTrue(result.Completions.All(t => t.InsertText == text));
        Assert.IsTrue(result.Completions.All(t => t.IsHintOnly));
    }
}
