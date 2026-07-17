using System;
using System.IO;
using Mako.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Models.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Models.Options;
using Pixeval.Utilities.IO;
using Pixeval.ViewModels;

namespace Pixeval.Tests;

[TestClass]
public sealed class NovelDownloadTaskGroupTest
{
    [TestMethod]
    [DataRow(NovelDownloadFormat.OriginalTxt, "txt")]
    [DataRow(NovelDownloadFormat.Html, "html")]
    [DataRow(NovelDownloadFormat.Md, "md")]
    public void BuiltInFormatShouldUseMacroFileNameAsFolder(NovelDownloadFormat format, string extension)
    {
        var tokenizedDestination = Path.Combine("downloads", "work.<ext>");

        var paths = NovelDownloadTaskGroup.GetOutputPaths(
            tokenizedDestination,
            NovelDownloadFormatToken.BuiltIn(format));

        var expectedFolder = Path.Combine("downloads", "work");
        Assert.AreEqual(Path.Combine(expectedFolder, $"novel.{extension}"), paths.NovelFile);
        Assert.AreEqual(expectedFolder, paths.ImageFolderPath);
    }

    [TestMethod]
    public void ExtensionFormatShouldUseMacroFileNameAndTemporaryImageFolder()
    {
        var tokenizedDestination = Path.Combine("downloads", "work.<ext>");

        var paths = NovelDownloadTaskGroup.GetOutputPaths(
            tokenizedDestination,
            new(NovelDownloadFormatToken.ExtensionPrefix + "pdf"));

        var expectedFile = Path.Combine("downloads", "work.pdf");
        Assert.AreEqual(expectedFile, paths.NovelFile);
        Assert.AreEqual(expectedFile + IoHelper.PixevalTempExtension, paths.ImageFolderPath);
    }

    [TestMethod]
    public void BuiltInDocumentsShouldReferenceDownloadedOriginalImageNames()
    {
        using var context = new NovelContext(CreateNovelContent());
        ((INovelContext<Stream>) context).InitImages();
        context.SetStream(0, new MemoryStream());
        context.SetStream(1, new MemoryStream());

        CollectionAssert.AreEqual((string[]) ["101.png", "202-2.webp"], context.AllFileNames);

        var html = context.LoadHtmlContent().ToString();
        StringAssert.Contains(html, "src=\"101.png\"");
        StringAssert.Contains(html, "src=\"202-2.webp\"");

        var markdown = context.LoadMdContent().ToString();
        StringAssert.Contains(markdown, "![101](101.png)");
        StringAssert.Contains(markdown, "![202-2](202-2.webp)");
    }

    private static NovelContent CreateNovelContent() => new()
    {
        Id = 1,
        Title = "Novel",
        SeriesId = null,
        SeriesTitle = null,
        SeriesIsWatched = null,
        UserId = 1,
        CoverUrl = "",
        Tags = [],
        Caption = "",
        Date = DateTimeOffset.UnixEpoch,
        Rating = new()
        {
            Like = 0,
            Bookmark = 0,
            View = 0
        },
        Text = "[uploadedimage:101]\n[pixivimage:202-2]",
        Marker = null,
        Illustrations =
        [
            new()
            {
                Visible = true,
                AvailableMessage = null,
                Illustration = new()
                {
                    Title = "",
                    Description = "",
                    Restrict = 0,
                    XRestrict = 0,
                    Sl = 0,
                    Tags = [],
                    Images = new()
                    {
                        Small = null,
                        Medium = "https://i.pximg.net/c/600x1200/novel/202_p1.webp?token=thumbnail",
                        Original = null
                    }
                },
                User = new()
                {
                    Id = 1,
                    Name = "",
                    Image = ""
                },
                Id = 202,
                Page = 2
            }
        ],
        Images =
        [
            new()
            {
                NovelImageId = 101,
                Sl = 0,
                Urls = new()
                {
                    Mw240 = "",
                    Mw480 = "",
                    X1200 = "https://i.pximg.net/c/1200x1200/novel/101.jpg?token=thumbnail",
                    X128 = "",
                    Original = "https://i.pximg.net/img-original/novel/101.png?token=original"
                }
            }
        ],
        SeriesNavigation = null,
        GlossaryItems = [],
        ReplaceableItemIds = [],
        AiType = default,
        IsOriginal = true,
        SeasonalEffectTagData = null,
        EventBanners = null,
        Language = ""
    };
}
