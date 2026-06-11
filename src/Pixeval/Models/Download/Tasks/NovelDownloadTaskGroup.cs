// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

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

    private NovelDownloadFormatToken DestinationNovelFormat { get; }

    private string NovelFile { get; }

    private string TempImageFolderPath { get; }

    [MemberNotNull(nameof(NovelContent), nameof(DocumentViewModel))]
    private void SetNovelContent(NovelContent novelContent)
    {
        NovelContent = novelContent;
        DocumentViewModel = new NovelContext(novelContent);
        var directory = DestinationNovelFormat.BuiltInFormat is null
            ? Path.GetDirectoryName(NovelFile)!
            : TempImageFolderPath;

        for (var i = 0; i < DocumentViewModel.TotalImagesCount; ++i)
        {
            var url = DocumentViewModel.AllUrls[i];
            var name = Path.Combine(directory, DocumentViewModel.AllTokens[i]);
            var imageDownloadTask = new ImageDownloadTask(new(url), IoHelper.ReplaceTokenExtensionFromUrl(name, url, -1),
                DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }

        SetNotCreateFromEntry();
    }

    public NovelDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationNovelFormat = GetNovelFormat(entry);

        // --- 设置只读路径
        NovelFile = IoHelper.ChangeExtension(TokenizedDestination, IoHelper.GetNovelExtension(DestinationNovelFormat));
        TempImageFolderPath = NovelFile + IoHelper.PixevalTempExtension;
        // ---
    }

    public NovelDownloadTaskGroup(
        Novel entry,
        string destination,
        NovelContent? novelContent) : base(entry, destination, DownloadItemType.Novel)
    {
        DestinationNovelFormat = IoHelper.GetAvailableNovelDownloadFormatToken();
        DatabaseEntry.FormatToken = DestinationNovelFormat.Value;

        // --- 设置只读路径
        NovelFile = IoHelper.ChangeExtension(TokenizedDestination, IoHelper.GetNovelExtension(DestinationNovelFormat));
        TempImageFolderPath = NovelFile + IoHelper.PixevalTempExtension;
        // ---

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

        FileHelper.CreateParentDirectory(NovelFile);
        await File.WriteAllTextAsync(NovelFile, content, token);
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

            FileHelper.CreateParentDirectory(NovelFile);
            await provider.FormatNovelAsync(NovelContent.Text, NovelFile, streams);
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

    public override string OpenLocalDestination => NovelFile;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (File.Exists(NovelFile))
            File.Delete(NovelFile);
        FileHelper.DeleteEmptyFolder(DestinationNovelFormat.BuiltInFormat is null
            ? Path.GetDirectoryName(NovelFile)
            : TempImageFolderPath);
    }

    private static NovelDownloadFormatToken GetNovelFormat(DownloadHistoryEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.FormatToken))
            return IoHelper.GetAvailableNovelDownloadFormatToken(entry.FormatToken);

        return NovelDownloadFormatToken.Default;
    }

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}
