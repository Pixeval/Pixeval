// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class SingleImageDownloadTaskGroup : ImageDownloadTask, IDownloadTaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; }

    public ValueTask InitializeTaskGroupAsync()
    {
        SetNotCreateFromEntry();
        return ValueTask.CompletedTask;
    }

    public IArtworkInfo Entry => DatabaseEntry.Entry.To<IArtworkInfo>();

    public string Id => DatabaseEntry.Entry.Id;

    public SingleImageDownloadTaskGroup(IArtworkInfo entry, string destination) : this(new(destination, DownloadItemType.Illustration, entry))
    {
        CurrentState = DownloadState.Queued;
        ProgressPercentage = 0;
        SetNotCreateFromEntry();
    }

    public SingleImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(GetImageUri(entry.Entry),
        IoHelper.ReplaceTokenExtensionFromUrl(entry.Destination, GetImageUri(entry.Entry)))
    {
        DatabaseEntry = entry;
        CurrentState = entry.State;
        if (entry.State is DownloadState.Completed or DownloadState.Cancelled or DownloadState.Error)
            ProgressPercentage = 100;
        // DatabaseEntry.Destination可以包含未被替换的token，从此可以拿到IllustrationDownloadFormat.Original
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    private static Uri GetImageUri(IArtworkInfo info) =>
        info switch
        {
            ISingleImage { ImageType: ImageType.SingleImage } singleImage => singleImage.ImageUri,
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.SingleFile
            } animatedImage => animatedImage.SingleImageUri!,
            _ => ThrowHelper.NotSupported<Uri>(info.ToString())
        };

    private void SetNotCreateFromEntry()
    {
        if (!IsCreateFromEntry)
            return;
        IsCreateFromEntry = false;
        PropertyChanged += (sender, e) =>
        {
            var g = sender.To<SingleImageDownloadTaskGroup>();
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

    private IllustrationDownloadFormat IllustrationDownloadFormat { get; }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        await ExifManager.SetTagsAsync(Destination, Entry, IllustrationDownloadFormat, token);
    }

    public DownloadToken GetToken() => new(this, CancellationTokenSource.Token);

    public int ActiveCount => CurrentState is DownloadState.Queued or DownloadState.Running or DownloadState.Pending or DownloadState.Paused or DownloadState.Cancelled ? 1 : 0;

    public int CompletedCount => CurrentState is DownloadState.Completed ? 1 : 0;

    public int ErrorCount => CurrentState is DownloadState.Error ? 1 : 0;

    public void SubscribeProgress(ChannelWriter<DownloadToken> writer)
    {
        DownloadTryResume += OnDownloadWrite;
        DownloadTryReset += OnDownloadWrite;

        return;
        void OnDownloadWrite(ImageDownloadTask o) => writer.TryWrite(o.To<SingleImageDownloadTaskGroup>().GetToken());
    }

    public int Count => 1;

    public IEnumerator<ImageDownloadTask> GetEnumerator() => ((IReadOnlyList<ImageDownloadTask>) [this]).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
