// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Download;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Database;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.ViewModels;

namespace Pixeval.Models.Download.Tasks;

public class NovelDownloadTaskGroup : DownloadTaskGroup
{
    public Novel Entry => (Novel) DatabaseEntry.Entry;

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

    private NovelDownloadFormatToken DestinationNovelFormat { get; }

    private string TempImageFolderPath { get; }

    [MemberNotNull(nameof(NovelContent), nameof(DocumentViewModel))]
    private void SetNovelContent(NovelContent novelContent)
    {
        NovelContent = novelContent;
        DocumentViewModel = new NovelContext(novelContent);
        var directory = DestinationNovelFormat.IsExtension
            ? TempImageFolderPath
            : Path.GetDirectoryName(DocPath)!;

        var imageFormat = DestinationNovelFormat.IsExtension
            ? IllustrationDownloadFormat.Png
            : DestinationIllustrationFormat;
        var imgExt = IoHelper.GetIllustrationExtension(imageFormat);
        if (imageFormat is not IllustrationDownloadFormat.Original)
            DocumentViewModel.ImageExtension = imgExt;
        for (var i = 0; i < DocumentViewModel.TotalImagesCount; ++i)
        {
            var url = DocumentViewModel.AllUrls[i];
            var name = Path.Combine(directory, DocumentViewModel.AllTokens[i]);
            var imageDownloadTask = new ImageDownloadTask(new(url),
                imageFormat is IllustrationDownloadFormat.Original
                    ? IoHelper.ReplaceTokenExtensionFromUrl(name, url, -1)
                    : name + imgExt,
                DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }

        SetNotCreateFromEntry();
    }

    public NovelDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        var separatorIndex = TokenizedDestination.LastIndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        DocPath = TokenizedDestination[..separatorIndex];
        TempImageFolderPath = $"{DocPath}.tmp";
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(separatorIndex + 1)..];
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(imgExt);
        DestinationNovelFormat = GetNovelFormat(DocPath);
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        string destination,
        NovelContent? novelContent) : base(entry, destination, DownloadItemType.Novel)
    {
        var separatorIndex = TokenizedDestination.LastIndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        DocPath = TokenizedDestination[..separatorIndex];
        TempImageFolderPath = $"{DocPath}.tmp";
        // .<ext> or .png or .etc 
        var imgExt = TokenizedDestination[(separatorIndex + 1)..];
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(imgExt);
        DestinationNovelFormat = GetNovelFormat(DocPath);
        if (novelContent is not null)
            SetNovelContent(novelContent);
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (NovelContent == null!)
            SetNovelContent(await App.AppViewModel.MakoClient.GetNovelContentAsync(Entry.Id));

        // 小说若无图片，则没有子任务。所以此类任务是瞬间完成，理论上不存在其他状态
        if (DatabaseEntry.State is DownloadState.Queued && TasksSet.Count is 0)
            await AllTasksDownloadedAsync();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        ((INovelContext<Stream>) DocumentViewModel).InitImages();
        if (DestinationNovelFormat.ExtensionFormatExtension is { } extension)
        {
            await FormatByExtensionAsync(extension);
            return;
        }

        await FormatBuiltInAsync(token);
    }

    private async Task FormatBuiltInAsync(CancellationToken token)
    {
        var format = DestinationNovelFormat.BuiltInFormat ?? NovelDownloadFormatToken.DefaultBuiltInFormat;
        var content = format switch
        {
            NovelDownloadFormat.OriginalTxt => NovelContent.Text,
            NovelDownloadFormat.Html => DocumentViewModel.LoadHtmlContent().ToString(),
            NovelDownloadFormat.Md => DocumentViewModel.LoadMdContent().ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        FileHelper.CreateParentDirectory(DocPath);
        await File.WriteAllTextAsync(DocPath, content, token);
    }

    private async Task FormatByExtensionAsync(string extension)
    {
        var provider = GetExtensionService().GetNovelFormatProvider(extension)
            ?? throw new NotSupportedException(extension);

        var streams = new Dictionary<string, Stream>();
        try
        {
            for (var i = 0; i < TasksSet.Count; i++)
            {
                var imageDownloadTask = TasksSet[i];
                var imageStream = File.OpenRead(imageDownloadTask.Destination);
                streams.Add(Path.GetFileName(imageDownloadTask.Destination), imageStream);
                DocumentViewModel.SetStream(i, imageStream);
            }

            FileHelper.CreateParentDirectory(DocPath);
            await provider.FormatNovelAsync(NovelContent.Text, DocPath, streams);
        }
        finally
        {
            await DeleteTemporaryImageTasksAsync();
            streams.Clear();
        }
    }

    private async Task DeleteTemporaryImageTasksAsync()
    {
        for (var i = 0; i < TasksSet.Count; i++)
        {
            if (DocumentViewModel.TryGetStream(i) is { } stream)
                await stream.DisposeAsync();
            TasksSet[i].Delete();
        }

        FileHelper.DeleteEmptyFolder(TempImageFolderPath);
    }

    public override string OpenLocalDestination => DocPath;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (File.Exists(DocPath))
            File.Delete(DocPath);
        FileHelper.DeleteEmptyFolder(DestinationNovelFormat.IsExtension ? TempImageFolderPath : Path.GetDirectoryName(DocPath));
    }

    private static NovelDownloadFormatToken GetNovelFormat(string docPath)
    {
        var extension = Path.GetExtension(docPath);
        if (IoHelper.TryGetNovelFormat(extension, out var builtInFormat))
            return NovelDownloadFormatToken.BuiltIn(builtInFormat);

        return NovelDownloadFormatToken.ExtensionPrefix + extension;
    }

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}
