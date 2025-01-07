// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Text;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public sealed class PixivNovelMdParser(StringBuilder sb, int pageIndex) : PixivNovelParser<StringBuilder, Stream, INovelParserViewModel<Stream>>
{
    protected override StringBuilder Vector => sb.AppendLine($"<div id=\"page{pageIndex}\" /><br/>");

    protected override void AddLastSpan(StringBuilder currentText, string lastSpan)
    {
        _ = currentText.Append(lastSpan);
    }

    protected override void AddRuby(StringBuilder currentText, string kanji, string ruby)
    {
        _ = currentText.Append($"{kanji}（{ruby}）");
    }

    protected override void AddHyperlink(StringBuilder currentText, string content, Uri uri)
    {
        _ = currentText.Append($"[{content}]({uri.OriginalString})");
    }

    protected override void AddInlineHyperlink(StringBuilder currentText, uint page, INovelParserViewModel<Stream> viewModel)
    {
        _ = currentText.Append($"[{MiscResources.GoToPageFormatted.Format(page)}](page{page - 1})");
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

    protected override void AddUploadedImage(StringBuilder currentText, INovelParserViewModel<Stream> viewModel, long imageId)
    {
        _ = currentText
            .AppendLine()
            .Append($"![{imageId}]({imageId}{viewModel.ImageExtension})")
            .AppendLine();
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelParserViewModel<Stream> viewModel, long imageId, int page)
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
