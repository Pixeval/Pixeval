// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Database;
using Pixeval.Download.Macros;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class MangaDownloadTaskGroup : DownloadTaskGroup
{
    public IImageSet Entry => DatabaseEntry.Entry.To<IImageSet>();

    public MangaDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(TokenizedDestination));
    }

    public MangaDownloadTaskGroup(IImageSet entry, string destination) : base(entry, destination, DownloadItemType.Manga)
    {
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(TokenizedDestination));
        SetTasksSet();
    }

    public override ValueTask InitializeTaskGroupAsync()
    {
        SetTasksSet();
        return ValueTask.CompletedTask;
    }

    private void SetTasksSet()
    {
        if (TasksSet.Count > 0)
            return;
        foreach (var page in Entry.Pages)
        {
            var imageDownloadTask = new ImageDownloadTask(page.ImageUri, IoHelper.ReplaceTokenExtensionFromUrl(TokenizedDestination, page.ImageUri).Replace(PicSetIndexMacro.NameConstToken, page.SetIndex.ToString()), DatabaseEntry.State);
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
