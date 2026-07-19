// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Database;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download.Tasks;

public class MangaDownloadTaskGroup : DownloadTaskGroup
{
    public IImageSet Entry => (IImageSet) DatabaseEntry.Entry;

    public MangaDownloadTaskGroup(DownloadHistoryEntryBase entry) : base(entry)
    {
        DestinationIllustrationFormat = GetFormatToken(entry);
    }

    public MangaDownloadTaskGroup(
        IImageSet entry,
        string destination,
        int? workSubscriptionId = null) : base(entry, destination, workSubscriptionId)
    {
        DestinationIllustrationFormat = IoHelper.GetAvailableIllustrationDownloadFormatToken();
        DatabaseEntry.FormatToken = DestinationIllustrationFormat.Value;
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
            var path = IoHelper.ReplaceTokenExtensionFromUrl(TokenizedDestination, page.ImageUri, page.SetIndex);
            var imageDownloadTask = new ImageDownloadTask(page.ImageUri, path, DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }

        SetNotCreateFromEntry();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (DestinationIllustrationFormat.ExtensionFormatExtension is not { } extension)
            return;

        var provider = GetExtensionService().GetStaticImageFormatProvider(extension)
                       ?? throw new NotSupportedException(extension);
        foreach (var task in TasksSet)
        {
            if (task.WasDownloadSkipped)
                continue;

            var tempPath = task.Destination + ".source";
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            FileHelper.Move(task.Destination, tempPath);
            try
            {
                await using var stream = File.OpenAsyncRead(tempPath);
                await provider.FormatImageAsync(stream, task.Destination);
            }
            catch
            {
                if (File.Exists(task.Destination))
                    File.Delete(task.Destination);
                throw;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }

    private IllustrationDownloadFormatToken DestinationIllustrationFormat { get; }

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

    private static IllustrationDownloadFormatToken GetFormatToken(DownloadHistoryEntryBase entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.FormatToken))
            return IoHelper.GetAvailableIllustrationDownloadFormatToken(entry.FormatToken);

        return IllustrationDownloadFormatToken.Default;
    }

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}
