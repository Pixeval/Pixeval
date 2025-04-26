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

        IDownloadTaskGroup? task;
        switch (context)
        {
            case ISingleImage { ImageType: ImageType.SingleImage } singleImage:
            {
                task = new SingleImageDownloadTaskGroup(singleImage, path);
                break;
            }
            case ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.MultiFiles
            } singleAnimatedImage:
            {
                task = new UgoiraDownloadTaskGroup(singleAnimatedImage, path);
                break;
            }
            case ISingleAnimatedImage
            {
                ImageType: ImageType.SingleAnimatedImage,
                PreferredAnimatedImageType: SingleAnimatedImageType.SingleFile or SingleAnimatedImageType.SingleZipFile
            } singleAnimatedImage:
            {
                task = new SingleAnimatedImageDownloadTaskGroup(singleAnimatedImage, path);
                break;
            }
            case IImageSet { ImageType: ImageType.ImageSet } imageSet:
            {
                task = new MangaDownloadTaskGroup(imageSet, path);
                break;
            }
            default:
                return ThrowHelper.ThrowNotSupportedException<IDownloadTaskGroup>();
        }

        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
