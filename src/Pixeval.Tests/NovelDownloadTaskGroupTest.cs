using System;
using System.IO;
using Mako;
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
        context.SetStream(2, new MemoryStream());

        Assert.AreSequenceEqual((string[]) ["cover.png", "101.png", "202-2.webp"], context.AllFileNames);

        var html = context.LoadHtmlContent().ToString();
        Assert.Contains("src=\"101.png\"", html);
        Assert.Contains("src=\"202-2.webp\"", html);

        var markdown = context.LoadMdContent().ToString();
        Assert.Contains("![101](101.png)", markdown);
        Assert.Contains("![202-2](202-2.webp)", markdown);
    }

    [TestMethod]
    public void BuiltInDocumentsShouldReferenceDownloadedCover()
    {
        var content = CreateNovelContent();
        content.CoverUrl = "https://i.pximg.net/img-original/novel/1.jpg?token=cover";
        using var context = new NovelContext(content);
        ((INovelContext<Stream>) context).InitImages();
        var coverStream = new MemoryStream();
        var uploadedImageStream = new MemoryStream();
        var illustrationStream = new MemoryStream();
        context.SetStream(0, coverStream);
        context.SetStream(1, uploadedImageStream);
        context.SetStream(2, illustrationStream);

        Assert.AreEqual("cover.jpg", context.CoverFileName);
        Assert.AreEqual(3, context.TotalImagesCount);
        Assert.AreSequenceEqual((string[]) ["cover.jpg", "101.png", "202-2.webp"], context.AllFileNames);
        Assert.AreSame(coverStream, context.TryGetStream(0));
        Assert.AreSame(uploadedImageStream, context.TryGetStream(1));
        Assert.AreSame(illustrationStream, context.TryGetStream(2));

        var html = context.LoadHtmlContent().ToString();
        Assert.Contains("<img src=\"cover.jpg\" alt=\"cover\" />", html);
        Assert.IsLessThan(html.IndexOf("101.png", StringComparison.Ordinal), html.IndexOf("cover.jpg", StringComparison.Ordinal));

        var markdown = context.LoadMdContent().ToString();
        Assert.Contains("![cover](cover.jpg)", markdown);
        Assert.IsLessThan(markdown.IndexOf("101.png", StringComparison.Ordinal), markdown.IndexOf("cover.jpg", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("not a uri")]
    [DataRow("file:///cover.jpg")]
    public void InvalidCoverUrlShouldUseDefaultImage(string coverUrl)
    {
        var content = CreateNovelContent();
        content.CoverUrl = coverUrl;
        using var context = new NovelContext(content);

        Assert.AreEqual(DefaultImageUrls.ImageNotAvailable, context.CoverUri.OriginalString);
        Assert.AreEqual("cover.png", context.CoverFileName);
        Assert.AreEqual(DefaultImageUrls.ImageNotAvailable, context.AllUrls[0]);
        Assert.AreEqual("cover.png", context.AllFileNames[0]);
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
