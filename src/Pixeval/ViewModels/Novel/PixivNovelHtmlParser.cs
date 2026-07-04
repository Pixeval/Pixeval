// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Net;
using System.Text;
using Pixeval.I18N;

namespace Pixeval.ViewModels;

public class PixivNovelHtmlParser<TImage>(StringBuilder sb, int pageIndex) : PixivNovelParser<StringBuilder, TImage, INovelContext<TImage>> where TImage : class
{
    protected override StringBuilder Vector => sb.AppendLine($"<div id=\"page{pageIndex}\" /><br/>");

    protected override void AddLastSpan(StringBuilder currentText, string lastSpan)
    {
        _ = currentText.Append($"<span>{lastSpan.ReplaceLineEndings(Environment.NewLine + "<br/>" + Environment.NewLine)}</span>");
    }

    protected override void AddRuby(StringBuilder currentText, string kanji, string ruby)
    {
        _ = currentText.Append($"<ruby>{WebUtility.HtmlEncode(kanji)}<rp>（</rp><rt>{WebUtility.HtmlEncode(ruby)}</rt><rp>）</rp></ruby>");
    }

    protected override void AddHyperlink(StringBuilder currentText, string content, Uri uri)
    {
        _ = currentText.Append($"<a href=\"{uri.OriginalString}\">{content}</a>");
    }

    protected override void AddInlineHyperlink(StringBuilder currentText, uint page)
    {
        _ = currentText.Append($"<a href=\"page{page - 1}\">{I18NManager.GetResource(MiscResources.GoToPageFormatted, page)}</a>");
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

    protected override void AddUploadedImage(StringBuilder currentText, INovelContext<TImage> viewModel, long imageId)
    {
        var url = viewModel.ImageLookup[imageId].ThumbnailUrl;
        _ = currentText
        .AppendLine("<br/><br/>")
            .AppendLine($"<img src=\"{imageId}{Path.GetExtension(url)}\" alt=\"{imageId}\" />")
            .AppendLine("<br/><br/>");
    }

    protected override void AddPixivImage(StringBuilder currentText, INovelContext<TImage> viewModel, long imageId, int page)
    {
        var key = (imageId, page);
        var info = viewModel.IllustrationLookup[key];
        _ = currentText
            .AppendLine("<br/><br/>")
            .AppendLine($"<a href=\"{info.WebsiteUri.OriginalString}\"><img src=\"{imageId}-{page}{Path.GetExtension(info.ThumbnailUrl)}\" alt=\"{imageId}-{page}\" /></a>")
            .AppendLine("<br/><br/>");
    }

    protected override void NewPage(StringBuilder currentText)
    {
        _ = currentText.AppendLine().AppendLine("<hr/>");
    }
}
