// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using Pixeval.I18N;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public class PixivNovelMdParser<TImage>(StringBuilder sb, int pageIndex) : PixivNovelParser<StringBuilder, TImage, INovelContext<TImage>> where TImage : class
{
    protected override StringBuilder Vector => sb.AppendLine($"<div id=\"page{pageIndex}\" />" + Environment.NewLine);

    protected override void AddLastSpan(StringBuilder currentText, string lastSpan)
    {
        _ = currentText.Append(lastSpan.ReplaceLineEndings(Environment.NewLine + Environment.NewLine));
    }

    protected override void AddRuby(StringBuilder currentText, string kanji, string ruby)
    {
        _ = currentText.Append($"{kanji}（{ruby}）");
    }

    protected override void AddHyperlink(StringBuilder currentText, string content, Uri uri)
    {
        _ = currentText.Append($"[{content}]({uri.OriginalString})");
    }

    protected override void AddInlineHyperlink(StringBuilder currentText, uint page)
    {
        _ = currentText.Append($"[{I18NManager.GetResource(MiscResources.GoToPageFormatted, page)}](page{page - 1})");
    }

    protected override void AddChapter(StringBuilder currentText, string chapterText)
    {
        // last span
        // ## {chapterText}
        // next span
        _ = currentText
            .AppendLine()
            .Append($"## {chapterText}")
            .AppendLine();
    }

    protected override void AddUploadedImage(StringBuilder currentText, INovelContext<TImage> viewModel, long imageId)
    {
        var url = viewModel.ImageLookup[imageId].ThumbnailUrl;
        _ = currentText
            .AppendLine()
            .Append($"![{imageId}]({imageId}{Path.GetExtension(url)})")
            .AppendLine();
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelContext<TImage> viewModel, long imageId, int page)
    {
        var key = (imageId, page);
        var url = viewModel.IllustrationLookup[key].ThumbnailUrl;
        _ = currentText
            .AppendLine()
            .Append($"[![{imageId}-{page}]({imageId}-{page}{Path.GetExtension(url)})]({MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString})")
            .AppendLine();
        // var info = viewModel.IllustrationLookup[key];
    }

    protected override void NewPage(StringBuilder currentText)
    {
        _ = currentText.AppendLine().AppendLine("---");
    }
}

public class PixivNovelMdDisplayParser(StringBuilder sb, int pageIndex) : PixivNovelMdParser<Bitmap>(sb, pageIndex)
{
    protected override void NewPage(StringBuilder currentText)
    {
    }

    protected override void AddRuby(StringBuilder currentText, string kanji, string ruby)
    {
        // TODO?
        base.AddRuby(currentText, kanji, ruby);
    }

    protected override void AddUploadedImage(StringBuilder currentText, INovelContext<Bitmap> viewModel, long imageId)
    {
        var url = viewModel.ImageLookup[imageId].ThumbnailUrl;
        _ = currentText
            .AppendLine()
            .Append($"![{imageId}]({url})")
            .AppendLine();
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelContext<Bitmap> viewModel, long imageId, int page)
    {
        var key = (imageId, page);
        var url = viewModel.IllustrationLookup[key].ThumbnailUrl;
        _ = currentText
            .AppendLine()
            .Append(
                $"[![{imageId}-{page}]({url})]({MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString})")
            .AppendLine();
        // var info = viewModel.IllustrationLookup[key];
    }
}
