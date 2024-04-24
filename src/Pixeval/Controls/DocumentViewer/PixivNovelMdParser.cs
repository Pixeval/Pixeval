#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/PixivNovelMdParser.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Text;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public sealed class PixivNovelMdParser(StringBuilder sb, int pageIndex) : PixivNovelParser<StringBuilder, Stream, FileParserViewModel>
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

    protected override void AddInlineHyperlink(StringBuilder currentText, uint page, FileParserViewModel viewModel)
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

    protected override void AddUploadedImage(StringBuilder currentText, FileParserViewModel viewModel, long imageId)
    {
        _ = currentText
            .AppendLine()
            .Append($"![{imageId}]({imageId}.)")
            .AppendLine();
    }

    protected override void AddPixivImage(StringBuilder currentText, FileParserViewModel viewModel, long imageId, int page)
    {
        var key = (imageId, page);
        _ = currentText
            .AppendLine()
            .Append($"[![{imageId}]({imageId}-{page}.)]({MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString})")
            .AppendLine();
        // var info = viewModel.IllustrationLookup[imageId];
    }

    protected override void NewPage(StringBuilder currentText)
    {
        _ = currentText.AppendLine().AppendLine("---");
    }
}
