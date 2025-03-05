// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Pixeval.Controls;

public partial class NovelContext(NovelContent novelContent) : INovelContext<Stream>, IDisposable
{
    public string? ImageExtension { get; set; }

    public int TotalImagesCount { get; } = novelContent.Images.Length + novelContent.Illusts.Length;

    /// <summary>
    /// 所有图片的URL
    /// </summary>
    /// <returns>小说图片是内嵌的，没必要用原图</returns>
    public string[] AllUrls { get; } = novelContent.Images.Select(x => x.OriginalUrl).Concat(novelContent.Illusts.Select(x => x.ThumbnailUrl)).ToArray();

    public string[] AllTokens { get; } = novelContent.Images.Select(x => x.NovelImageId.ToString()).Concat(novelContent.Illusts.Select(x => $"{x.Id}-{x.Page}")).ToArray();

    public StringBuilder LoadMdContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        var sb = new StringBuilder();
        for (var i = 0; index < length; ++i)
        {
            var parser = new PixivNovelMdParser<Stream>(sb, i);
            _ = parser.Parse(NovelContent.Text, ref index, this);
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
        }

        return sb;
    }

    public StringBuilder LoadHtmlContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        var sb = new StringBuilder();
        for (var i = 0; index < length; ++i)
        {
            var parser = new PixivNovelHtmlParser<Stream>(sb, i);
            _ = parser.Parse(NovelContent.Text, ref index, this);
            if (LoadingCancellationTokenSource.IsCancellationRequested)
                break;
        }

        return sb;
    }

    public Document LoadPdfContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        PixivNovelPdfParser.Init();

        return
            Document.Create(t =>
                t.Page(p =>
                {
                    p.MarginHorizontal(90);
                    p.MarginVertical(72);
                    p.DefaultTextStyle(new TextStyle().LineHeight(2));
                    p.Content().Column(c =>
                    {
                        for (var i = 0; index < length; ++i)
                        {
                            var parser = new PixivNovelPdfParser(c, i);
                            _ = parser.Parse(NovelContent.Text, ref index, this);
                            if (LoadingCancellationTokenSource.IsCancellationRequested)
                                break;
                        }
                    });
                }));
    }

    public (long Id, IEnumerable<string> Tags)? GetIdTags(int index)
    {
        if (index < NovelContent.Images.Length)
            return null;
        var illust = NovelContent.Illusts[index - NovelContent.Images.Length];
        return (illust.Id, illust.Illust.Tags.Select(t => t.Tag));
    }

    public Stream? TryGetStream(int index)
    {
        if (index < NovelContent.Images.Length)
            return UploadedImages.GetValueOrDefault(NovelContent.Images[index].NovelImageId);
        var illust = NovelContent.Illusts[index - NovelContent.Images.Length];
        return IllustrationImages.GetValueOrDefault((illust.Id, illust.Page));
    }

    public void SetStream(int index, Stream? stream)
    {
        if (index < NovelContent.Images.Length)
        {
            UploadedImages[NovelContent.Images[index].NovelImageId] = stream ?? AppInfo.GetImageNotAvailableStream();
        }
        else
        {
            var illust = NovelContent.Illusts[index - NovelContent.Images.Length];
            IllustrationImages[(illust.Id, illust.Page)] = stream ?? AppInfo.GetImageNotAvailableStream();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        LoadingCancellationTokenSource.TryCancelDispose();
        foreach (var (_, value) in IllustrationImages)
            value.Dispose();
        IllustrationImages.Clear();
        foreach (var (_, value) in UploadedImages)
            value.Dispose();
        UploadedImages.Clear();
        IllustrationLookup.Clear();
    }

    public NovelContent NovelContent { get; } = novelContent;

    public Dictionary<(long, int), NovelIllustInfo> IllustrationLookup { get; } = [];

    public Dictionary<(long, int), Stream> IllustrationImages { get; } = [];

    public Dictionary<long, Stream> UploadedImages { get; } = [];

    public CancellationTokenSource LoadingCancellationTokenSource { get; } = new();
}
