// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

/// <summary>
/// 只有<see cref="SingleAnimatedImageType.MultiFiles"/>使用这个类，其他使用<see cref="SingleImageDownloadTaskGroupBase"/>
/// </summary>
public partial class UgoiraDownloadTaskGroup : DownloadTaskGroup
{
    public ISingleAnimatedImage Entry => DatabaseEntry.Entry.To<ISingleAnimatedImage>();

    private string TempFolderPath { get; set; } = null!;

    private string CsvPath => Path.Combine(TempFolderPath, "intervals in milliseconds.csv");

    private IReadOnlyList<int> MsDelays { get; set; } = null!;

    private void SetTasksSet()
    {
        if (TasksSet.Count > 0)
            return;
        TempFolderPath = DestinationUgoiraFormat is UgoiraDownloadFormat.Original
            ? IoHelper.RemoveTokenExtension(TokenizedDestination)
            : TokenizedDestination + ".tmp";
        var msDelays = new int[Entry.MultiImageUris!.Count];
        for (var i = 0; i < Entry.MultiImageUris.Count; ++i)
        {
            var (uri, msDelay) = Entry.MultiImageUris[i];
            msDelays[i] = msDelay;
            var imageDownloadTask = new ImageDownloadTask(uri,
                $"{TempFolderPath}\\{i}{Path.GetExtension(uri.OriginalString)}", DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }
        MsDelays = msDelays;
        SetNotCreateFromEntry();
    }

    public UgoiraDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationUgoiraFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
    }

    public UgoiraDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination, DownloadItemType.Ugoira)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.MultiFiles)
            ThrowHelper.InvalidOperation($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.MultiFiles)}");
        if (!Entry.MultiImageUris!.IsPreloaded)
            ThrowHelper.InvalidOperation($"{nameof(ISingleAnimatedImage.MultiImageUris)} should be preloaded");
        DestinationUgoiraFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
    }

    public override ValueTask InitializeTaskGroupAsync()
    {
        SetTasksSet();
        return ValueTask.CompletedTask;
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (DestinationUgoiraFormat is UgoiraDownloadFormat.Original)
        {
            await File.WriteAllTextAsync(CsvPath,
                string.Join(',', MsDelays.Select(t => t.ToString())), token);
            return;
        }

        using var image = await Destinations.UgoiraSaveToImageAsync(MsDelays);
        image.SetIdTags(Entry);
        await image.UgoiraSaveToFileAsync(TokenizedDestination, DestinationUgoiraFormat);
        foreach (var imageDownloadTask in TasksSet)
            imageDownloadTask.Delete();
        FileHelper.DeleteEmptyFolder(TempFolderPath);
    }

    private UgoiraDownloadFormat DestinationUgoiraFormat { get; }

    public override string OpenLocalDestination => DestinationUgoiraFormat is UgoiraDownloadFormat.Original ? TempFolderPath : TokenizedDestination;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (DestinationUgoiraFormat is UgoiraDownloadFormat.Original)
        {
            if (File.Exists(CsvPath))
                File.Delete(CsvPath);
        }
        else if (File.Exists(TokenizedDestination))
            File.Delete(TokenizedDestination);

        FileHelper.DeleteEmptyFolder(TempFolderPath);
    }
}
