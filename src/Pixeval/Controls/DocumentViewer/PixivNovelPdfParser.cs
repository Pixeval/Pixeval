#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/PixivNovelPdfParser.cs
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
using Pixeval.Util;
using Pixeval.Utilities;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pixeval.Controls;


public sealed class PixivNovelPdfParser(ColumnDescriptor descriptor, int pageIndex) : PixivNovelParser<ColumnDescriptor, Stream, INovelParserViewModel<Stream>>
{
    public static void Init() { }

    static PixivNovelPdfParser() => Settings.License = LicenseType.Community;

    protected override ColumnDescriptor Vector
    {
        get
        {
            _ = descriptor.Item().Section(pageIndex.ToString());
            return descriptor;
        }
    }

    private Action<TextDescriptor>? _lastDelegate;

    public void AddAction(Action<TextDescriptor> action)
    {
        _lastDelegate += action;
    }

    public void LineBreak(ColumnDescriptor currentColumn)
    {
        if (_lastDelegate is null)
            return;
        currentColumn.Item().Text(_lastDelegate);
        _lastDelegate = null;
    }

    public void AddText(string text)
    {
        AddAction(t => t.Span(text));
    }

    protected override void AddLastSpan(ColumnDescriptor currentColumn, string lastSpan)
    {
        AddText(lastSpan);
    }

    protected override void AddRuby(ColumnDescriptor currentColumn, string kanji, string ruby)
    {
        AddText(kanji);
        AddAction(t => t.Span($"（{ruby}）").FontColor(Colors.Grey.Medium));
    }

    protected override void AddHyperlink(ColumnDescriptor currentColumn, string content, Uri uri)
    {
        AddAction(t => t.Hyperlink(content, uri.OriginalString).FontColor(Colors.Blue.Medium));
    }

    protected override void AddInlineHyperlink(ColumnDescriptor currentColumn, uint page, INovelParserViewModel<Stream> viewModel)
    {
        AddAction(t => t.SectionLink(MiscResources.GoToPageFormatted.Format(page), (page - 1).ToString()).FontColor(Colors.Blue.Medium));
    }

    protected override void AddChapter(ColumnDescriptor currentColumn, string chapterText)
    {
        LineBreak(currentColumn);
        _ = currentColumn.Item();
        _ = currentColumn.Item().Text(chapterText).FontSize(20).Bold();
        _ = currentColumn.Item();
    }

    protected override void AddUploadedImage(ColumnDescriptor currentColumn, INovelParserViewModel<Stream> viewModel, long imageId)
    {
        LineBreak(currentColumn);
        _ = currentColumn.Item().Image(viewModel.UploadedImages[imageId]).FitWidth();
    }

    protected override void AddPixivImage(ColumnDescriptor currentColumn, INovelParserViewModel<Stream> viewModel, long imageId, int page)
    {
        LineBreak(currentColumn);
        var key = (imageId, page);
        _ = currentColumn.Item().Hyperlink(MakoHelper.GenerateIllustrationWebUri(imageId).OriginalString).Image(viewModel.IllustrationImages[key]);
        // var info = viewModel.IllustrationLookup[imageId];
        // Image(viewModel.IllustrationImages[key]);
    }

    protected override void Finish(ColumnDescriptor currentColumn) => LineBreak(currentColumn);

    protected override void NewPage(ColumnDescriptor currentColumn)
    {
        LineBreak(currentColumn);
        currentColumn.Item().PageBreak();
    }
}
