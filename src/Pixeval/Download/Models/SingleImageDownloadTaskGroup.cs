// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class SingleImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleImage Entry => DatabaseEntry.Entry.To<ISingleImage>();

    private IllustrationDownloadFormat DestinationIllustrationFormat { get; }

    /// <inheritdoc />
    public SingleImageDownloadTaskGroup(ISingleImage entry, string destination) : base(entry, destination)
    {
        // DatabaseEntry.Destination可以包含未被替换的token，从此可以拿到IllustrationDownloadFormat.Original
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    /// <inheritdoc />
    public SingleImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (DestinationIllustrationFormat is not IllustrationDownloadFormat.Original)
            await ExifManager.SetTagsAsync(Destination, Entry, DestinationIllustrationFormat, token);
    }
}

public partial class SingleAnimatedImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleAnimatedImage Entry => DatabaseEntry.Entry.To<ISingleAnimatedImage>();

    private UgoiraDownloadFormat DestinationUgoiraFormat { get; }

    public SingleAnimatedImageDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile and not SingleAnimatedImageType.SingleFile)
            ThrowHelper.InvalidOperation($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.SingleZipFile)} or {nameof(SingleAnimatedImageType.SingleFile)}");
        // DatabaseEntry.Destination可以包含未被替换的token，从此可以拿到UgoiraDownloadFormat.Original
        DestinationUgoiraFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    public SingleAnimatedImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationUgoiraFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (DestinationUgoiraFormat is UgoiraDownloadFormat.Original)
            return;
        switch (Entry.PreferredAnimatedImageType)
        {
            case SingleAnimatedImageType.SingleZipFile:
            {
                await Entry.ZipImageDelays!.TryPreloadListAsync(Entry);
                var msDelays = Entry.ZipImageDelays!;
                var zipPath = Destination + ".zip";
                File.Move(Destination, zipPath);
                await using var read = IoHelper.OpenAsyncRead(zipPath);
                using var image = await read.UgoiraSaveToImageAsync(msDelays);
                image.SetIdTags(Entry);
                await image.UgoiraSaveToFileAsync(Destination, DestinationUgoiraFormat);
                break;
            }
            case SingleAnimatedImageType.SingleFile:
                await ExifManager.SetTagsAsync(Destination, Entry, DestinationUgoiraFormat, token);
                break;
        }
    }
}

public abstract class SingleImageDownloadTaskGroupBase : ImageDownloadTask, IDownloadTaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; }

    public ValueTask InitializeTaskGroupAsync()
    {
        SetNotCreateFromEntry();
        return ValueTask.CompletedTask;
    }

    public string Id => DatabaseEntry.Entry.Id;

    public SingleImageDownloadTaskGroupBase(IArtworkInfo entry, string destination) : this(new(destination, DownloadItemType.Illustration, entry))
    {
        CurrentState = DownloadState.Queued;
        ProgressPercentage = 0;
    }

    public SingleImageDownloadTaskGroupBase(DownloadHistoryEntry entry) : base(GetImageUri(entry.Entry),
        IoHelper.ReplaceTokenExtensionFromUrl(entry.Destination, GetImageUri(entry.Entry)))
    {
        DatabaseEntry = entry;
        CurrentState = entry.State;
        if (entry.State is DownloadState.Completed or DownloadState.Cancelled or DownloadState.Error)
            ProgressPercentage = 100;
    }

    private static Uri GetImageUri(IArtworkInfo info) =>
        info switch
        {
            ISingleImage { ImageType: ImageType.SingleImage } singleImage => singleImage.ImageUri,
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.SingleZipFile or SingleAnimatedImageType.SingleFile
            } animatedImage
                => animatedImage.SingleImageUri!,
            _ => ThrowHelper.NotSupported<Uri>(info.ToString())
        };

    private void SetNotCreateFromEntry()
    {
        if (!IsCreateFromEntry)
            return;
        IsCreateFromEntry = false;
        PropertyChanged += (sender, e) =>
        {
            var g = sender.To<SingleImageDownloadTaskGroupBase>();
            if (e.PropertyName is not nameof(CurrentState))
                return;
            if (g.CurrentState is DownloadState.Running or DownloadState.Paused)
                return;
            g.DatabaseEntry.State = g.CurrentState;
            var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
            manager.Update(g.DatabaseEntry);
        };
    }

    private bool IsCreateFromEntry { get; set; } = true;

    public DownloadToken GetToken() => new(this, CancellationTokenSource.Token);

    public int ActiveCount => CurrentState is DownloadState.Queued or DownloadState.Running or DownloadState.Pending or DownloadState.Paused or DownloadState.Cancelled ? 1 : 0;

    public int CompletedCount => CurrentState is DownloadState.Completed ? 1 : 0;

    public int ErrorCount => CurrentState is DownloadState.Error ? 1 : 0;

    public void SubscribeProgress(ChannelWriter<DownloadToken> writer)
    {
        DownloadTryResume += OnDownloadWrite;
        DownloadTryReset += OnDownloadWrite;

        return;
        void OnDownloadWrite(ImageDownloadTask o) => writer.TryWrite(o.To<SingleImageDownloadTaskGroupBase>().GetToken());
    }

    public int Count => 1;

    public IEnumerator<ImageDownloadTask> GetEnumerator() => ((IReadOnlyList<ImageDownloadTask>) [this]).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
