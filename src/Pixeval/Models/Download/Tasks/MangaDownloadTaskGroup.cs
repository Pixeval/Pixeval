// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Models.Database;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download.Tasks;

public class MangaDownloadTaskGroup : DownloadTaskGroup
{
    public IImageSet Entry => (IImageSet) DatabaseEntry.Entry;

    public MangaDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(TokenizedDestination));
    }

    public MangaDownloadTaskGroup(IImageSet entry, string destination) : base(entry, destination, DownloadItemType.Manga)
    {
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(TokenizedDestination));
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
            var imageDownloadTask = new ImageDownloadTask(page.ImageUri, IoHelper.ReplaceTokenExtensionFromUrl(TokenizedDestination, page.ImageUri, page.SetIndex), DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    protected override Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    private IllustrationDownloadFormat DestinationIllustrationFormat { get; }

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
        FileHelper.DeleteEmptyFolder(OpenLocalDestination);
    }
}
