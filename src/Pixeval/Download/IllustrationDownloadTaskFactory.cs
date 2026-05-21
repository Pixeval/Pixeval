// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IArtworkInfo, IDownloadTaskGroup, object>
{
    public IDownloadTaskGroup Create(IArtworkInfo context, string rawPath, object? parameter = null)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(rawPath, context));
        _ = manager.Delete(entry => entry.Destination == path);

        var task = context switch
        {
            ISingleImage { ImageType: ImageType.SingleImage } singleImage => new SingleImageDownloadTaskGroup(
                singleImage, path),
            ISingleImage { ImageType: ImageType.ImageSet, SetIndex: > -1 } singleImage =>
                new SingleImageDownloadTaskGroup(singleImage, path),
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.MultiFiles
            } singleAnimatedImage => new UgoiraDownloadTaskGroup(singleAnimatedImage, path),
            ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.SingleFile
                or SingleAnimatedImageType.SingleZipFile
            } singleAnimatedImage => new SingleAnimatedImageDownloadTaskGroup(singleAnimatedImage, path),
            IImageSet { ImageType: ImageType.ImageSet } imageSet => new MangaDownloadTaskGroup(imageSet, path),
            _ => ThrowHelper.ThrowNotSupportedException<IDownloadTaskGroup>()
        };

        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
