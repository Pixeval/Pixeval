using System;
using System.Collections.Generic;
using Mako.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Filters;
using Pixeval.Filters.Nodes;
using Pixeval.Models.Filters;

namespace Pixeval.Tests;

[TestClass]
public sealed class WorkFilterNodeTest
{
    private static readonly FilterLanguage _Language = WorkFilterLanguage.Instance;

    [TestMethod]
    public void WorkFilterNodesShouldSupportAllPredicatesAndGroups()
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
        AssertMatches(work, "(and Blue @Alice)");
        AssertMatches(work, "(or Red @Alice)");
        AssertMatches(work, "!Red");
        AssertDoesNotMatch(work, "(and Blue @Bob)");
        AssertDoesNotMatch(work, "!(or Blue @Alice)");
    }

    [TestMethod]
    public void WorkFilterNodesShouldRejectNonMatchingPredicates()
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
            illustrationType: IllustrationType.Illustration);

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
        Assert.IsTrue(Parse(text).Match(work), text);

    private static void AssertDoesNotMatch(Illustration work, string text) =>
        Assert.IsFalse(Parse(text).Match(work), text);

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
}
