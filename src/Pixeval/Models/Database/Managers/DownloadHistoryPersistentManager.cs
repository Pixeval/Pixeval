// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Misaki;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public sealed class DownloadHistoryPersistentManager(SQLiteConnection db, FileLogger logger)
    : ArtworkHistoryPersistentManager<DownloadHistoryEntry>(db, logger)
{
    public DownloadHistoryEntry? GetByDestination(string destination) =>
        string.IsNullOrWhiteSpace(destination)
            ? null
            : FindEntry(entry => entry.Destination == destination);

    public void AddOrReplace(DownloadHistoryEntry entry) =>
        InsertReplacing(entry, query => query.FirstOrDefault(item => item.Destination == entry.Destination));

    public async IAsyncEnumerable<DownloadHistoryEntry> GetBySubscriptionIdAsync(
        int subscriptionEntryId,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        if (subscriptionEntryId <= 0)
            yield break;

        await foreach (var entry in QueryEntriesAsync(
                               item => item.WorkSubscriptionId == subscriptionEntryId,
                               token: token)
                           .ConfigureAwait(false))
            yield return entry;
    }

    public async IAsyncEnumerable<IDownloadTaskGroup> StreamTaskGroupsAsync(
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (var entry in StreamEntriesAsync(skip, token).ConfigureAwait(false))
            yield return ToModel(entry);
    }

    public bool TryDeleteByDestination(string destination) =>
        !string.IsNullOrWhiteSpace(destination)
        && TryDelete(query => query.FirstOrDefault(entry => entry.Destination == destination));

    private static IDownloadTaskGroup ToModel(DownloadHistoryEntry entry)
    {
        return entry.Entry switch
        {
            ISingleImage { ImageType: ImageType.SingleImage } or
                ISingleImage { ImageType: ImageType.ImageSet, SetIndex: > -1 } => new SingleImageDownloadTaskGroup(entry),
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.SingleZipFile or SingleAnimatedImageType.SingleFile
            } => new SingleAnimatedImageDownloadTaskGroup(entry),
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.MultiFiles
            } => new UgoiraDownloadTaskGroup(entry),
            IImageSet { ImageType: ImageType.ImageSet } => new MangaDownloadTaskGroup(entry),
            Novel => new NovelDownloadTaskGroup(entry),
            _ => new SingleImageDownloadTaskGroup(entry)
        };
    }
}
