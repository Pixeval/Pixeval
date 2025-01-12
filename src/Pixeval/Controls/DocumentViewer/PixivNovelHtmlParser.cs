// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Text;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public class PixivNovelHtmlParser(StringBuilder sb, int pageIndex) : PixivNovelParser<StringBuilder, Stream, INovelParserViewModel<Stream>>
{
    protected override StringBuilder Vector => sb.AppendLine($"<div id=\"page{pageIndex}\" /><br/>");

    protected override void AddLastSpan(StringBuilder currentText, string lastSpan)
    {
        _ = currentText.Append($"<span>{lastSpan.ReplaceLineEndings("\n<br/>\n")}</span>");
    }

    protected override void AddRuby(StringBuilder currentText, string kanji, string ruby)
    {
        _ = currentText.Append($"<ruby>{kanji}<rp>（<rp><rt>{ruby}</rt><rp>）<rp></ruby>");
    }

    protected override void AddHyperlink(StringBuilder currentText, string content, Uri uri)
    {
        _ = currentText.Append($"<a href=\"{uri.OriginalString}\">{content}</a>");
    }

    protected override void AddInlineHyperlink(StringBuilder currentText, uint page, INovelParserViewModel<Stream> viewModel)
    {
        _ = currentText.Append($"<a href=\"page{page - 1}\">{MiscResources.GoToPageFormatted.Format(page)}</a>");
    }

    protected override void AddChapter(StringBuilder currentText, string chapterText)
    {
        // last span
        // ## {chapterText}
        // next span
        _ = currentText
            .AppendLine("<br/><br/>")
            .AppendLine($"<h2>{chapterText}</h2>")
            .AppendLine("<br/><br/>");
    }

    protected override void AddUploadedImage(StringBuilder currentText, INovelParserViewModel<Stream> viewModel, long imageId)
    {
        _ = currentText
        .AppendLine("<br/><br/>")
            .AppendLine($"<img src=\"{imageId}{viewModel.ImageExtension}\" alt=\"{imageId}\" />")
            .AppendLine("<br/><br/>");
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelParserViewModel<Stream> viewModel, long imageId, int page)
    {
        // var key = (imageId, page);
        _ = currentText
            .AppendLine("<br/><br/>")
            .AppendLine($"<a href=\"{MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString}\"><img src=\"{imageId}-{page}{viewModel.ImageExtension}\" alt=\"{imageId}-{page}\" /></a>")
            .AppendLine("<br/><br/>");
        // var info = viewModel.IllustrationLookup[imageId];
    }

    protected override void NewPage(StringBuilder currentText)
    {
        _ = currentText.AppendLine().AppendLine("<hr/>");
    }
}
