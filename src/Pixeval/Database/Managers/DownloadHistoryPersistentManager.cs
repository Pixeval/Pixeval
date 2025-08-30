// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using LiteDB;
using Mako.Model;
using Misaki;
using Pixeval.Download.Models;

namespace Pixeval.Database.Managers;

public class DownloadHistoryPersistentManager(ILiteDatabase collection, int maximumRecords) : PersistentManagerBase<DownloadHistoryEntry, IDownloadTaskGroup>(collection, maximumRecords)
{
    protected override IDownloadTaskGroup ToModel(DownloadHistoryEntry entry)
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
