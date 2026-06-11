// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Misaki;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IArtworkInfo, IDownloadTaskGroup, object>
{
    public IDownloadTaskGroup Create(IArtworkInfo context, string rawPath, object? parameter = null) =>
        Create(new ParserContext(context), rawPath, parameter);

    public IDownloadTaskGroup Create(ParserContext parserContext, string rawPath, object? parameter = null)
    {
        var context = parserContext.ArtworkInfo;
        var path = IoHelper.NormalizePath(DownloadPathMacroParser.Reduce(rawPath, parserContext));

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
