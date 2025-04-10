// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Controls;
using Mako.Model;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using QuestPDF.Fluent;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class NovelDownloadTaskGroup : DownloadTaskGroup
{
    public Novel Entry => DatabaseEntry.Entry.To<Novel>();

    private NovelContent NovelContent { get; set; } = null!;

    private NovelContext DocumentViewModel { get; set; } = null!;

    /// <summary>
    /// 小说正文的保存路径
    /// </summary>
    /// <remarks>
    /// ..\[ID] NovelName.pdf<br/>
    /// ..\[ID] NovelName\novel.txt
    /// </remarks>
    private string DocPath { get; }

    private IllustrationDownloadFormat IllustrationDownloadFormat { get; }

    private NovelDownloadFormat NovelDownloadFormat { get; }

    private string PdfTempFolderPath { get; }

    [MemberNotNull(nameof(NovelContent), nameof(DocumentViewModel))]
    private void SetNovelContent(
        NovelContent novelContent)
    {
        NovelContent = novelContent;
        DocumentViewModel = new NovelContext(novelContent);
        var directory = NovelDownloadFormat is NovelDownloadFormat.Pdf
            ? PdfTempFolderPath
            : Path.GetDirectoryName(DocPath)!;

        _ = Directory.CreateDirectory(directory);
        var imgExt = IoHelper.GetIllustrationExtension(IllustrationDownloadFormat);
        if (IllustrationDownloadFormat is not IllustrationDownloadFormat.Original)
            DocumentViewModel.ImageExtension = imgExt;
        for (var i = 0; i < DocumentViewModel.TotalImagesCount; ++i)
        {
            var url = DocumentViewModel.AllUrls[i];
            var name = Path.Combine(directory, DocumentViewModel.AllTokens[i]);
            var imageDownloadTask = new ImageDownloadTask(new(url),
                IllustrationDownloadFormat is IllustrationDownloadFormat.Original
                    ? IoHelper.ReplaceTokenExtensionFromUrl(name, url)
                    : name + imgExt,
                DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }

        SetNotCreateFromEntry();
    }

    public NovelDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(imgExt);
        NovelDownloadFormat = IoHelper.GetNovelFormat(Path.GetExtension(DocPath));
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        string destination) : base(entry, destination, DownloadItemType.Novel)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(imgExt);
        NovelDownloadFormat = IoHelper.GetNovelFormat(Path.GetExtension(DocPath));
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        NovelContent novelContent,
        string destination) : base(entry, destination, DownloadItemType.Novel)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(imgExt);
        NovelDownloadFormat = IoHelper.GetNovelFormat(Path.GetExtension(DocPath));
        SetNovelContent(novelContent);
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (NovelContent == null!)
            SetNovelContent(await App.AppViewModel.MakoClient.GetNovelContentAsync(Entry.Id));

        if (TasksSet.Count is 0)
            await AllTasksDownloadedAsync();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (NovelDownloadFormat is NovelDownloadFormat.Pdf)
        {
            var i = 0;
            foreach (var imageDownloadTask in TasksSet)
            {
                DocumentViewModel.SetStream(i, IoHelper.OpenAsyncRead(imageDownloadTask.Destination));
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

        ((INovelContext<Stream>) DocumentViewModel).InitImages();

        var content = NovelDownloadFormat switch
        {
            NovelDownloadFormat.OriginalTxt => NovelContent.Text,
            NovelDownloadFormat.Html => DocumentViewModel.LoadHtmlContent().ToString(),
            NovelDownloadFormat.Md => DocumentViewModel.LoadMdContent().ToString(),
            _ => ThrowHelper.ArgumentOutOfRange<NovelDownloadFormat, string>(NovelDownloadFormat)
        };

        await File.WriteAllTextAsync(DocPath, content, token);
        if (IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;

        var destinations = Destinations;
        for (var i = 0; i < destinations.Count; ++i)
            if (DocumentViewModel.GetIdTags(i) is { Id: var id, Tags: var tags })
                await ExifManager.SetIdTagsAsync(destinations[i], id, tags, IllustrationDownloadFormat, token);
    }

    public override string OpenLocalDestination => DocPath;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (File.Exists(DocPath))
            File.Delete(DocPath);
        IoHelper.DeleteEmptyFolder(NovelDownloadFormat is NovelDownloadFormat.Pdf ? PdfTempFolderPath : Path.GetDirectoryName(DocPath));
    }
}
