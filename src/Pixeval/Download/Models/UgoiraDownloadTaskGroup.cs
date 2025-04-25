// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

/// <summary>
/// 只有<see cref="SingleAnimatedImageType.MultiFiles"/>使用这个类，其他使用<see cref="SingleImageDownloadTaskGroup"/>
/// </summary>
public partial class UgoiraDownloadTaskGroup : DownloadTaskGroup
{
    public ISingleAnimatedImage Entry => DatabaseEntry.Entry.To<ISingleAnimatedImage>();

    private string TempFolderPath => $"{TokenizedDestination}.tmp";

    private IReadOnlyList<int> MsDelays { get; set; }

    [MemberNotNull(nameof(MsDelays))]
    private void SetTasksSet()
    {
        // TODO 下载经常报错权限冲突
        _ = Directory.CreateDirectory(TempFolderPath);
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
        UgoiraDownloadFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
        MsDelays = null!;
    }

    public UgoiraDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination, DownloadItemType.Ugoira)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.MultiFiles)
            ThrowHelper.InvalidOperation($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.MultiFiles)}");
        if (!Entry.MultiImageUris!.IsPreloaded)
            ThrowHelper.InvalidOperation($"{nameof(ISingleAnimatedImage.MultiImageUris)} should be preloaded");
        UgoiraDownloadFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
        SetTasksSet();
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        await Entry.MultiImageUris!.TryPreloadListAsync(Entry);
        SetTasksSet();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (UgoiraDownloadFormat is UgoiraDownloadFormat.OriginalZip)
        {
            ZipFile.CreateFromDirectory(TempFolderPath, TokenizedDestination, CompressionLevel.Optimal, false);
        }
        else
        {
            using var image = await Destinations.UgoiraSaveToImageAsync(MsDelays);
            image.SetIdTags(Entry);
            await image.SaveAsync(TokenizedDestination, IoHelper.GetUgoiraEncoder(UgoiraDownloadFormat), token);
        }
        foreach (var imageDownloadTask in TasksSet)
            imageDownloadTask.Delete();
        IoHelper.DeleteEmptyFolder(TempFolderPath);
    }

    private UgoiraDownloadFormat UgoiraDownloadFormat { get; }

    public override string OpenLocalDestination => TokenizedDestination;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        IoHelper.DeleteEmptyFolder(TempFolderPath);
        if (File.Exists(TokenizedDestination))
            File.Delete(TokenizedDestination);
    }
}
