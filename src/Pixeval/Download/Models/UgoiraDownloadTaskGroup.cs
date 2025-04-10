// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Mako.Net.Response;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class UgoiraDownloadTaskGroup : DownloadTaskGroup, IImageDownloadTaskGroup
{
    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    private UgoiraMetadata Metadata { get; set; } = null!;

    private string TempFolderPath => $"{TokenizedDestination}.tmp";

    [MemberNotNull(nameof(Metadata))]
    private void SetMetadata(UgoiraMetadata metadata)
    {
        Metadata = metadata;
        var ugoiraOriginalUrls = Entry.GetUgoiraOriginalUrls(Metadata.Frames.Count);
        _ = Directory.CreateDirectory(TempFolderPath);
        for (var i = 0; i < ugoiraOriginalUrls.Count; ++i)
        {
            var imageDownloadTask = new ImageDownloadTask(new(ugoiraOriginalUrls[i]), $"{TempFolderPath}\\{i}{Path.GetExtension(ugoiraOriginalUrls[i])}", DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    public UgoiraDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        UgoiraDownloadFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
    }

    public UgoiraDownloadTaskGroup(Illustration entry, UgoiraMetadata metadata, string destination) : base(entry, destination, DownloadItemType.Ugoira)
    {
        UgoiraDownloadFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
        SetMetadata(metadata);
    }

    public UgoiraDownloadTaskGroup(Illustration entry, string destination) : base(entry, destination,
        DownloadItemType.Ugoira)
    {
        UgoiraDownloadFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(TokenizedDestination));
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (Metadata == null!)
            SetMetadata(await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(Entry.Id));
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (UgoiraDownloadFormat is UgoiraDownloadFormat.OriginalZip)
        {
            ZipFile.CreateFromDirectory(TempFolderPath, TokenizedDestination, CompressionLevel.Optimal, false);
        }
        else
        {
            using var image = await Destinations.UgoiraSaveToImageAsync([.. Metadata.Delays]);
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
