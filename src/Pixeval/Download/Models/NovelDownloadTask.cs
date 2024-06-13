#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelDownloadTask.cs
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Download.Macros;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using QuestPDF.Fluent;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class NovelDownloadTask : DownloadTaskBase
{
    public NovelDownloadTask(
        DownloadHistoryEntry entry,
        NovelItemViewModel novel,
        NovelContent novelContent)
        : this(entry, novel, novelContent, new DocumentViewerViewModel(novelContent))
    {
    }

    protected NovelDownloadTask(
        DownloadHistoryEntry entry,
        NovelItemViewModel novel,
        NovelContent novelContent,
        DocumentViewerViewModel viewModel) : base(entry)
    {
        NovelItemViewModel = novel;
        NovelContent = novelContent;
        DocumentViewModel = viewModel;
    }

    public override IWorkViewModel ViewModel => NovelItemViewModel;

    public NovelItemViewModel NovelItemViewModel { get; protected set; }

    public NovelContent NovelContent { get; protected set; }

    public DocumentViewerViewModel DocumentViewModel { get; protected set; }

    public override IReadOnlyList<string> ActualDestinations
    {
        get
        {
            if (!IsFolder)
                return [Destination.RemoveTokens()];

            var destinations = new List<string>();
            var directory = Path.GetDirectoryName(Destination)!;
            if (Destination.EndsWith(FileExtensionMacro.NameConstToken))
            {
                var docPath = Destination[..^FileExtensionMacro.NameConstToken.Length];
                destinations.Add(docPath);
                for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
                {
                    var url = DocumentViewModel.AllUrls[i];
                    var token = DocumentViewModel.AllTokens[i];
                    destinations.Add(IoHelper.ReplaceTokenExtensionFromUrl(Path.Combine(directory, token), url).RemoveTokens());
                }
            }
            else
            {
                var ext = Destination.LastIndexOf('.');
                var docPath = Destination[..ext];
                var imgExt = Destination[ext..];
                DocumentViewModel.ImageExtension = imgExt;
                destinations.Add(docPath);
                destinations.AddRange(DocumentViewModel.AllTokens.Select(token => Path.Combine(directory, token + imgExt).RemoveTokens()));
            }

            return destinations;
        }
    }

    /// <summary>
    /// 只有pdf是单文件
    /// </summary>
    public override bool IsFolder => !Destination.EndsWith(".pdf");

    public override async Task DownloadAsync(Downloader downloadStreamAsync)
    {
        var count = DocumentViewModel.TotalCount;

        StartProgress = 0;
        ProgressRatio = .9 / count;
        for (var i = 0; i < count; ++i)
        {
            var dest = ActualDestinations.Count > i + 1 ? ActualDestinations[i + 1] : null;
            var stream = await DownloadAsyncCore(downloadStreamAsync, DocumentViewModel.AllUrls[i], dest);
            DocumentViewModel.SetStream(i, stream);
            StartProgress += 100 * ProgressRatio;
        }
        await ManageResult();
    }

    protected virtual async Task<Stream?> DownloadAsyncCore(Downloader downloadStreamAsync, string url, string? destination)
    {
        if (!App.AppViewModel.AppSettings.OverwriteDownloadedFile && destination is not null && File.Exists(destination))
            return null;

        if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<Stream>(MakoHelper.GetOriginalCacheKey(url)) is { } stream)
            return stream;

        if (await downloadStreamAsync(url, this, CancellationHandle) is Result<Stream>.Success result)
            return result.Value;

        return null;
    }

    protected virtual async Task ManageResult()
    {
        for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
        {
            var stream = DocumentViewModel.GetStream(i);
            stream.Position = 0;
        }

        switch (App.AppViewModel.AppSettings.NovelDownloadFormat)
        {
            case NovelDownloadFormat.Pdf:
                var document = DocumentViewModel.LoadPdfContent(CancellationHandle);
                document.GeneratePdf(ActualDestinations[0]);
                Report(100);
                return;
            case NovelDownloadFormat.OriginalTxt:
                await File.WriteAllTextAsync(ActualDestinations[0], NovelContent.Text);
                break;
            case NovelDownloadFormat.Html:
                var sbHtml = DocumentViewModel.LoadHtmlContent(CancellationHandle);
                await File.WriteAllTextAsync(ActualDestinations[0], sbHtml.ToString());
                break;
            case NovelDownloadFormat.Md:
                var sbMd = DocumentViewModel.LoadMdContent(CancellationHandle);
                await File.WriteAllTextAsync(ActualDestinations[0], sbMd.ToString());
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(App.AppViewModel.AppSettings.NovelDownloadFormat);
                break;
        }

        for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
        {
            var stream = DocumentViewModel.GetStream(i);
            var destination = ActualDestinations[i + 1];
            if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            {
                await stream.StreamSaveToFileAsync(destination);
            }
            else
            {
                using var image = await Image.LoadAsync(stream);
                if (DocumentViewModel.GetTags(i) is { } tags)
                    image.SetTags(tags);
                await image.IllustrationSaveToFileAsync(destination);
            }
        }

        Report(100);
    }

    private double StartProgress { get; set; }

    private double ProgressRatio { get; set; }

    public override void Report(double value) => base.Report(StartProgress + ProgressRatio * value);
}
