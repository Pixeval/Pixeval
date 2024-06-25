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
    private string DocPath { get; set; } = null!;

    private string PdfTempFolderPath => $"{TokenizedDestination}.tmp";

    [MemberNotNull(nameof(NovelContent), nameof(DocumentViewModel), nameof(DocPath))]
    private void SetNovelContent(
        NovelContent novelContent,
        DocumentViewerViewModel? documentViewModel = null)
    {
        NovelContent = novelContent;
        DocumentViewModel = documentViewModel ?? new DocumentViewerViewModel(novelContent);
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        // ..\[ID] NovelName.pdf
        // ..\[ID] NovelName\novel.txt
        DocPath = TokenizedDestination[..backSlash];
        // <ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        var directory = DocPath.EndsWith(".pdf") ? PdfTempFolderPath : Path.GetDirectoryName(DocPath)!;

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
                { Stream = DocumentViewModel.GetStream(i) };
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    public NovelDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        string destination) : base(entry, destination, DownloadItemType.Novel)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        NovelContent novelContent,
        DocumentViewerViewModel documentViewModel,
        string destination) : base(entry, destination, DownloadItemType.Novel) =>
        SetNovelContent(novelContent, documentViewModel);

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (NovelContent == null!)
            SetNovelContent(await App.AppViewModel.MakoClient.GetNovelContentAsync(Entry.Id));
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
        {
            // TODO: bug
            var stream = DocumentViewModel.GetStream(i);
            stream.Position = 0;
        }

        string content;
        switch (App.AppViewModel.AppSettings.NovelDownloadFormat)
        {
            case NovelDownloadFormat.Pdf:
                var document = DocumentViewModel.LoadPdfContent();
                document.GeneratePdf(DocPath);
                Directory.Delete(PdfTempFolderPath);
                return;
            case NovelDownloadFormat.OriginalTxt:
                content = NovelContent.Text;
                break;
            case NovelDownloadFormat.Html:
                content = DocumentViewModel.LoadHtmlContent().ToString();
                break;
            case NovelDownloadFormat.Md:
                content = DocumentViewModel.LoadMdContent().ToString();
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(App.AppViewModel.AppSettings.NovelDownloadFormat);
                return;
        }

        await File.WriteAllTextAsync(DocPath, content);
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;

        var destinations = Destinations;
        for (var i = 0; i < destinations.Count; ++i)
            if (DocumentViewModel.GetIdTags(i) is { Id: var id, Tags: var tags })
                await TagsManager.SetIdTagsAsync(destinations[i], id, tags);
    }

    public override string OpenLocalDestination => DocPath.EndsWith(".pdf") ? DocPath : Path.GetDirectoryName(DocPath)!;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (DocPath.EndsWith(".pdf"))
        {
            IoHelper.DeleteEmptyFolder(PdfTempFolderPath);
            if (File.Exists(DocPath))
                File.Delete(DocPath);
        }
        else
        {
            if (File.Exists(DocPath))
                File.Delete(DocPath);
            IoHelper.DeleteEmptyFolder(Path.GetDirectoryName(DocPath));
        }
    }
}
