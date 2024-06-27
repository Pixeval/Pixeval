#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelDownloadTaskGroup.cs
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Download.Macros;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using QuestPDF.Fluent;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public class NovelDownloadTaskGroup : DownloadTaskGroup
{
    public Novel Entry => DatabaseEntry.Entry.To<Novel>();

    private NovelContent NovelContent { get; set; } = null!;

    private DocumentViewerViewModel DocumentViewModel { get; set; } = null!;

    /// <summary>
    /// 小说正文的保存路径
    /// </summary>
    /// <remarks>
    /// ..\[ID] NovelName.pdf<br/>
    /// ..\[ID] NovelName\novel.txt
    /// </remarks>
    private string DocPath { get; }

    private bool IsPdf => DocPath.EndsWith(".pdf");

    private string PdfTempFolderPath { get; }

    [MemberNotNull(nameof(NovelContent), nameof(DocumentViewModel))]
    private void SetNovelContent(
        NovelContent novelContent,
        DocumentViewerViewModel? documentViewModel = null)
    {
        NovelContent = novelContent;
        DocumentViewModel = documentViewModel ?? new DocumentViewerViewModel(novelContent);
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        // <ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        var directory = IsPdf ? PdfTempFolderPath : Path.GetDirectoryName(DocPath)!;

        _ = Directory.CreateDirectory(directory);
        var flag = false;
        if (imgExt == FileExtensionMacro.NameConstToken)
            flag = true;
        else
            DocumentViewModel.ImageExtension = imgExt;
        for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
        {
            var url = DocumentViewModel.AllUrls[i];
            var name = Path.Combine(directory, DocumentViewModel.AllTokens[i]);
            var imageDownloadTask = new ImageDownloadTask(new(url), flag
                    ? IoHelper.ReplaceTokenExtensionFromUrl(name, url)
                    : name + imgExt)
            { Stream = DocumentViewModel.TryGetStream(i) };
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    public NovelDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        string destination) : base(entry, destination, DownloadItemType.Novel)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        NovelContent novelContent,
        DocumentViewerViewModel documentViewModel,
        string destination) : base(entry, destination, DownloadItemType.Novel)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
        SetNovelContent(novelContent, documentViewModel);
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (NovelContent == null!)
            SetNovelContent(await App.AppViewModel.MakoClient.GetNovelContentAsync(Entry.Id));
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        if (IsPdf)
        {
            var i = 0;
            foreach (var imageDownloadTask in TasksSet)
            {
                DocumentViewModel.SetStream(i, File.OpenRead(imageDownloadTask.Destination));
                ++i;
            }
            var document = DocumentViewModel.LoadPdfContent();
            document.GeneratePdf(DocPath);
            i = 0;
            foreach (var imageDownloadTask in TasksSet)
            {
                if (DocumentViewModel.TryGetStream(i) is { } stream)
                    await stream.DisposeAsync();
                imageDownloadTask.Delete();
                ++i;
            }
            IoHelper.DeleteEmptyFolder(PdfTempFolderPath);
            return;
        }

        // TODO: 更优雅的判断格式，如直接从外部传入NovelDownloadFormat
        var content = Path.GetExtension(DocPath) switch
        {
            ".txt" => NovelContent.Text,
            ".html" => DocumentViewModel.LoadHtmlContent().ToString(),
            ".md" => DocumentViewModel.LoadMdContent().ToString(),
            _ => ThrowHelper.ArgumentOutOfRange<NovelDownloadFormat, string>(App.AppViewModel.AppSettings.NovelDownloadFormat)
        };

        await File.WriteAllTextAsync(DocPath, content);
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;

        var destinations = Destinations;
        for (var i = 0; i < destinations.Count; ++i)
            if (DocumentViewModel.GetIdTags(i) is { Id: var id, Tags: var tags })
                await TagsManager.SetIdTagsAsync(destinations[i], id, tags);
    }

    public override string OpenLocalDestination => DocPath;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (File.Exists(DocPath))
            File.Delete(DocPath);
        IoHelper.DeleteEmptyFolder(IsPdf ? PdfTempFolderPath : Path.GetDirectoryName(DocPath));
    }
}
