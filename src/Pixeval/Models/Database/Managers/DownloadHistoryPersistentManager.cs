// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Mako.Model;
using Misaki;
using Pixeval.Models.Download.Tasks;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public class DownloadHistoryPersistentManager(SQLiteConnection db)
    : PersistentManagerBase<DownloadHistoryEntry, IDownloadTaskGroup>(db, App.AppViewModel.AppSettings.MaximumDownloadHistoryRecords)
{
    public DownloadHistoryEntry? GetByDestination(string destination) =>
        string.IsNullOrWhiteSpace(destination)
            ? null
            : Queryable.FirstOrDefault(t => t.Destination == destination);

    public IReadOnlyList<DownloadHistoryEntry> GetByFolderId(int downloadFolderId) =>
        downloadFolderId <= 0
            ? []
            : Queryable.Where(t => t.DownloadFolderId == downloadFolderId).ToArray();

    public override IDownloadTaskGroup ToModel(DownloadHistoryEntry entry)
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
