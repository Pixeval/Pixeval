// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Misaki;
using Pixeval.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IArtworkInfo, IDownloadTaskGroup, object>
{
    public IDownloadTaskGroup Create(IArtworkInfo context, string rawPath, object? parameter = null)
    {
        var path = IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(rawPath, context));
        path = IoHelper.ChangeExtension(path, context.ImageType is ImageType.SingleAnimatedImage
            ? IoHelper.GetUgoiraExtension()
            : IoHelper.GetIllustrationExtension());

        IDownloadTaskGroup task = context switch
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
            _ => throw new NotSupportedException()
        };

        return task;
    }
}
