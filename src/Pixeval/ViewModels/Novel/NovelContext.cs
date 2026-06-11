// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Mako.Model;
using Pixeval.AppManagement;

namespace Pixeval.ViewModels;

public class NovelContext(NovelContent novelContent) : INovelContext<Stream>, IDisposable
{
    public int TotalImagesCount { get; } = novelContent.Images.Count + novelContent.Illustrations.Count;

    /// <summary>
    /// 所有图片的URL
    /// </summary>
    /// <returns>小说图片是内嵌的，没必要用原图</returns>
    public string[] AllUrls { get; } = [.. novelContent.Images.Select(x => x.OriginalUrl), .. novelContent.Illustrations.Select(x => x.ThumbnailUrl)];

    public string[] AllTokens { get; } = [.. novelContent.Images.Select(x => x.NovelImageId.ToString()), .. novelContent.Illustrations.Select(x => $"{x.Id}-{x.Page}")];

    public StringBuilder LoadMdContent()
    {
        var index = 0;
        var length = NovelContent.Text.Length;

        var sb = new StringBuilder();
        for (var i = 0; index < length; ++i)
        {
            var parser = new PixivNovelMdParser<Stream>(sb, i);
            _ = parser.Parse(NovelContent.Text, ref index, this);
            if (LoadingCts.IsCancellationRequested)
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
            if (LoadingCts.IsCancellationRequested)
                break;
        }

        return sb;
    }

    public Stream? TryGetStream(int index)
    {
        if (index < NovelContent.Images.Count)
            return UploadedImages.GetValueOrDefault(NovelContent.Images[index].NovelImageId);
        var illustration = NovelContent.Illustrations[index - NovelContent.Images.Count];
        return IllustrationImages.GetValueOrDefault((illustration.Id, illustration.Page));
    }

    public void SetStream(int index, Stream? stream)
    {
        if (index < NovelContent.Images.Count)
        {
            UploadedImages[NovelContent.Images[index].NovelImageId] = stream ?? AppInfo.GetImageNotAvailableStream();
        }
        else
        {
            var illustration = NovelContent.Illustrations[index - NovelContent.Images.Count];
            IllustrationImages[(illustration.Id, illustration.Page)] = stream ?? AppInfo.GetImageNotAvailableStream();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        LoadingCts.Cancel();
        foreach (var (_, value) in IllustrationImages)
            value.Dispose();
        IllustrationImages.Clear();
        foreach (var (_, value) in UploadedImages)
            value.Dispose();
        UploadedImages.Clear();
        IllustrationLookup.Clear();
        ImageLookup.Clear();
    }

    public NovelContent NovelContent { get; } = novelContent;

    public Dictionary<(long, int), NovelIllustration> IllustrationLookup { get; } = [];

    public Dictionary<(long, int), Stream> IllustrationImages { get; } = [];

    public Dictionary<long, NovelImage> ImageLookup { get; } = [];

    public Dictionary<long, Stream> UploadedImages { get; } = [];

    public CancellationTokenSource LoadingCts { get; } = new();
}
