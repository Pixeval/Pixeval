// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Database;
using Pixeval.Download.Macros;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class MangaDownloadTaskGroup : DownloadTaskGroup, IImageDownloadTaskGroup
{
    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    public MangaDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(TokenizedDestination));
    }

    public MangaDownloadTaskGroup(Illustration entry, string destination, IReadOnlyList<Stream?>? streams = null) : base(entry, destination, DownloadItemType.Manga)
    {
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(TokenizedDestination));
        SetTasksSet(streams);
    }

    public override ValueTask InitializeTaskGroupAsync()
    {
        SetTasksSet();
        return ValueTask.CompletedTask;
    }

    private void SetTasksSet(IReadOnlyList<Stream?>? streams = null)
    {
        if (TasksSet.Count > 0)
            return;
        var mangaOriginalUrls = Entry.MangaOriginalUrls;
        for (var i = 0; i < mangaOriginalUrls.Count; ++i)
        {
            var imageDownloadTask = new ImageDownloadTask(new(mangaOriginalUrls[i]), IoHelper.ReplaceTokenExtensionFromUrl(TokenizedDestination, mangaOriginalUrls[i]).Replace(PicSetIndexMacro.NameConstToken, i.ToString()), DatabaseEntry.State)
            {
                Stream = streams?[i]
            };
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        foreach (var destination in Destinations)
        {
            if (token.IsCancellationRequested)
                return;
            await ExifManager.SetTagsAsync(destination, Entry, IllustrationDownloadFormat, token);
        }
    }

    private IllustrationDownloadFormat IllustrationDownloadFormat { get; }

    public override string OpenLocalDestination
    {
        get
        {
            if (TasksSet.Count is 0)
                SetTasksSet();
            return Path.GetDirectoryName(TasksSet[0].Destination)!;
        }
    }

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        IoHelper.DeleteEmptyFolder(OpenLocalDestination);
    }
}
