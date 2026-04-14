// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using System.Text;
using Avalonia.Media.Imaging;
using Pixeval.I18N;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public class PixivNovelMdParser<T>(StringBuilder sb, int pageIndex) : PixivNovelParser<StringBuilder, T, INovelContext<T>> where T : class
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

    protected override void AddInlineHyperlink(StringBuilder currentText, uint page, INovelContext<T> viewModel)
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

    protected override void AddUploadedImage(StringBuilder currentText, INovelContext<T> viewModel, long imageId)
    {
        _ = currentText
            .AppendLine()
            .Append($"![{imageId}]({imageId}{viewModel.ImageExtension})")
            .AppendLine();
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelContext<T> viewModel, long imageId, int page)
    {
        // var key = (imageId, page);
        _ = currentText
            .AppendLine()
            .Append($"[![{imageId}-{page}]({imageId}-{page}{viewModel.ImageExtension})]({MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString})")
            .AppendLine();
        // var info = viewModel.IllustrationLookup[imageId];
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
        _ = currentText
            .AppendLine()
            .Append($"![{imageId}]({viewModel.NovelContent.Images.FirstOrDefault(t => t.NovelImageId == imageId)?.ThumbnailUrl})")
            .AppendLine();
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelContext<Bitmap> viewModel, long imageId, int page)
    {
        // var key = (imageId, page);
        _ = currentText
            .AppendLine()
            .Append(
                $"[![{imageId}-{page}]({viewModel.IllustrationLookup[(imageId, page)].ThumbnailUrl})]({MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString})")
            .AppendLine();
        // var info = viewModel.IllustrationLookup[imageId];
    }
}
