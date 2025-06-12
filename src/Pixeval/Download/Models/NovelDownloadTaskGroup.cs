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

    private IllustrationDownloadFormat DestinationIllustrationFormat { get; }

    private NovelDownloadFormat DestinationNovelFormat { get; }

    private string PdfTempFolderPath { get; }

    [MemberNotNull(nameof(NovelContent), nameof(DocumentViewModel))]
    private void SetNovelContent(
        NovelContent novelContent)
    {
        NovelContent = novelContent;
        DocumentViewModel = new NovelContext(novelContent);
        var directory = DestinationNovelFormat is NovelDownloadFormat.Pdf
            ? PdfTempFolderPath
            : Path.GetDirectoryName(DocPath)!;

        var imgExt = IoHelper.GetIllustrationExtension(DestinationIllustrationFormat);
        if (DestinationIllustrationFormat is not IllustrationDownloadFormat.Original)
            DocumentViewModel.ImageExtension = imgExt;
        for (var i = 0; i < DocumentViewModel.TotalImagesCount; ++i)
        {
            var url = DocumentViewModel.AllUrls[i];
            var name = Path.Combine(directory, DocumentViewModel.AllTokens[i]);
            var imageDownloadTask = new ImageDownloadTask(new(url),
                DestinationIllustrationFormat is IllustrationDownloadFormat.Original
                    ? IoHelper.ReplaceTokenExtensionFromUrl(name, url, -1)
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
        FileHelper.CreateParentDirectories(DocPath);
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(imgExt);
        DestinationNovelFormat = IoHelper.GetNovelFormat(Path.GetExtension(DocPath));
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        string destination,
        NovelContent? novelContent) : base(entry, destination, DownloadItemType.Novel)
    {
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        DocPath = TokenizedDestination[..backSlash];
        PdfTempFolderPath = $"{DocPath}.tmp";
        FileHelper.CreateParentDirectories(DocPath);
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(imgExt);
        DestinationNovelFormat = IoHelper.GetNovelFormat(Path.GetExtension(DocPath));
        if (novelContent is not null)
            SetNovelContent(novelContent);
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (NovelContent == null!)
            SetNovelContent(await App.AppViewModel.MakoClient.GetNovelContentAsync(Entry.Id));
        // 如果小说正文内容为空，说明是从数据库刚读出的任务，不要写文件
        // 小说若无图片，则没有子任务。所以此类任务是瞬间完成，理论上不存在其他状态
        else if (TasksSet.Count is 0)
            await AllTasksDownloadedAsync();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (DestinationNovelFormat is NovelDownloadFormat.Pdf)
        {
            var i = 0;
            foreach (var imageDownloadTask in TasksSet)
            {
                DocumentViewModel.SetStream(i, FileHelper.OpenAsyncRead(imageDownloadTask.Destination));
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

            FileHelper.DeleteEmptyFolder(PdfTempFolderPath);
            return;
        }

        ((INovelContext<Stream>) DocumentViewModel).InitImages();

        var content = DestinationNovelFormat switch
        {
            NovelDownloadFormat.OriginalTxt => NovelContent.Text,
            NovelDownloadFormat.Html => DocumentViewModel.LoadHtmlContent().ToString(),
            NovelDownloadFormat.Md => DocumentViewModel.LoadMdContent().ToString(),
            _ => ThrowHelper.ArgumentOutOfRange<NovelDownloadFormat, string>(DestinationNovelFormat)
        };

        await File.WriteAllTextAsync(DocPath, content, token);
        if (DestinationIllustrationFormat is IllustrationDownloadFormat.Original)
            return;

        var destinations = Destinations;
        for (var i = 0; i < destinations.Count; ++i)
            if (DocumentViewModel.GetIdTags(i) is { Id: var id, Tags: var tags })
                await ExifManager.SetIdTagsAsync(destinations[i], id.ToString(), tags, DestinationIllustrationFormat, token);
    }

    public override string OpenLocalDestination => DocPath;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (File.Exists(DocPath))
            File.Delete(DocPath);
        FileHelper.DeleteEmptyFolder(DestinationNovelFormat is NovelDownloadFormat.Pdf ? PdfTempFolderPath : Path.GetDirectoryName(DocPath));
    }
}
