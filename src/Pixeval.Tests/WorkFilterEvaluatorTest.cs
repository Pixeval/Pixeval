using System;
using System.Collections.Generic;
using Mako.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Filters;
using Pixeval.Filters.Nodes;
using Pixeval.Filters.Syntax;
using Pixeval.Models.Filters;

namespace Pixeval.Tests;

[TestClass]
public sealed class WorkFilterEvaluatorTest
{
    private static readonly FilterLanguage _Language = new([
        new TitleSyntax(),
        new AuthorSyntax(),
        new TagSyntax(),
        new BookmarkSyntax(),
        new RatioSyntax(),
        new StartDateSyntax(),
        new EndDateSyntax(),
        new R18Syntax(),
        new R18GSyntax(),
        new AiSyntax(),
        new GifSyntax()
    ]);

    [TestMethod]
    public void EvaluatorShouldSupportAllWorkFilterPredicates()
    {
        var work = CreateIllustration(
            title: "Blue Hour",
            author: "Alice",
            tags:
            [
                new()
                {
                    Name = "sky",
                    TranslatedName = "空"
                }
            ],
            totalBookmarks: 180,
            createDate: new(2024, 2, 3, 0, 0, 0, TimeSpan.Zero),
            width: 1200,
            height: 600,
            xRestrict: XRestrict.R18,
            aiType: AiType.AiGenerated,
            illustrationType: IllustrationType.Ugoira);

        AssertMatches(work, "Blue");
        AssertMatches(work, "@Alice");
        AssertMatches(work, "#空");
        AssertMatches(work, "like:100-200");
        AssertMatches(work, "ratio:1-3");
        AssertMatches(work, "start:2024-1-1");
        AssertMatches(work, "end:2024-12-31");
        AssertMatches(work, "+r18");
        AssertMatches(work, "+ai");
        AssertMatches(work, "+gif");
        AssertMatches(work with { XRestrict = XRestrict.R18G }, "+r18g");
    }

    [TestMethod]
    public void EvaluatorShouldRejectNonMatchingPredicates()
    {
        var work = CreateIllustration(
            title: "Blue Hour",
            author: "Alice",
            tags:
            [
                new()
                {
                    Name = "sky",
                    TranslatedName = null
                }
            ],
            totalBookmarks: 180,
            createDate: new(2024, 2, 3, 0, 0, 0, TimeSpan.Zero),
            width: 1200,
            height: 600,
            xRestrict: XRestrict.Ordinary,
            aiType: AiType.NotAiGenerated,
            illustrationType: IllustrationType.Illust);

        AssertDoesNotMatch(work, "Red");
        AssertDoesNotMatch(work, "@Bob");
        AssertDoesNotMatch(work, "#sea");
        AssertDoesNotMatch(work, "like:200-300");
        AssertDoesNotMatch(work, "ratio:3-4");
        AssertDoesNotMatch(work, "+r18");
        AssertDoesNotMatch(work, "+ai");
        AssertDoesNotMatch(work, "+gif");
    }

    private static void AssertMatches(Illustration work, string text) =>
        Assert.IsTrue(WorkFilterEvaluator.Filter(work, Parse(text)), text);

    private static void AssertDoesNotMatch(Illustration work, string text) =>
        Assert.IsFalse(WorkFilterEvaluator.Filter(work, Parse(text)), text);

    private static FilterNode Parse(string text)
    {
        var analysis = _Language.Analyze(text);
        Assert.IsTrue(analysis.IsSuccess, text);
        return analysis.Query!.Root;
    }

    private static Illustration CreateIllustration(
        string title,
        string author,
        IReadOnlyList<Tag> tags,
        int totalBookmarks,
        DateTimeOffset createDate,
        int width,
        int height,
        XRestrict xRestrict,
        AiType aiType,
        IllustrationType illustrationType) =>
        new()
        {
            Id = 100,
            Title = title,
            Description = "",
            IsPrivate = false,
            XRestrict = xRestrict,
            Tags = tags,
            User = new()
            {
                Id = 200,
                Name = author,
                Account = author.ToLowerInvariant(),
                ProfileImageUrls = new() { Medium = "https://example.test/avatar.png" }
            },
            CreateDate = createDate,
            ThumbnailUrls = new()
            {
                SquareMedium = "https://example.test/square.jpg",
                Medium = "https://example.test/medium.jpg",
                Large = "https://example.test/large.jpg"
            },
            IsFavorite = false,
            TotalFavorite = totalBookmarks,
            TotalView = 1000,
            Visible = true,
            IsMuted = false,
            Series = null,
            Type = illustrationType,
            Tools = [],
            PageCount = 1,
            Width = width,
            Height = height,
            SanityLevel = xRestrict is XRestrict.Ordinary ? 2 : 6,
            MetaSinglePage = new() { OriginalImageUrl = "https://example.test/original.jpg" },
            MetaPages = [],
            AiType = aiType,
            IllustrationBookStyle = 0
        };

    private sealed class TitleSyntax : FilterTextSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Title;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.Default("keyword"),
            FilterSyntaxPattern.Keyword("title", exampleValue: "keyword")
        ];
    }

    private sealed class AuthorSyntax : FilterTextSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Author;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.PrefixOnly("@", "artist"),
            FilterSyntaxPattern.Keyword("artist", exampleValue: "artist")
        ];
    }

    private sealed class TagSyntax : FilterTextSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Tag;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.PrefixOnly("#", "tag"),
            FilterSyntaxPattern.Keyword("tag", exampleValue: "tag")
        ];
    }

    private sealed class BookmarkSyntax : FilterLongRangeSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Bookmark;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.Keyword("like", exampleValue: "100-200")
        ];
    }

    private sealed class RatioSyntax : FilterDoubleRangeSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Ratio;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.Keyword("ratio", exampleValue: "1-2")
        ];
    }

    private sealed class StartDateSyntax : FilterDateSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.StartDate;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.Keyword("start", exampleValue: "2024-1-1")
        ];
    }

    private sealed class EndDateSyntax : FilterDateSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.EndDate;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            FilterSyntaxPattern.Keyword("end", exampleValue: "2024-1-1")
        ];
    }

    private sealed class R18Syntax : FilterFlagSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.R18;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["r18"], Metadata: false),
            new("-", ["r18"], Metadata: true)
        ];
    }

    private sealed class R18GSyntax : FilterFlagSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.R18G;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["r18g"], Metadata: false),
            new("-", ["r18g"], Metadata: true)
        ];
    }

    private sealed class AiSyntax : FilterFlagSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Ai;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["ai"], Metadata: false),
            new("-", ["ai"], Metadata: true)
        ];
    }

    private sealed class GifSyntax : FilterFlagSyntax
    {
        public override string Key => WorkFilterSyntaxKeys.Gif;

        public override IReadOnlyList<FilterSyntaxPattern> Patterns { get; } =
        [
            new("+", ["gif"], Metadata: false),
            new("-", ["gif"], Metadata: true)
        ];
    }
}
